using ClsNac.Mirror.Figure;
using ClsNac.Mirror.WaveField;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZedGraph;


namespace Focusing1D
{
    public partial class Form1 : Form
    {
        Mirror1D M1;
        Mirror1D M2;

        WaveField1D wMO1;
        WaveField1D wMO2;

        bool flagM2;

        PBS pbs;

        private CancellationTokenSource _cts = null;

        public Form1()
        {
            InitializeComponent();
            this.comboBox_LambdaEnergy.Text = "波長";
            this.nud_coreNum.Maximum = Environment.ProcessorCount;
            this.nud_coreNum.Value = this.nud_coreNum.Maximum;
        }



        private void button_FigCalc_Click(object sender, EventArgs e)
        {
            try
            {
                FigCalc();

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        void FigCalc()
        {
            var mtype = mType.Ell;
            var mdiv = this.checkBox_DivAngle.Checked ? mDiv.Angle : mDiv.Even;
            var mpos = mPos.Lower;

            if (this.rB_PhaseCompensator.Checked)
            {
                
                //後楕円ミラー
                M2 = new Mirror1D(
                    Convert.ToDouble(this.textBox_LSM1.Text),
                    Convert.ToDouble(this.textBox_LM1F1.Text),
                    Convert.ToDouble(this.textBox_M1theta.Text),
                    Convert.ToInt32(this.textBox_DivM1.Text),
                    Convert.ToDouble(this.textBox_M1L.Text),
                    mtype, mdiv, mpos);
                
                //前形状可変ミラー
                
            }

            else
            {
                if (this.rB_SingleFocus.Checked)
                {

                }
                else if (this.rB_DoubleFocus.Checked)
                {
                }
                
            }


            //1枚目


            M1 = new Mirror1D(
                Convert.ToDouble(this.textBox_LSM1.Text),
                Convert.ToDouble(this.textBox_LM1F1.Text),
                Convert.ToDouble(this.textBox_M1theta.Text),
                Convert.ToInt32(this.textBox_DivM1.Text),
                Convert.ToDouble(this.textBox_M1L.Text),
                mtype, mdiv, mpos);

            M1.ReCalcMirror();

            //Graph
            GraphFig(this.zgc_M1, M1);


            //2枚目
            if (this.rB_DoubleFocus.Checked)
            {

                mpos = this.checkBox_MirrorPosition.Checked ? mPos.Upper : mPos.Lower;

                M2 = new Mirror1D(
                    Convert.ToDouble(this.textBox_LM1M2.Text) - Convert.ToDouble(this.textBox_LM1F1.Text),
                    Convert.ToDouble(this.textBox_LM2F2.Text),
                    Convert.ToDouble(this.textBox_M2theta.Text),
                    Convert.ToInt32(this.textBox_DivM2.Text),
                    Convert.ToDouble(this.textBox_M2L.Text),
                    mtype, mdiv, mpos);

                //Mirror1D.ReCalcMirror(M1, M2);

                //graph
                GraphFig(this.zgc_M2, M2);

                flagM2 = true;
                this.groupBox_F2.Enabled = true;
            }
            else if (this.rB_PhaseCompensator.Checked)
            {
                //M1はDM
                //M2はEllipse

                //M1をM2にコピー
                M2 = M1;
                
                //DM設定
                mpos = this.checkBox_MirrorPosition.Checked ? mPos.Upper : mPos.Lower;

                

                flagM2 = true;
                this.groupBox_F2.Enabled = false;
            }
            else
            {
                flagM2 = false;
                this.groupBox_F2.Enabled = false;
            }

        }

        void FigCalcNA()
        {
            //fullNAを計算

            //
        }

        private async void button_WaveOptCalc_Click(object sender, EventArgs e)
        {

            try
            {
                #region cancel
                if (this._cts == null)
                {
                    this._cts = new CancellationTokenSource();
                }
                else
                {
                    this._cts.Cancel(true);
                    return;
                }
                #endregion

                var progress = new Progress<ProgressInfo>(this.ProgressReport);

                if (this.tabControl_WO.SelectedIndex == 0)
                {

                    if (M1 == null || (flagM2 && M2 == null))
                        return;

                    this.button_WaveOptCalc.Text = "停止";

                    wMO1 = new WaveField1D();
                    wMO2 = new WaveField1D();

                    if (this.comboBox_LambdaEnergy.Text == "波長")
                    {
                        wMO1.lambda = 1e-10 * Convert.ToDouble(this.textBox_WavelengthEnergy.Text);
                        wMO2.lambda = 1e-10 * Convert.ToDouble(this.textBox_WavelengthEnergy.Text);
                    }
                    else
                    {
                        wMO1.Energy = 1e3 * Convert.ToDouble(this.textBox_WavelengthEnergy.Text);
                        wMO2.Energy = 1e3 * Convert.ToDouble(this.textBox_WavelengthEnergy.Text);
                    }


                    wMO1.ct = this._cts.Token;
                    wMO2.ct = this._cts.Token;
                    await Task.Run(() => WaveOptics(Convert.ToInt32(this.nud_coreNum.Value), progress)).ContinueWith(t =>
                    {
                        this._cts.Dispose();
                        this._cts = null;
                        return;
                    });

                    this.GraphWaveM(this.zgc_M1w, M1.m);
                    this.PlotFocus(this.pictureBox_F1, M1.fw);
                    this.GraphWaveF(this.zgc_F1, M1.fw, M1.fw.iMinFWHM1);

                    if (flagM2)
                    {
                        this.GraphWaveM(this.zgc_M2w, M2.m);
                        this.PlotFocus(this.pictureBox_F2, M2.fw);
                        this.GraphWaveF(this.zgc_F2, M2.fw, M2.fw.iMinFWHM1);
                    }
                }
                else
                {
                    ExecutePBS(Convert.ToInt32(this.nud_coreNum.Value), progress);
                    //await Task.Run(() => ExecutePBS(Convert.ToInt32(this.nud_coreNum.Value), progress)).ContinueWith(t =>
                    //{
                    //    this._cts.Dispose();
                    //    this._cts = null;
                    //    return;
                    //});
                    this._cts = null;
                }




            }
            finally
            {

                this.button_WaveOptCalc.Text = "波動光学計算";
            }
        }

        void WaveOptics(int _core, IProgress<ProgressInfo> progress)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            try
            {
                sw.Start();

                #region Mirror1

                //光源設定
                if (this.radioButton_PointSource.Checked)
                    this.M1.Source(1, 0);
                else if(this.radioButton_GaussSource.Checked)
                {
                    this.M1.Source(Convert.ToInt16(this.textBox_SourceDivY.Text),
                        Convert.ToDouble(this.textBox_SourceSizeY.Text),
                        true);
                }
                else
                    this.M1.Source(Convert.ToInt16(this.textBox_SourceDivY.Text),
                        Convert.ToDouble(this.textBox_SourceSizeY.Text) / Convert.ToInt16(this.textBox_SourceDivY.Text));

                //焦点設定
                //ミラーの座標から焦点位置を決定
                M1.Detector(Convert.ToInt16(this.textBox_Detector1nx.Text), Convert.ToDouble(this.textBox_Detector1dx.Text),
                    Convert.ToInt16(this.textBox_Detector1ny.Text), Convert.ToDouble(this.textBox_Detector1dy.Text),
                    Convert.ToDouble(this.textBox_Detector1bx.Text), Convert.ToDouble(this.textBox_Detector1by.Text));

                //コンストラクタ
                wMO1.Initialize(M1.s, M1.m, M1.fw);

                //実行
                wMO1.Execute(_core, progress);

                #endregion

                #region Mirror2
                if (flagM2)
                {
                    this.M2.Source(Convert.ToInt16(this.textBox_Detector1ny.Text), Convert.ToDouble(this.textBox_Detector1dy.Text));
                    for (int i = 0; i < this.M2.s.div; i++)
                    {
                        this.M2.s.u[i] = this.M1.fw.u[this.M1.fw.n / 2][i];
                    }

                    //光源(=第一焦点)を設定
                    //M2.s = M1.fw.ToCoord();

                    //焦点を設定
                    //ミラーの座標から焦点位置を決定
                    M2.Detector(Convert.ToInt16(this.textBox_Detector2nx.Text), Convert.ToDouble(this.textBox_Detector2dx.Text),
                        Convert.ToInt16(this.textBox_Detector2ny.Text), Convert.ToDouble(this.textBox_Detector2dy.Text),
                        Convert.ToDouble(this.textBox_Detector2bx.Text), Convert.ToDouble(this.textBox_Detector2by.Text));

                    //コンストラクタ
                    wMO2.Initialize(M2.s, M2.m, M2.fw);

                    //実行
                    wMO2.Execute(_core, progress);

                }
                #endregion
            }
            finally
            {

                sw.Stop();
                progress.Report(new ProgressInfo(0, string.Format("終了({0} msec)", sw.ElapsedMilliseconds)));
            }
        }

        void ExecutePBS(int _core,IProgress<ProgressInfo> progress)
        {
            //光源設定
            if (this.radioButton_PointSource.Checked)
                this.M1.Source(1, 0);
            else
                this.M1.Source(Convert.ToInt16(this.textBox_SourceDivY.Text),
                    Convert.ToDouble(this.textBox_SourceSizeY.Text) / Convert.ToInt16(this.textBox_SourceDivY.Text));


            pbs = new PBS(
                Convert.ToInt32(this.textBox_PbsSlitN.Text),
                Convert.ToDouble(this.textBox_PbsSlitX.Text),
                Convert.ToDouble(this.textBox_PbsSlitOL.Text),
                Convert.ToInt32(this.textBox_PbsSlitDiv.Text),
                Convert.ToDouble(this.textBox_PbsSlitOL.Text) / 100.0);
            
            if (this.comboBox_LambdaEnergy.Text == "波長")
            {
                pbs .lambda = 1e-10 * Convert.ToDouble(this.textBox_WavelengthEnergy.Text);
            }
            else
            {
                pbs.Energy = 1e3 * Convert.ToDouble(this.textBox_WavelengthEnergy.Text);
            }

            //光源設定
            if (this.radioButton_PointSource.Checked)
                this.M1.Source(1, 0);
            else
                this.M1.Source(Convert.ToInt16(this.textBox_SourceDivY.Text),
                    Convert.ToDouble(this.textBox_SourceSizeY.Text) / Convert.ToInt16(this.textBox_SourceDivY.Text));
            
            //焦点設定
            //ミラーの座標から焦点位置を決定
            M1.Detector(1, 0.0,
                Convert.ToInt16(this.textBox_PbsD1ny.Text), Convert.ToDouble(this.textBox_PbsD1dy.Text),
                Convert.ToDouble(this.textBox_PbsD1bx.Text), Convert.ToDouble(this.textBox_PbsD1by.Text));

            //光源(=第一焦点)を設定
            M2.s = M1.fw.ToCoord();

            //焦点を設定
            //ミラーの座標から焦点位置を決定
            M2.Detector(1, 0.0,
                Convert.ToInt16(this.textBox_PbsD2ny.Text), Convert.ToDouble(this.textBox_PbsD2dy.Text),
               Convert.ToDouble(this.textBox_PbsD2bx.Text), Convert.ToDouble(this.textBox_PbsD2by.Text));

            pbs.setSource(M1.s.div, M1.s.x, M1.s.y, M1.s.u);
            //pbs.setCoord(pbs.wM1, M1.mw.div, M1.mw.x, M1.mw.y);
            pbs.setCoord(pbs.wM1, M1.m.div, M1.m.x, M1.m.y);
            pbs.setCoord(pbs.wF1, M1.fw.div, M1.fw.x[0], M1.fw.y[0]);
            //pbs.setCoord(pbs.wM2, M2.mw.div, M2.mw.x, M2.mw.y);
            pbs.setCoord(pbs.wM2, M2.m.div, M2.m.x, M2.m.y);
            pbs.setCoord(pbs.wF2, M2.fw.div, M2.fw.x[0], M2.fw.y[0]);

            pbs.Execute(_core);

            #region graph

            GraphPBS(this.zgc_PbsF1, M1.fw.xmod, pbs.wF1);
            GraphPBS(this.zgc_PbsF2, M2.fw.xmod, pbs.wF2);
            GraphPBSM(this.zgc_PbsF1Med,  pbs.mF1);
            GraphPBSM(this.zgc_PbsF2Med,  pbs.mF2);
            #endregion
        }



        class PBS
        {
            const double h = 6.62607e-34;
            const double e = 1.602e-19;
            const double c = 2.99792458e8;

            public double lambda { get; set; }
            public double Energy
            {
                get { return h * c / (e * lambda); }
                set { lambda = h * c / (e * value); }
            }

            double tShold;
            int slitN;
            double slitX;
            double slitOL;
            int slitDiv;

            public WaveField wS;
            public WaveField[] wSlit;
            public WaveField[] wM1;
            public WaveField[] wF1;
            public WaveField[] wM2;
            public WaveField[] wF2;

            public double[] mF1;
            public double[] mF2;

            public PBS(int _slitN,double _slitX,double _slitOL,int _slitDiv,double _tShold)
            {
                slitN = _slitN;
                slitX = _slitX;
                slitOL = _slitOL;
                slitDiv = _slitDiv;
                tShold = _tShold;

                wSlit = new WaveField[slitN];
                wM1 = new WaveField[slitN];
                wF1 = new WaveField[slitN];
                wM2 = new WaveField[slitN];
                wF2 = new WaveField[slitN];

            }

            public void setSource(int div, double[] x, double[] y,System.Numerics.Complex[] u)
            {
                wS = new WaveField(div, lambda);
                wS.x = x;
                wS.y = y;
                wS.u = u;
            }

            public void setCoord(WaveField[] wf, int div, double[] x, double[] y)
            {
                for (int i = 0; i < wf.Length; i++)
                {
                    wf[i] = new WaveField(div, lambda);
                    wf[i].x = x;
                    wf[i].y = y;
                }
            }

            public void Execute(int core)
            {
                //スリット位置決定
                //(wS.xc,wS.yc)と{(wM1[0].x[0],wM1[0].y[0]),(wM1[0].x[n],wM1[0].y[n])}の一次方程式
                int div = wS.x.Length;
                int divM = wM1[0].x.Length;
                slitX = wM1[0].x[divM / 2] - slitX;

                double a0 = (wM1[0].y[0] - wS.y[div / 2]) / (wM1[0].x[0] - wS.x[div / 2]);
                double b0 = wS.y[div / 2] - a0 * wS.x[div / 2];
                double slitY0 = a0 * slitX + b0;

                double an = (wM1[0].y[divM - 1] - wS.y[div / 2]) / (wM1[0].x[divM - 1] - wS.x[div / 2]);
                double bn = wS.y[div / 2] - an * wS.x[div / 2];
                double slitYn = an * slitX + bn;

                slitY0 += (slitYn - slitY0) / (slitN - 1) / 2.0;
                slitYn -= (slitYn - slitY0) / (slitN - 1) / 2.0;
                

                double slitDY = (slitYn - slitY0) / (slitN * slitDiv - 1);

                

                for (int i = 0; i < slitN; i++)
                {
                    wSlit[i] = new WaveField(slitDiv, lambda);
                    for (int j = 0; j < slitDiv; j++)
                    {
                        wSlit[i].x[j] = slitX;
                        wSlit[i].y[j] = slitDY * (i * slitDiv + j) + slitY0;
                    }
                }
                //

                
                for (int i = 0; i < slitN; i++)
                {
                    //_wS -> wSlit[i]
                    wSlit[i].ForwardPropagation(wS, core);
                    //wSlit[i] -> wM1[i]
                    wM1[i].ForwardPropagation(wSlit[i], core);
                    //wM1[i] -> wF1[i]
                    wF1[i].ForwardPropagation(wM1[i], core);
                    //wF1[i] -> wM2[i]
                    wM2[i].ForwardPropagation(wF1[i], core);
                    //wM2[i] -> wF2[i]
                    wF2[i].ForwardPropagation(wM2[i], core);

                }

                //重心計算
                mF1 = new double[slitN];
                mF2 = new double[slitN];
                    double avrg1 = 0.0;
                    double avrg2 = 0.0;

                for (int i = 0; i < slitN; i++)
                {
                    mF1[i] = 0.0;
                    mF2[i] = 0.0;


                    double sum1 = 0.0;
                    double sum2 = 0.0;

                    //max探す
                    double max = 0.0;
                    for (int j = 0; j < wF1[i].x.Length; j++)
                    {
                        if (max < wF1[i].Intensity[j]) max = wF1[i].Intensity[j];
                    }
                    max *= tShold;
                    for (int j = 0; j < wF1[i].x.Length; j++)
                    {
                        double intensity = wF1[i].Intensity[j] - max < 0.0 ? 0.0 : wF1[i].Intensity[j] - max;
                        mF1[i] += intensity * Math.Sqrt(Math.Pow(wF1[i].y[j] - wF1[i].y[0], 2.0) + Math.Pow(wF1[i].x[j] - wF1[i].x[0], 2.0));
                        sum1 += wF1[i].Intensity[j];
                    }
                    max = 0.0;
                    for (int j = 0; j < wF2[i].Intensity.Length; j++)
                    {
                        if (max < wF2[i].Intensity[j]) max = wF2[i].Intensity[j];
                    }
                    max *= tShold;
                    for (int j = 0; j < wF2[i].x.Length; j++)
                    {
                        double intensity = wF2[i].Intensity[j] - max < 0.0 ? 0.0 : wF2[i].Intensity[j] - max;
                        mF2[i] += intensity * Math.Sqrt(Math.Pow(wF2[i].y[j] - wF2[i].y[0], 2.0) + Math.Pow(wF2[i].x[j] - wF2[i].x[0], 2.0));
                        sum2 += wF2[i].Intensity[j];
                    }
                    mF1[i] /= sum1;
                    mF2[i] /= sum2;
                    avrg1 += mF1[i];
                    avrg2 += mF2[i];
                }
                for(int i=0;i<slitN;i++)
                {
                    mF1[i] -= avrg1 / (double)mF1.Length;
                }
                for (int i = 0; i < mF2.Length; i++)
                {
                    mF2[i] -= avrg2 / (double)mF2.Length;
                }
            }

            public void Output(string _fbd)
            {
                double[,] slit = new double[slitN, slitDiv];
                for (int i = 0; i < slitN; i++)
                    for (int j = 0; j < slitDiv; j++)
                        slit[i, j] = wSlit[i].Intensity[j];
                ClsNac.FileIO.FileIO.writeFile(_fbd + "\\slit.txt", slit);
                ClsNac.FileIO.FileIO.writeFile(_fbd + "\\m1.txt", outputData(wM1));
                ClsNac.FileIO.FileIO.writeFile(_fbd + "\\f1.txt", outputData(wF1));
                ClsNac.FileIO.FileIO.writeFile(_fbd + "\\m2.txt", outputData(wM2));
                ClsNac.FileIO.FileIO.writeFile(_fbd + "\\f2.txt", outputData(wF2));
                ClsNac.FileIO.FileIO.writeFile(_fbd + "\\mf1.txt", mF1);
                ClsNac.FileIO.FileIO.writeFile(_fbd + "\\mf2.txt", mF2);
            }

            double[,] outputData(WaveField[] wf)
            {
                double[,] data = new double[wf.Length, wf[0].Intensity.Length];
                for(int i=0;i<slitN;i++)
                {
                    for(int j=0;j<wf[0].Intensity.Length;j++)
                    {
                        data[i, j] = wf[i].Intensity[j];
                    }
                }
                return data;
            }
        }


        void ProgressReport(ProgressInfo info)
        {
            this.toolStripProgressBar.Value = info.Value;
            this.toolStripStatusLabel.Text = info.Message;
        }

        private void comboBox_LambdaEnergy_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.label_WavelengthEnergy.Text = this.comboBox_LambdaEnergy.SelectedIndex == 0 ? "[Å]" : "[keV]";
        }



