using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Focusing2DWolter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        ClsNac.Mirror2DWolter m2d;
        ClsNac.WaveField.WaveField2D wS;
        ClsNac.WaveField.WaveField2D wME;
        ClsNac.WaveField.WaveField2D wMH;
        ClsNac.WaveField.WaveField2D[] wF;

        private void Button_Fig_Calc2D_Click(object sender, EventArgs e)
        {
            m2d = new ClsNac.Mirror2DWolter(
                Convert.ToDouble(textBox_L1.Text), 
                Convert.ToDouble(textBox_L3.Text), 
                Convert.ToDouble(textBox_L4.Text), 
                Convert.ToDouble(textBox_theta_Ell.Text), 
                Convert.ToDouble(textBox_theta_Hyp.Text));

            m2d.ellipse(Convert.ToDouble(textBox_Fig_LengthMEL.Text), Convert.ToInt32(textBox_Fig_CountMEL.Text),
                Convert.ToDouble(textBox_Fig_LengthMW.Text), Convert.ToInt32(textBox_Fig_CountMW.Text));

            m2d.hyperbola(Convert.ToDouble(textBox_Fig_LengthMHL.Text), Convert.ToInt32(textBox_Fig_CountMHL.Text),
                Convert.ToDouble(textBox_Fig_LengthMW.Text), Convert.ToInt32(textBox_Fig_CountMW.Text));
            m2d.SetWolterPos();
            //表示



            ClsNac.Graphic.Plot2dPlane p2p_e = new ClsNac.Graphic.Plot2dPlane(pictureBox_FigEllipse);
            p2p_e.Draw(m2d.ez);
            ClsNac.Graphic.Plot2dPlane p2p_h = new ClsNac.Graphic.Plot2dPlane(pictureBox_FigHyperbola);
            p2p_h.Draw(m2d.hz);
        }

        private void Button_Fig_Output2D_Click(object sender, EventArgs e)
        {
            if (fbd_FigOutput.ShowDialog() == DialogResult.OK)
                ClsNac.FileIO.FileIO.writeFile(fbd_FigOutput.SelectedPath + "\\test.txt", m2d.z);
        }

        private CancellationTokenSource _cts = null;


        private async void Button_WaveOptCalc_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

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

            try
            {
                button_WaveOptCalc.Enabled = false;
                sw.Start();

                if (m2d == null)
                {
                    MessageBox.Show("形状がありません");
                    return;
                }

                #region 初期設定
                double lambda = 1e-10 * Convert.ToDouble(this.textBox_WavelengthEnergy.Text);

                #region 光源設定
                wS = new ClsNac.WaveField.WaveField2D(lambda);

                ClsNac.Mirror2DWolter.SourceType st;
                if (radioButton_SourceGauss.Checked) st = ClsNac.Mirror2DWolter.SourceType.gauss;
                else if (radioButton_SourceRectangle.Checked) st = ClsNac.Mirror2DWolter.SourceType.rectangle;
                else st = ClsNac.Mirror2DWolter.SourceType.point;

                double[,] sx, sy, sz, sre, sim;

                m2d.source(out sx, out sy, out sz, out sre, out sim,
                    Convert.ToDouble(this.textBox_SourceSizeY.Text), Convert.ToInt32(this.textBox_SourceDivY.Text),
                    Convert.ToDouble(this.textBox_SourceSizeZ.Text), Convert.ToInt32(this.textBox_SourceDivZ.Text),
                    st);

                wS.Initialize(sx, sy, sz, _real: sre, _imag: sim);
                #endregion

                #region ミラー設定
                wME = new ClsNac.WaveField.WaveField2D(lambda);
                wME.Initialize(m2d.ex, m2d.ey, m2d.ez);
                
                wMH = new ClsNac.WaveField.WaveField2D(lambda);
                wMH.Initialize(m2d.hx, m2d.hy, m2d.hz);
                //厳密な計算

                #endregion

                #region 焦点設定 detector
                //焦点設定
                int fnx = Convert.ToInt32(textBox_Fnx.Text);
                double[][,] fx = new double[fnx][,];
                double[][,] fy = new double[fnx][,];
                double[][,] fz = new double[fnx][,];
                wF = new ClsNac.WaveField.WaveField2D[fnx];

                for (int i = 0; i < fnx; i++)
                {
                    m2d.focus(out fx[i], out fy[i], out fz[i],
                    Convert.ToDouble(this.textBox_Fdy.Text), Convert.ToInt32(this.textBox_Fny.Text),
                    Convert.ToDouble(this.textBox_Fdz.Text), Convert.ToInt32(this.textBox_Fnz.Text),
                    Convert.ToDouble(textBox_Fbx.Text), Convert.ToDouble(textBox_Fby.Text), Convert.ToDouble(textBox_Fbz.Text));
                }
                

                for (int i = 0; i < fnx; i++)
                {
                    wF[i] = new ClsNac.WaveField.WaveField2D(lambda);
                    wF[i].Initialize(fx[i], fy[i], fz[i]);
                }
                #endregion
                #endregion

                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\sx.txt", sx);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\sy.txt", sy);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\sz.txt", sz);

                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\ex.txt", m2d.ex);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\ey.txt", m2d.ey);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\ez.txt", m2d.ez);

                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\hx.txt", m2d.hx);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\hy.txt", m2d.hy);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\hz.txt", m2d.hz);

                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\fx.txt", fx[0]);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\fy.txt", fy[0]);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\fz.txt", fz[0]);


                //全要素数計算
                toolStripProgressBar.Value = 0;
                toolStripProgressBar.Maximum = 100;
                long progressMax = wS.div * wME.div + wME.div * wMH.div + wMH.div * wF.Length * wF[0].div;
                int progress_sme = (int)(100.0 * wS.div * wME.div / progressMax);
                int progress_memh = (int)(100.0 * wME.div * wMH.div / progressMax);
                int progress_mhf = (int)(100.0 * wMH.div * wF[0].div / progressMax);


                await Task.Run(() =>
                {
                    //伝播計算

                    //BeginInvoke((Action)(() => { ProgressReport(0, "Source->Mirror"); }));

                    wME.ForwardPropagation2(wS);
                    //BeginInvoke((Action)(() => { ProgressReport(progress_sme, "Mirror->Focus0"); }));

                    wMH.ForwardPropagation2(wME);

                    //BeginInvoke((Action)(() => { ProgressReport(progress_memh, "Mirror->Focus0"); }));

                    for (int i = 0; i < wF.Length; i++)
                    {
                        wF[i].ForwardPropagation2(wMH);

                        //BeginInvoke((Action)(() => { ProgressReport(progress_mhf, string.Format("Mirror->Focus{0}", i + 1)); }));
                    }
                    //
                });

                //表示
                ClsNac.Graphic.Plot2dPlane p2p_e = new ClsNac.Graphic.Plot2dPlane(pictureBox_WoEllipse);
                p2p_e.Draw(wME.Intensity);
                ClsNac.Graphic.Plot2dPlane p2p_h = new ClsNac.Graphic.Plot2dPlane(pictureBox_WoHyperbola);
                p2p_h.Draw(wMH.Intensity);
                ClsNac.Graphic.Plot2dPlane myPlane = new ClsNac.Graphic.Plot2dPlane(this.pictureBox_Focus);
                myPlane.Draw(wF[wF.Length / 2].Intensity);
                //listbox追加
                //listBox1.Items.Clear();

                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\intensity_e.txt", wME.Intensity);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\intensity_h.txt", wMH.Intensity);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\intensity_f.txt", wF[wF.Length / 2].Intensity);

                //
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sw.Stop();
                //progressBar.Value = 100;
                label_Progress.Text = "Finish  " + sw.Elapsed.ToString(@"hh\:mm\:ss");
                this._cts = null;
                button_WaveOptCalc.Enabled = true;
            }

        }


        private void ProgressReport(int _progressValue, string _message)
        {
            toolStripProgressBar.Value += _progressValue;
            toolStripStatusLabel1.Text = _message;
        }

        private void Button_DetectorOutput_Click(object sender, EventArgs e)
        {

        }
    }
}
