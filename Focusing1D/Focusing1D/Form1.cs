using System;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZedGraph;
using System.Xml.Serialization;
using ClsNac.Mirror1D;
using System.IO;
namespace Focusing1D
{
    public partial class Form1 : Form
    {
        Mirror1D M1;
        Mirror1D M2;
        Mirror1D.Parameter pm1;
        Mirror1D.Parameter pm2;

        bool flagM2;
        bool flagF1;

        XmlSerializer serializer = new XmlSerializer(typeof(Mirror1D.Parameter));
        FileStream fs;

        private CancellationTokenSource _cts = null;

        public Form1()
        {
            InitializeComponent();
            this.comboBox_LambdaEnergy.Text = "エネルギー";
            this.nud_coreNum.Maximum = Environment.ProcessorCount;
            this.nud_coreNum.Value = this.nud_coreNum.Maximum;

            this.rB_SingleFocus.Checked = true;
            //this.rB_DoubleFocus.Checked = true;
            this.radioButton_PointSource.Checked = true;
        }



        private void button_FigCalc_Click(object sender, EventArgs e)
        {
            try
            {
                FigCalc();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void FigCalc()
        {
            if (this.rB_PhaseCompensator.Checked)
            {
                this.flagF1 = false;
                this.flagM2 = true;

                //前形状可変ミラー
                pm1 = new Mirror1D.Parameter();
                pm1.L1 = Convert.ToDouble(this.textBox_LSM1.Text) - Convert.ToDouble(this.textBox_LM1M2.Text);
                pm1.L2 = Convert.ToDouble(this.textBox_LM1M2.Text);
                pm1.theta_i = Convert.ToDouble(this.textBox_M2theta.Text);
                pm1.divML = Convert.ToInt32(this.textBox_DivM2.Text);
                pm1.ML = Convert.ToDouble(this.textBox_M2L.Text);
                pm1.mirrorType = Mirror1D.Parameter.MirrorType.Plane;
                pm1.pos = Mirror1D.Parameter.Pos.lower;
                M1 = new Mirror1D(pm1);

                //後ろ楕円ミラー
                pm2 = new Mirror1D.Parameter();
                pm2.L1 = Convert.ToDouble(this.textBox_LSM1.Text);
                pm2.L2 = Convert.ToDouble(this.textBox_LM1F1.Text);
                pm2.theta_i = Convert.ToDouble(this.textBox_M1theta.Text);
                pm2.divML = Convert.ToInt32(this.textBox_DivM1.Text);
                pm2.ML = Convert.ToDouble(this.textBox_M1L.Text);
                pm2.mirrorType = Mirror1D.Parameter.MirrorType.Ellipse;
                pm2.pos = this.checkBox_MirrorPosition.Checked
                    ? Mirror1D.Parameter.Pos.upper
                    : Mirror1D.Parameter.Pos.lower;
                M2 = new Mirror1D(pm2);
            }

            else
            {
                //1枚目
                this.flagF1 = true;
                this.flagM2 = false;

                pm1 = new Mirror1D.Parameter();
                pm1.L1 = Convert.ToDouble(this.textBox_LSM1.Text);
                pm1.L2 = Convert.ToDouble(this.textBox_LM1F1.Text);
                pm1.theta_i = Convert.ToDouble(this.textBox_M1theta.Text);
                pm1.divML = Convert.ToInt32(this.textBox_DivM1.Text);
                pm1.ML = Convert.ToDouble(this.textBox_M1L.Text);
                pm1.mirrorType = Mirror1D.Parameter.MirrorType.Ellipse;
                pm1.pos = Mirror1D.Parameter.Pos.lower;
                M1 = new Mirror1D(pm1);

                if (this.rB_DoubleFocus.Checked)
                {
                    //2枚目
                    this.flagM2 = true;

                    pm2 = new Mirror1D.Parameter();
                    pm2.L1 = Convert.ToDouble(this.textBox_LM1M2.Text) - Convert.ToDouble(this.textBox_LM1F1.Text);
                    pm2.L2 = Convert.ToDouble(this.textBox_LM2F2.Text);
                    pm2.theta_i = Convert.ToDouble(this.textBox_M2theta.Text);
                    pm2.divML = Convert.ToInt32(this.textBox_DivM2.Text);
                    pm2.ML = Convert.ToDouble(this.textBox_M2L.Text);
                    pm2.mirrorType = Mirror1D.Parameter.MirrorType.Ellipse;
                    pm2.pos = this.checkBox_MirrorPosition.Checked
                        ? Mirror1D.Parameter.Pos.upper
                        : Mirror1D.Parameter.Pos.lower;
                    M2 = new Mirror1D(pm2);

                    
                }

            }

            #region 座標補正
            if (flagF1 && flagM2)
            {
                //第一焦点有り，二枚目ミラー有り
                //二段光学系の場合
                //後段光学系の移動
                M2.Rot(M1.pm.theta_f - M2.pm.theta_s);
                M2.Move(2.0 * M1.pm.f, 0);
}
            else if (!flagF1 && flagM2)
            {
                //第一焦点無し，二枚目ミラー有り
                //PhaseCompensatorの場合
                //回転
                M2.Rot(M1.pm.theta_s - M2.pm.theta_s, 0.0, 0.0);
                M2.Rot(2.0 * M1.pm.theta_i, M1.m.xc, M1.m.yc);
            }
            else
            {
                //一段光学系の場合
                //何もしない
            }
            #endregion

            //1枚目


            //Graph
            GraphFig(this.zgc_M1, M1);
            if (flagM2)
                GraphFig(this.zgc_M2, M2);

        }


        private async void button_WaveOptCalc_Click(object sender, EventArgs e)
        {
            #region 入射角誤差
            //ミラー1回転
            double theta1e = Convert.ToDouble(textBox_M1E_Theta.Text);
            M1.m.Rot(theta1e, M1.m.xc, M1.m.yc);


            #endregion


            #region 光源設定
            Mirror1D.Parameter.SourceType sType;
            if (this.radioButton_GaussSource.Checked)
                sType = Mirror1D.Parameter.SourceType.Gaussian;
            else if (this.radioButton_RectangleSource.Checked)
                sType = Mirror1D.Parameter.SourceType.Rectangle;
            else
                sType = Mirror1D.Parameter.SourceType.Point;

            M1.SetSource(sType, Convert.ToDouble(this.textBox_SourceSizeY.Text), Convert.ToInt32(this.textBox_SourceDivY.Text));
            #endregion

            #region 焦点設定
            if (flagF1)
            {
                M1.SetDetector(
                    Convert.ToDouble(this.textBox_Detector1dx.Text), Convert.ToInt32(this.textBox_Detector1nx.Text), Convert.ToDouble(this.textBox_Detector1bx.Text),
                    Convert.ToDouble(this.textBox_Detector1dy.Text), Convert.ToInt32(this.textBox_Detector1ny.Text), Convert.ToDouble(this.textBox_Detector1by.Text));


            }
            if (flagM2)
                M2.SetDetector(
                    Convert.ToDouble(this.textBox_Detector2dx.Text), Convert.ToInt32(this.textBox_Detector2nx.Text), Convert.ToDouble(this.textBox_Detector2bx.Text),
                    Convert.ToDouble(this.textBox_Detector2dy.Text), Convert.ToInt32(this.textBox_Detector2ny.Text), Convert.ToDouble(this.textBox_Detector2by.Text));

            #endregion


            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            //光源からミラー1
            var wF = new ClsNac.WaveField.WaveField();
            if (this.comboBox_LambdaEnergy.Text == "波長") wF.lambda = 1e-9 * Convert.ToDouble(this.textBox_WavelengthEnergy.Text);
            else wF.Energy = 1e3 * Convert.ToDouble(this.textBox_WavelengthEnergy.Text);

            wF.ForwardPropagation(M1.s, ref M1.m);
            GraphWaveM(this.zgc_M1w, M1.m);

            //ミラー1から焦点1
            if (flagF1)
            {
                for (int i = 0; i < M1.pm.Fnx; i++)
                    wF.ForwardPropagation(M1.m, ref M1.f[i]);

                //探索
                double Max = double.MinValue;
                int imax = 0, jmax = 0;
                for (int i = 0; i < M1.pm.Fnx; i++)
                {
                    for (int j = 0; j < M1.pm.Fny; j++)
                    {
                        if (Max < M1.fIntensity[i, j])
                        {
                            Max = M1.fIntensity[i, j];
                            imax = i;
                            jmax = j;
                        }
                    }
                }
                //textBox_Detector1bx.Text = Convert.ToString(M1.pm.Fbx + M1.pm.Fdx * (imax - M1.pm.Fnx / 2));
                textBox_Detector1by.Text = Convert.ToString(M1.pm.Fby + M1.pm.Fdy * (jmax - M1.pm.Fny / 2));
                //探索

                GraphWaveF(this.zgc_F1, M1, imax);
                //graph
                PlotFocus(this.pictureBox_F1, M1);
            }

            //ミラー2へ
            if (flagM2)
            {
                if (flagF1)
                {
                    //焦点1からミラー2
                    wF.ForwardPropagation(M1.f[M1.pm.Fnx / 2], ref M2.m);
                }
                else
                {
                    //第一焦点がない場合
                    //ミラー1からミラー2
                    wF.ForwardPropagation(M1.m, ref M2.m);
                }

                for (int i = 0; i < M2.pm.Fnx; i++)
                    wF.ForwardPropagation(M2.m, ref M2.f[i]);

                //graph
                GraphWaveM(this.zgc_M2w, M2.m);
                
                PlotFocus(this.pictureBox_F2, M2);

                GraphWaveF(this.zgc_F2, M2);
            }
            else
            {
                GraphClear(this.zgc_M2);
                this.pictureBox_F2.Image = null;
            }
            sw.Stop();

            //FigCalc();


            MessageBox.Show(sw.ElapsedMilliseconds.ToString());
        }

        /*void WaveOptics(int _core, IProgress<ProgressInfo> progress)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

            try
            {
                sw.Start();

                #region Mirror1

                //光源設定
                if (this.radioButton_PointSource.Checked)
                    this.M1.Source(1, 0);
                else if (this.radioButton_GaussSource.Checked)
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
        */


        #region pbs
        /*void ExecutePBS(int _core,IProgress<ProgressInfo> progress)
        //{
        //    //光源設定
        //    if (this.radioButton_PointSource.Checked)
        //        this.M1.Source(1, 0);
        //    else
        //        this.M1.Source(Convert.ToInt16(this.textBox_SourceDivY.Text),
        //            Convert.ToDouble(this.textBox_SourceSizeY.Text) / Convert.ToInt16(this.textBox_SourceDivY.Text));


        //    pbs = new PBS(
        //        Convert.ToInt32(this.textBox_PbsSlitN.Text),
        //        Convert.ToDouble(this.textBox_PbsSlitX.Text),
        //        Convert.ToDouble(this.textBox_PbsSlitOL.Text),
        //        Convert.ToInt32(this.textBox_PbsSlitDiv.Text),
        //        Convert.ToDouble(this.textBox_PbsSlitOL.Text) / 100.0);

        //    if (this.comboBox_LambdaEnergy.Text == "波長")
        //    {
        //        pbs .lambda = 1e-10 * Convert.ToDouble(this.textBox_WavelengthEnergy.Text);
        //    }
        //    else
        //    {
        //        pbs.Energy = 1e3 * Convert.ToDouble(this.textBox_WavelengthEnergy.Text);
        //    }

        //    //光源設定
        //    if (this.radioButton_PointSource.Checked)
        //        this.M1.Source(1, 0);
        //    else
        //        this.M1.Source(Convert.ToInt16(this.textBox_SourceDivY.Text),
        //            Convert.ToDouble(this.textBox_SourceSizeY.Text) / Convert.ToInt16(this.textBox_SourceDivY.Text));

        //    //焦点設定
        //    //ミラーの座標から焦点位置を決定
        //    M1.Detector(1, 0.0,
        //        Convert.ToInt16(this.textBox_PbsD1ny.Text), Convert.ToDouble(this.textBox_PbsD1dy.Text),
        //        Convert.ToDouble(this.textBox_PbsD1bx.Text), Convert.ToDouble(this.textBox_PbsD1by.Text));

        //    //光源(=第一焦点)を設定
        //    M2.s = M1.fw.ToCoord();

        //    //焦点を設定
        //    //ミラーの座標から焦点位置を決定
        //    M2.Detector(1, 0.0,
        //        Convert.ToInt16(this.textBox_PbsD2ny.Text), Convert.ToDouble(this.textBox_PbsD2dy.Text),
        //       Convert.ToDouble(this.textBox_PbsD2bx.Text), Convert.ToDouble(this.textBox_PbsD2by.Text));

        //    pbs.setSource(M1.s.div, M1.s.x, M1.s.y, M1.s.u);
        //    //pbs.setCoord(pbs.wM1, M1.mw.div, M1.mw.x, M1.mw.y);
        //    pbs.setCoord(pbs.wM1, M1.m.div, M1.m.x, M1.m.y);
        //    pbs.setCoord(pbs.wF1, M1.fw.div, M1.fw.x[0], M1.fw.y[0]);
        //    //pbs.setCoord(pbs.wM2, M2.mw.div, M2.mw.x, M2.mw.y);
        //    pbs.setCoord(pbs.wM2, M2.m.div, M2.m.x, M2.m.y);
        //    pbs.setCoord(pbs.wF2, M2.fw.div, M2.fw.x[0], M2.fw.y[0]);

        //    pbs.Execute(_core);

        //    #region graph

        //    GraphPBS(this.zgc_PbsF1, M1.fw.xmod, pbs.wF1);
        //    GraphPBS(this.zgc_PbsF2, M2.fw.xmod, pbs.wF2);
        //    GraphPBSM(this.zgc_PbsF1Med,  pbs.mF1);
        //    GraphPBSM(this.zgc_PbsF2Med,  pbs.mF2);
        //    #endregion
        //}*/


        /*class PBS
        //{
        //    const double h = 6.62607e-34;
        //    const double e = 1.602e-19;
        //    const double c = 2.99792458e8;

        //    public double lambda { get; set; }
        //    public double Energy
        //    {
        //        get { return h * c / (e * lambda); }
        //        set { lambda = h * c / (e * value); }
        //    }

        //    double tShold;
        //    int slitN;
        //    double slitX;
        //    double slitOL;
        //    int slitDiv;

        //    public WaveField wS;
        //    public WaveField[] wSlit;
        //    public WaveField[] wM1;
        //    public WaveField[] wF1;
        //    public WaveField[] wM2;
        //    public WaveField[] wF2;

        //    public double[] mF1;
        //    public double[] mF2;

        //    public PBS(int _slitN,double _slitX,double _slitOL,int _slitDiv,double _tShold)
        //    {
        //        slitN = _slitN;
        //        slitX = _slitX;
        //        slitOL = _slitOL;
        //        slitDiv = _slitDiv;
        //        tShold = _tShold;

        //        wSlit = new WaveField[slitN];
        //        wM1 = new WaveField[slitN];
        //        wF1 = new WaveField[slitN];
        //        wM2 = new WaveField[slitN];
        //        wF2 = new WaveField[slitN];

        //    }

        //    public void setSource(int div, double[] x, double[] y,System.Numerics.Complex[] u)
        //    {
        //        wS = new WaveField(div, lambda);
        //        wS.x = x;
        //        wS.y = y;
        //        wS.u = u;
        //    }

        //    public void setCoord(WaveField[] wf, int div, double[] x, double[] y)
        //    {
        //        for (int i = 0; i < wf.Length; i++)
        //        {
        //            wf[i] = new WaveField(div, lambda);
        //            wf[i].x = x;
        //            wf[i].y = y;
        //        }
        //    }

        //    public void Execute(int core)
        //    {
        //        //スリット位置決定
        //        //(wS.xc,wS.yc)と{(wM1[0].x[0],wM1[0].y[0]),(wM1[0].x[n],wM1[0].y[n])}の一次方程式
        //        int div = wS.x.Length;
        //        int divM = wM1[0].x.Length;
        //        slitX = wM1[0].x[divM / 2] - slitX;

        //        double a0 = (wM1[0].y[0] - wS.y[div / 2]) / (wM1[0].x[0] - wS.x[div / 2]);
        //        double b0 = wS.y[div / 2] - a0 * wS.x[div / 2];
        //        double slitY0 = a0 * slitX + b0;

        //        double an = (wM1[0].y[divM - 1] - wS.y[div / 2]) / (wM1[0].x[divM - 1] - wS.x[div / 2]);
        //        double bn = wS.y[div / 2] - an * wS.x[div / 2];
        //        double slitYn = an * slitX + bn;

        //        slitY0 += (slitYn - slitY0) / (slitN - 1) / 2.0;
        //        slitYn -= (slitYn - slitY0) / (slitN - 1) / 2.0;


        //        double slitDY = (slitYn - slitY0) / (slitN * slitDiv - 1);



        //        for (int i = 0; i < slitN; i++)
        //        {
        //            wSlit[i] = new WaveField(slitDiv, lambda);
        //            for (int j = 0; j < slitDiv; j++)
        //            {
        //                wSlit[i].x[j] = slitX;
        //                wSlit[i].y[j] = slitDY * (i * slitDiv + j) + slitY0;
        //            }
        //        }
        //        //


        //        for (int i = 0; i < slitN; i++)
        //        {
        //            //_wS -> wSlit[i]
        //            wSlit[i].ForwardPropagation(wS, core);
        //            //wSlit[i] -> wM1[i]
        //            wM1[i].ForwardPropagation(wSlit[i], core);
        //            //wM1[i] -> wF1[i]
        //            wF1[i].ForwardPropagation(wM1[i], core);
        //            //wF1[i] -> wM2[i]
        //            wM2[i].ForwardPropagation(wF1[i], core);
        //            //wM2[i] -> wF2[i]
        //            wF2[i].ForwardPropagation(wM2[i], core);

        //        }

        //        //重心計算
        //        mF1 = new double[slitN];
        //        mF2 = new double[slitN];
        //            double avrg1 = 0.0;
        //            double avrg2 = 0.0;

        //        for (int i = 0; i < slitN; i++)
        //        {
        //            mF1[i] = 0.0;
        //            mF2[i] = 0.0;


        //            double sum1 = 0.0;
        //            double sum2 = 0.0;

        //            //max探す
        //            double max = 0.0;
        //            for (int j = 0; j < wF1[i].x.Length; j++)
        //            {
        //                if (max < wF1[i].Intensity[j]) max = wF1[i].Intensity[j];
        //            }
        //            max *= tShold;
        //            for (int j = 0; j < wF1[i].x.Length; j++)
        //            {
        //                double intensity = wF1[i].Intensity[j] - max < 0.0 ? 0.0 : wF1[i].Intensity[j] - max;
        //                mF1[i] += intensity * Math.Sqrt(Math.Pow(wF1[i].y[j] - wF1[i].y[0], 2.0) + Math.Pow(wF1[i].x[j] - wF1[i].x[0], 2.0));
        //                sum1 += wF1[i].Intensity[j];
        //            }
        //            max = 0.0;
        //            for (int j = 0; j < wF2[i].Intensity.Length; j++)
        //            {
        //                if (max < wF2[i].Intensity[j]) max = wF2[i].Intensity[j];
        //            }
        //            max *= tShold;
        //            for (int j = 0; j < wF2[i].x.Length; j++)
        //            {
        //                double intensity = wF2[i].Intensity[j] - max < 0.0 ? 0.0 : wF2[i].Intensity[j] - max;
        //                mF2[i] += intensity * Math.Sqrt(Math.Pow(wF2[i].y[j] - wF2[i].y[0], 2.0) + Math.Pow(wF2[i].x[j] - wF2[i].x[0], 2.0));
        //                sum2 += wF2[i].Intensity[j];
        //            }
        //            mF1[i] /= sum1;
        //            mF2[i] /= sum2;
        //            avrg1 += mF1[i];
        //            avrg2 += mF2[i];
        //        }
        //        for(int i=0;i<slitN;i++)
        //        {
        //            mF1[i] -= avrg1 / (double)mF1.Length;
        //        }
        //        for (int i = 0; i < mF2.Length; i++)
        //        {
        //            mF2[i] -= avrg2 / (double)mF2.Length;
        //        }
        //    }

        //    public void Output(string _fbd)
        //    {
        //        double[,] slit = new double[slitN, slitDiv];
        //        for (int i = 0; i < slitN; i++)
        //            for (int j = 0; j < slitDiv; j++)
        //                slit[i, j] = wSlit[i].Intensity[j];
        //        ClsNac.FileIO.FileIO.writeFile(_fbd + "\\slit.txt", slit);
        //        ClsNac.FileIO.FileIO.writeFile(_fbd + "\\m1.txt", outputData(wM1));
        //        ClsNac.FileIO.FileIO.writeFile(_fbd + "\\f1.txt", outputData(wF1));
        //        ClsNac.FileIO.FileIO.writeFile(_fbd + "\\m2.txt", outputData(wM2));
        //        ClsNac.FileIO.FileIO.writeFile(_fbd + "\\f2.txt", outputData(wF2));
        //        ClsNac.FileIO.FileIO.writeFile(_fbd + "\\mf1.txt", mF1);
        //        ClsNac.FileIO.FileIO.writeFile(_fbd + "\\mf2.txt", mF2);
        //    }

        //    double[,] outputData(WaveField[] wf)
        //    {
        //        double[,] data = new double[wf.Length, wf[0].Intensity.Length];
        //        for(int i=0;i<slitN;i++)
        //        {
        //            for(int j=0;j<wf[0].Intensity.Length;j++)
        //            {
        //                data[i, j] = wf[i].Intensity[j];
        //            }
        //        }
        //        return data;
        //    }
        //}*/


        /*void ProgressReport(ProgressInfo info)
        //{
        //    this.toolStripProgressBar.Value = info.Value;
        //    this.toolStripStatusLabel.Text = info.Message;
        //}

        //private void comboBox_LambdaEnergy_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    this.label_WavelengthEnergy.Text = this.comboBox_LambdaEnergy.SelectedIndex == 0 ? "[Å]" : "[keV]";
        //}*/

        #endregion

        #region graph

        #region Figure
        enum AngleCurv { IncAngle, Curv,FigError }
        AngleCurv ac;

        /// <summary>形状グラフ表示</summary>
        private void GraphFig(ZedGraphControl zgcFig, Mirror1D Mirror)
        {
            GraphPane myPane = zgcFig.GraphPane;
            myPane.CurveList.Clear();
            LineItem myLine =  myPane.AddCurve("Mirror shape", Mirror.m.x_mod, Mirror.m.y_mod, Color.Red, SymbolType.None);
            LineItem myLineSub = null;
            switch(ac)
            {
                case AngleCurv.Curv:
                    myLineSub = myPane.AddCurve("Curvature", Mirror.m.x_mod, Mirror.curv, Color.Blue, SymbolType.None);
                    myPane.Y2Axis.Scale.Mag = 0;
                    myPane.Y2Axis.Title.Text = "Curvature(m)";
                    break;
                case AngleCurv.FigError:
                    myLineSub = myPane.AddCurve("FigError", Mirror.m.x_mod, Mirror.FigError, Color.Blue, SymbolType.None);
                    myPane.Y2Axis.Scale.Mag = -9;
                    myPane.Y2Axis.Title.Text = "Figure error(nm)";
                    break;
                case AngleCurv.IncAngle:
                    myLineSub = myPane.AddCurve("Incident angle", Mirror.m.x_mod, Mirror.theta, Color.Blue, SymbolType.None);
                    myPane.Y2Axis.Scale.Mag = -3;
                    myPane.Y2Axis.Title.Text = "Incident Angle(mrad)";
                    break;

            }

            myLineSub.IsY2Axis = true;

            //myPane.Title.Text = zgcFig.Name.Substring(4) + " NA:" + (Mirror.NA * 1e6).ToString("F1");

            myPane.XAxis.Title.Text = "Position (mm)";
            myPane.XAxis.Scale.Max = Mirror.pm.ML;
            myPane.XAxis.Scale.Min = 0.0;
            myPane.XAxis.Scale.MaxAuto = false;
            myPane.XAxis.Scale.MinAuto = false;
            myPane.XAxis.Scale.Mag = -3;
            myPane.XAxis.Title.IsOmitMag = true;

            myPane.YAxis.Title.Text = "Figure (μm)";
            myPane.YAxis.Scale.Mag = -6;
            myPane.YAxis.Title.IsOmitMag = true;

            myPane.Y2Axis.IsVisible = true;
            myPane.Y2Axis.Title.IsOmitMag = true;

            zgcFig.AxisChange();
            zgcFig.Refresh();
        }

        private void rB_IncAngle_CheckedChanged(object sender, EventArgs e)
        {
            if (rB_IncAngle.Checked)
            {
                ac = AngleCurv.IncAngle;
                GraphFig(this.zgc_M1, M1);
                if (flagM2)
                    GraphFig(this.zgc_M2, M2);
                else
                    GraphClear(this.zgc_M2);
            }
        }

        private void rB_Curv_CheckedChanged(object sender, EventArgs e)
        {
            if (rB_Curv.Checked)
            {
                ac = AngleCurv.Curv;
                GraphFig(this.zgc_M1, M1);
                if (flagM2)
                    GraphFig(this.zgc_M2, M2);
                else
                    GraphClear(this.zgc_M2);
            }
        }

        private void rB_FigError_CheckedChanged(object sender, EventArgs e)
        {
            if(this.rB_FigError.Checked)
            {
                ac = AngleCurv.FigError;
                GraphFig(this.zgc_M1, M1);
                if (flagM2)
                    GraphFig(this.zgc_M2, M2);
                else
                    GraphClear(this.zgc_M2);

            }
        }
        #endregion


        /// <summary>
        /// ミラー強度位相分布
        /// </summary>
        /// <param name="zgc"></param>
        /// <param name="_CoordM"></param>
        private void GraphWaveM(ZedGraph.ZedGraphControl zgc,Coord1D coord)
        {
            GraphPane myPane = zgc.GraphPane;
            myPane.CurveList.Clear();
            LineItem lineIntens = myPane.AddCurve("Intensity", coord.x_mod, coord.Intensity, Color.Red, SymbolType.None);
            LineItem linePhase = myPane.AddCurve("Phase", coord.x_mod, coord.Phase, Color.Blue, SymbolType.None);
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
        private void PlotFocus(PictureBox pictureBox, Mirror1D m)
        {
            ClsNac.Graphic.Plot2dPlane myPlane = new ClsNac.Graphic.Plot2dPlane(pictureBox);
            myPlane.Draw(m.fIntensity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zgc"></param>
        /// <param name="_CoordF"></param>
        private void GraphWaveF(ZedGraphControl zgc, Mirror1D m, int i = 0)
        {
            double[] x = new double[m.pm.Fny];
            for(int j=0;j<m.pm.Fny;j++)
            {
                x[j] = m.pm.Fdy * (j - m.pm.Fny/2);
            }

            GraphPane myPane = zgc.GraphPane;
            myPane.CurveList.Clear();
            LineItem lineIntens = myPane.AddCurve("Intensity", x, m.f[i].Intensity, Color.Red);

            myPane.XAxis.Scale.Min = x[0];
            myPane.XAxis.Scale.Max = x[m.pm.Fny - 1];
            myPane.XAxis.Title.Text = "Position (µm)";
            myPane.XAxis.Scale.Mag = -6;
            myPane.XAxis.Title.IsOmitMag = true;

            myPane.YAxis.Title.Text = "Intensity (a.u.)";

            //myPane.Title.Text = (1e9 * m.FWHM1[i]).ToString("F0") + " (nm) 焦点ずれ: " + (1e6 * m.dx * (i - (m.n - 1) / 2.0)).ToString("F0") + " (um)";
            myPane.Title.Text = (1e9 * ClsNac.WaveField.WaveField.FWHM(m.f[m.pm.Fnx / 2])).ToString("F0");
            zgc.AxisChange();
            zgc.Refresh();
        }

        private void GraphClear(ZedGraphControl zgc)
        {
            zgc.GraphPane.CurveList.Clear();
        }
        #endregion


        #region output
        private void button_FigOutput_Click(object sender, EventArgs e)
        {
            if (this.fbd_FigOutput.ShowDialog() == DialogResult.OK)
            {
                //1枚目設定
                fs = new FileStream(this.fbd_FigOutput.SelectedPath + "\\setting.xml", FileMode.Create);
                serializer.Serialize(fs, M1.pm);
                

                //1枚目形状
                ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror1_FigY.txt", M1.m.y_mod);
                ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror1_Fig.txt", M1.m.x_mod, M1.m.y_mod);
                ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror1_inc.txt", M1.m.x_mod, M1.theta);
                ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror1_FigRaw.txt", M1.m.x, M1.m.y);
                ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror1_Curv.txt", M1.m.x_mod, M1.curv);

                if (this.flagM2)
                {
                    //2枚目設定
                    serializer.Serialize(fs, M2.pm);

                    //2枚目形状
                    ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror2_Fig.txt", M2.m.x_mod, M2.m.y_mod);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror2_FigY.txt", M2.m.y_mod);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror2_inc.txt", M2.m.x_mod, M2.theta);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd_FigOutput.SelectedPath + "\\Mirror2_FigRaw.txt", M2.m.x, M2.m.y);
                }

                fs.Close();
            }
        }

        private void saveSetting(string fileName)
        {
            Mirror1D.Parameter pm = new Mirror1D.Parameter();
            
        }

        private void button_DetectorOutput_Click(object sender, EventArgs e)
        {
            if (this.fbd_Detector.ShowDialog() == DialogResult.OK)
            {
                ClsNac.FileIO.FileIO.writeFile(this.fbd_Detector.SelectedPath + "\\m1.txt", M1.m.Intensity);

                //StringBuilder sb = new StringBuilder();
                //sb.AppendLine("FWHM=" + this.zgc_F1.GraphPane.Title.Text);

                DetectorOutput(this.fbd_Detector.SelectedPath + "\\Detector1.txt", this.M1.fIntensity);
                if (flagM2)
                {
                    ClsNac.FileIO.FileIO.writeFile(this.fbd_Detector.SelectedPath + "\\m2.txt", M2.m.Intensity);
                    DetectorOutput(this.fbd_Detector.SelectedPath + "\\Detector2.txt", this.M2.fIntensity);
                    //sb.AppendLine("FWHM=" + this.zgc_F2.GraphPane.Title.Text);
                }
                //System.IO.File.WriteAllText(this.fbd_Detector.SelectedPath + "\\fwhm.txt", sb.ToString());
            }
        }

        void DetectorOutput(string Path, double[,] f)
        {
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < f.GetLength(1); j++)
            {
                for (int i = 0; i < f.GetLength(0); i++)
                {
                    sb.Append(Convert.ToString(f[i, j])).Append(" ");
                }
                sb.AppendLine();
            }
            System.IO.File.WriteAllText(Path, sb.ToString());
        }


        #endregion

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
                this.groupBox_M1.Text = "ミラー１";
                this.groupBox_M1E.Text = "ミラー１調整";
                this.groupBox_M2.Text = "ミラー２";
                this.groupBox_M2E.Text = "ミラー２調整";
                this.label16.Text = "ミラー間";
                this.textBox_LM2F2.Enabled = true;
            }

        }

        private void rB_DoubleFocus_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rB_DoubleFocus.Checked)
            {
                this.groupBox_M2.Enabled = true;
                this.groupBox_M2E.Enabled = true;
                this.groupBox_M1.Text = "ミラー１";
                this.groupBox_M1E.Text = "ミラー１調整";
                this.groupBox_M2.Text = "ミラー２";
                this.groupBox_M2E.Text = "ミラー２調整";
                this.label16.Text = "ミラー間";
                this.textBox_LM2F2.Enabled = true;
            }
        }