        #region graph

        enum AngleCurv { IncAngle, Curv }
        AngleCurv ac;

        /// <summary>形状グラフ表示</summary>
        private void GraphFig(ZedGraphControl zgcFig, Mirror1D Mirror)
        {
            GraphPane myPane = zgcFig.GraphPane;
            myPane.CurveList.Clear();
            LineItem myLine = myPane.AddCurve("Mirror shape", Mirror.m.xmod, Mirror.m.ymod, Color.Red, SymbolType.None);
            LineItem myLine2 = myPane.AddCurve("Mirror shape2", Mirror.mw.xmod2, Mirror.mw.ymod, Color.Green,SymbolType.None);
            LineItem myLineInc = ac == AngleCurv.Curv ?
                myPane.AddCurve("Curvature", Mirror.m.xmod, Mirror.m.curv, Color.Blue, SymbolType.None) :
                myPane.AddCurve("Incident angle", Mirror.m.xmod, Mirror.m.theta, Color.Blue, SymbolType.None);

            myLineInc.IsY2Axis = true;

            myPane.Title.Text = zgcFig.Name.Substring(4) + " NA:" + (Mirror.NA * 1e6).ToString("F1");

            myPane.XAxis.Title.Text = "Position (mm)";
            myPane.XAxis.Scale.Max = Mirror.ML;
            myPane.XAxis.Scale.Min = 0.0;
            myPane.XAxis.Scale.MaxAuto = false;
            myPane.XAxis.Scale.MinAuto = false;
            myPane.XAxis.Scale.Mag = -3;
            myPane.XAxis.Title.IsOmitMag = true;

            myPane.YAxis.Title.Text = "Figure (μm)";
            myPane.YAxis.Scale.Mag = -6;
            myPane.YAxis.Title.IsOmitMag = true;

            myPane.Y2Axis.IsVisible = true;
            myPane.Y2Axis.Title.Text = ac == AngleCurv.Curv ? "Curvature(m)" : "Angle (mrad)";
            myPane.Y2Axis.Scale.Mag = ac == AngleCurv.Curv ? 0 : -3;
            myPane.Y2Axis.Title.IsOmitMag = true;

            zgcFig.AxisChange();
            zgcFig.Refresh();
        }

        private void rB_IncAngle_CheckedChanged(object sender, EventArgs e)
        {
            if(rB_IncAngle.Checked)
            {
                ac = AngleCurv.IncAngle;
                GraphFig(this.zgc_M1, M1);
                if (flagM2)
                    GraphFig(this.zgc_M2, M2);
            }
        }

        private void rB_Curv_CheckedChanged(object sender, EventArgs e)
        {
            if(rB_Curv.Checked)
            {
                ac = AngleCurv.Curv;
                GraphFig(this.zgc_M1, M1);
                if (flagM2)
                    GraphFig(this.zgc_M2, M2);

            }
        }

        /// <summary>
        /// ミラー強度位相分布
        /// </summary>
        /// <param name="zgc"></param>
        /// <param name="_CoordM"></param>
        private void GraphWaveM(ZedGraph.ZedGraphControl zgc, ClsNac.Mirror.CoordM _CoordM)
        {
            GraphPane myPane = zgc.GraphPane;
            myPane.CurveList.Clear();
            LineItem lineIntens = myPane.AddCurve("Intensity", _CoordM.xmod, _CoordM.Intensity, Color.Red, SymbolType.None);
            LineItem linePhase = myPane.AddCurve("Phase", _CoordM.xmod, _CoordM.Phase, Color.Blue, SymbolType.None);
            linePhase.IsY2Axis = true;

            myPane.XAxis.Title.Text = "Position (mm)";
            myPane.XAxis.Scale.Mag = -3;
            myPane.XAxis.Title.IsOmitMag = true;

            myPane.YAxis.Title.Text = "Intensity (a.u.)";

            myPane.Y2Axis.IsVisible = true;
            myPane.Y2Axis.Title.Text = "Phase (rad)";

            

            zgc.AxisChange();
            zgc.Refresh();
        }