        private void rB_PhaseCompensator_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rB_PhaseCompensator.Checked)
            {
                this.groupBox_M2.Enabled = true;
                this.groupBox_M2E.Enabled = true;
                this.groupBox_M1.Text = "ミラー２";
                this.groupBox_M1E.Text = "ミラー２調整";
                this.groupBox_M2.Text = "形状可変ミラー";
                this.groupBox_M2E.Text = "形状可変ミラー調整";
                this.label16.Text = "ミラー前";
                this.textBox_LM2F2.Enabled = false;


            }
        }

        #endregion

        private void comboBox_LambdaEnergy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.comboBox_LambdaEnergy.SelectedIndex == 0)
            {
                this.label_WavelengthEnergy.Text = "[nm]";
            }
            else
            {
                this.label_WavelengthEnergy.Text = "[keV]";
            }
        }

        private void button_M1E_Fig_Click(object sender, EventArgs e)
        {
            if (this.ofd_EM1.ShowDialog() == DialogResult.OK)
            {
                double[] subError = null;
                ClsNac.FileIO.FileIO.readFile(this.ofd_EM1.FileName, ref subError);
                if (this.rB_PhaseCompensator.Checked)
                    M2.PlusError(subError);
                else
                    M1.PlusError(subError);

                GraphFig(this.zgc_M1, M1);
                if (flagM2)
                    GraphFig(this.zgc_M2, M2);
                else
                    GraphClear(this.zgc_M2);
            }

        }

        private void button_M2E_Fig_Click(object sender, EventArgs e)
        {
            if (this.ofd_EM1.ShowDialog() == DialogResult.OK)
            {
                double[] subError = null;
                ClsNac.FileIO.FileIO.readFile(this.ofd_EM1.FileName, ref subError);
                if (this.rB_PhaseCompensator.Checked)
                    M1.PlusError(subError);
                else
                    M2.PlusError(subError);

                GraphFig(this.zgc_M1, M1);
                if (flagM2)
                    GraphFig(this.zgc_M2, M2);
                else
                    GraphClear(this.zgc_M2);
            }

        }

    }
}