        /// <summary>
        /// 焦点グラフプロット
        /// </summary>
        /// <param name="pictureBox"></param>
        /// <param name="fw"></param>
        private void PlotFocus(PictureBox pictureBox, ClsNac.Mirror.CoordF fw)
        {
            ClsNac.Graphic.Plot2dPlane myPlane = new ClsNac.Graphic.Plot2dPlane(pictureBox);
            myPlane.Draw(fw.Intensity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zgc"></param>
        /// <param name="_CoordF"></param>
        private void GraphWaveF(ZedGraphControl zgc, ClsNac.Mirror.CoordF _CoordF, int i = 0)
        {
            GraphPane myPane = zgc.GraphPane;
            myPane.CurveList.Clear();
            LineItem lineIntens = myPane.AddCurve("Intensity", _CoordF.ymod, _CoordF.IntensityJagged[i], Color.Red);

            myPane.XAxis.Scale.Min = _CoordF.ymod[0];
            myPane.XAxis.Scale.Max = _CoordF.ymod[_CoordF.div - 1];
            myPane.XAxis.Title.Text = "Position (µm)";
            myPane.XAxis.Scale.Mag = -6;
            myPane.XAxis.Title.IsOmitMag = true;

            myPane.YAxis.Title.Text = "Intensity (a.u.)";

            myPane.Title.Text = (1e9 * _CoordF.FWHM1[i]).ToString("F0") + " (nm) 焦点ずれ: " + (1e6 * _CoordF.dx * (i - (_CoordF.n - 1) / 2.0)).ToString("F0") + " (um)";

            zgc.AxisChange();
            zgc.Refresh();
        }

        private void GraphPBS(ZedGraphControl zgc,double[] x,WaveField[] wf)
        {
            GraphPane myPane = zgc.GraphPane;
            myPane.CurveList.Clear();
            int seed = Environment.TickCount;
            for(int i=0;i<wf.Length;i++)
            {
                Random rdm = new Random(seed++);
                int iR = rdm.Next(256);
                int iG = rdm.Next(256);
                int iB = rdm.Next(256);
                Color color = Color.FromArgb(iR, iG, iB);
                myPane.AddCurve("PBS " + i.ToString(), x, wf[i].Intensity, color, SymbolType.None);
            }
            myPane.YAxis.IsVisible = false;
            myPane.XAxis.Scale.Max = wf[0].Intensity.Length;
            myPane.XAxis.Title.IsVisible = false;
            myPane.Title.IsVisible = false;
            zgc.AxisChange();
            zgc.Refresh();
        }

        private void GraphPBSM(ZedGraphControl zgc, double[] med)
        {
            double[] x=new double[med.Length];
            for (int i = 0; i < med.Length; i++)
                x[i] = i;
            GraphPane myPane = zgc.GraphPane;
            myPane.CurveList.Clear();
            myPane.AddCurve("Median Point", x, med, Color.Red);
            myPane.Title.IsVisible = false;
            myPane.XAxis.Scale.Max = med.Length-1;
            myPane.XAxis.Title.IsVisible = false;
            myPane.Legend.IsVisible = false;
            zgc.AxisChange();
            zgc.Refresh();
        }

        #endregion

        #region error
        private void button_EM1_Fig_Click(object sender, EventArgs e)
        {
            if(this.ofd_EM1.ShowDialog()==DialogResult.OK)
            {
                double[] subError = null;
                ClsNac.FileIO.FileIO.readFile(this.ofd_EM1.FileName, ref subError);
                M1.PlusError(subError);
            }
        }

        private void button_EM2_Fig_Click(object sender, EventArgs e)
        {
            if (this.ofd_EM2.ShowDialog() == DialogResult.OK)
            {
                double[] subError = null;
                ClsNac.FileIO.FileIO.readFile(this.ofd_EM2.FileName, ref subError);
                M2.PlusError(subError);
            }

        }
        #endregion

        #region output
        private void button_FigOutput_Click(object sender, EventArgs e)
        {
            if (this.fbd_FigOutput.ShowDialog() == DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("LSM1\t" + this.textBox_LSM1.Text);
                sb.AppendLine("LM1F1\t" + this.textBox_LM1F1.Text);
                sb.AppendLine("M1L\t" + this.textBox_M1L.Text);
                sb.AppendLine("M1theta\t" + this.textBox_M1theta.Text);

                ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror1_FigY.txt",  M1.m.ymod);
                ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror1_Fig.txt", M1.m.xmod, M1.m.ymod);
                ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror1_inc.txt", M1.m.xmod, M1.m.theta);
                ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror1_FigW.txt", M1.mw.xmod, M1.mw.ymod);
                ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror1_FigRaw.txt", M1.m.x, M1.m.y);

                if (this.flagM2)
                {
                    sb.AppendLine();
                    sb.AppendLine("LM1M2\t" + this.textBox_LM1M2.Text);
                    sb.AppendLine("LM2F2\t" + this.textBox_LM2F2.Text);
                    sb.AppendLine("M2L\t" + this.textBox_M2L.Text);
                    sb.AppendLine("M2theta\t" + this.textBox_M2theta.Text);

                    ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror2_Fig.txt", M2.m.xmod, M2.m.ymod);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror2_FigY.txt", M2.m.ymod);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror2_inc.txt", M2.m.xmod, M2.m.theta);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror2_FigW.txt", M2.mw.xmod, M2.mw.ymod);

                }
                System.IO.File.WriteAllText(this.fbd_FigOutput.SelectedPath + "\\setting.txt", sb.ToString());
            }
        }

        private void button_DetectorOutput_Click(object sender, EventArgs e)
        {
            if (this.fbd_Detector.ShowDialog() == DialogResult.OK)
            {
                if (this.tabControl_WO.SelectedIndex == 0)
                {
                    ClsNac.FileIO.FileIO.writeFile(this.fbd_Detector.SelectedPath + "\\fx.txt", M1.fw.x[0]);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd_Detector.SelectedPath + "\\fy.txt", M1.fw.y[0]);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd_Detector.SelectedPath + "\\m1.txt", M1.m.Intensity);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd_Detector.SelectedPath + "\\m2.txt", M2.m.Intensity);

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("FWHM=" + this.zgc_F1.GraphPane.Title.Text);

                    DetectorOutput(this.fbd_Detector.SelectedPath + "\\Detector1.txt", this.M1.fw);
                    if (flagM2)
                    {
                        DetectorOutput(this.fbd_Detector.SelectedPath + "\\Detector2.txt", this.M2.fw);
                        sb.AppendLine("FWHM=" + this.zgc_F2.GraphPane.Title.Text);
                    }
                    System.IO.File.WriteAllText(this.fbd_Detector.SelectedPath + "\\fwhm.txt", sb.ToString());
                }
                else
                {
                    pbs.Output(this.fbd_Detector.SelectedPath);
                }
            }
        }

        void DetectorOutput(string Path, ClsNac.Mirror.CoordF wf)
        {
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < wf.Intensity.GetLength(1); j++)
            {
                for (int i = 0; i < wf.Intensity.GetLength(0); i++)
                {
                    sb.Append(Convert.ToString(wf.Intensity[i, j])).Append(" ");
                }
                sb.AppendLine();
            }
            System.IO.File.WriteAllText(Path, sb.ToString());
        }


        #endregion


        private void toolStripButton_SettingSave_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton_SettingLoad_Click(object sender, EventArgs e)
        {

        }

        #region radiobutton

        private void rB_SingleFocus_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rB_SingleFocus.Checked)
            {
                this.groupBox_M2.Enabled = false;
                this.groupBox_M2E.Enabled = false;
                this.groupBox_M2.Text = "ミラー２";
                this.groupBox_M2E.Text = "ミラー２調整";

                this.textBox_LM2F2.Enabled = true;
            }

        }

        private void rB_DoubleFocus_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rB_DoubleFocus.Checked)
            {
                this.groupBox_M2.Enabled = true;
                this.groupBox_M2E.Enabled = true;
                this.groupBox_M2.Text = "ミラー２";
                this.groupBox_M2E.Text = "ミラー２調整";

                this.textBox_LM2F2.Enabled = true;
            }
        }

        private void rB_PhaseCompensator_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rB_PhaseCompensator.Checked)
            {
                this.groupBox_M2.Enabled = true;
                this.groupBox_M2E.Enabled = true;
                this.groupBox_M2.Text = "形状可変ミラー";
                this.groupBox_M2E.Text = "形状可変ミラー調整";

                this.textBox_LM2F2.Enabled = false;


            }
        }

        #endregion
    }
}
