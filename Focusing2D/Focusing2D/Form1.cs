#define CL

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClsNac.Mirror2D;
using System.Runtime.InteropServices;

namespace Focusing2D
{
    public partial class Form1 : Form
    {
        Mirror2D.Parameter paraH1 = new Mirror2D.Parameter();
        Mirror2D.Parameter paraH2 = new Mirror2D.Parameter();
        Mirror2D.Parameter paraV1 = new Mirror2D.Parameter();
        Mirror2D.Parameter paraV2 = new Mirror2D.Parameter();
        Mirror2D m1h, m1v, m2h, m2v;


        public Form1()
        {
            InitializeComponent();
        }

        private void button_FigCalc_Click(object sender, EventArgs e)
        {
            FigCalc();
        }

        void FigCalc()
        {
            //すべてのミラーの形状を決定
            #region parameter

            //1st水平
            paraH1.L1 = Convert.ToDouble(this.textBox_LSM1h.Text);
            paraH1.L2 = Convert.ToDouble(this.textBox_LM1hF1.Text);
            paraH1.ML = Convert.ToDouble(this.textBox_LM1h.Text);
            paraH1.MW = Convert.ToDouble(this.textBox_WM1h.Text);
            paraH1.theta_i = Convert.ToDouble(this.textBox_ThetaM1h.Text);
            paraH1.divL = Convert.ToInt32(this.textBox_DivLM1h.Text);
            paraH1.divW = Convert.ToInt32(this.textBox_DivWM1h.Text);
            paraH1.dir = Mirror2D.Dir.Horizontal;
            paraH1.pos = this.checkBox_PosM1h.Checked ? Mirror2D.Pos.ring : Mirror2D.Pos.hall;
            m1h = new Mirror2D(paraH1);

            //1st垂直
            paraV1.L1 = Convert.ToDouble(this.textBox_LSM1v.Text);
            paraV1.L2 = Convert.ToDouble(this.textBox_LM1vF1.Text);
            paraV1.ML = Convert.ToDouble(this.textBox_LM1v.Text);
            paraV1.MW = Convert.ToDouble(this.textBox_WM1v.Text);
            paraV1.theta_i = Convert.ToDouble(this.textBox_ThetaM1v.Text);
            paraV1.divL = Convert.ToInt32(this.textBox_DivLM1v.Text);
            paraV1.divW = Convert.ToInt32(this.textBox_DivWM1v.Text);
            paraV1.dir = Mirror2D.Dir.Vertical;
            paraV1.pos = this.checkBox_PosM1v.Checked ? Mirror2D.Pos.upper : Mirror2D.Pos.lower;
            m1v = new Mirror2D(paraV1, m1h);


            ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m1h_x.txt", m1h.m.x);
            ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m1h_y.txt", m1h.m.y);
            ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m1h_z.txt", m1h.m.z);

            ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m1v_x.txt", m1v.m.x);
            ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m1v_y.txt", m1v.m.y);
            ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m1v_z.txt", m1v.m.z);


            //前のミラーに対してアライメント
            //焦点を回転中心に前ミラーの入射光に合わせる
            //前ミラーを回転中心に出射光に合わせる

            //前から順番にアライメント調整
            //面内回転・入射角を前のミラーに合わせる
            //n枚目ミラー座標系をn-1枚目ミラーに合わせる

            if (this.checkBox_TwoStage.Checked)
            {
                //2nd水平
                paraH2.L1 = Convert.ToDouble(this.textBox_LM1hM2h.Text) - paraH1.L2;
                paraH2.L2 = Convert.ToDouble(this.textBox_LM2hF2.Text);
                paraH2.ML = Convert.ToDouble(this.textBox_LM1h.Text);
                paraH2.MW = Convert.ToDouble(this.textBox_WM1h.Text);
                paraH2.theta_i = Convert.ToDouble(this.textBox_ThetaM1h.Text);
                paraH2.divL = Convert.ToInt32(this.textBox_DivLM1h.Text);
                paraH2.divW = Convert.ToInt32(this.textBox_DivWM1h.Text);
                paraH2.dir = Mirror2D.Dir.Horizontal;
                paraH2.pos = this.checkBox_PosM2h.Checked ? Mirror2D.Pos.lower : Mirror2D.Pos.upper;
                m2h = new Mirror2D(paraH2);
                //2nd垂直
                paraV2.L1 = Convert.ToDouble(this.textBox_LM1vM2v.Text) - paraV1.L2;
                paraV2.L2 = Convert.ToDouble(this.textBox_LM2vF2.Text);
                paraV2.ML = Convert.ToDouble(this.textBox_LM2v.Text);
                paraV2.MW = Convert.ToDouble(this.textBox_WM2v.Text);
                paraV2.theta_i = Convert.ToDouble(this.textBox_ThetaM2v.Text);
                paraV2.divL = Convert.ToInt32(this.textBox_DivLM2v.Text);
                paraV2.divW = Convert.ToInt32(this.textBox_DivWM2v.Text);
                paraV2.dir = Mirror2D.Dir.Vertical;
                paraV2.pos = this.checkBox_PosM2v.Checked ? Mirror2D.Pos.lower : Mirror2D.Pos.upper;
                m2v = new Mirror2D(paraV2);
            }

            #endregion




        }
        System.Diagnostics.Stopwatch sw;


        private async void button_WaveOptCalc_Click(object sender, EventArgs e)
        {
            sw = new System.Diagnostics.Stopwatch();
            this.button_WaveOptCalc.Enabled = false;
            this.button_FigCalc.Enabled = false;

            try
            {
                sw.Start();

                double lambda = Convert.ToDouble(textBox_WavelengthEnergy.Text) * 1e-10;
                m1h.Source(1, 1);

                m1v.Focus(Convert.ToInt32(this.textBox_Detector1nx.Text), Convert.ToInt32(this.textBox_Detector1ny.Text), Convert.ToInt32(this.textBox_Detector1nz.Text),
                    Convert.ToDouble(this.textBox_Detector1dx.Text), Convert.ToDouble(this.textBox_Detector1dy.Text), Convert.ToDouble(this.textBox_Detector1dz.Text),
                    Convert.ToDouble(this.textBox_Detector1bx.Text),Convert.ToDouble(this.textBox_Detector1by.Text),Convert.ToDouble(this.textBox_Detector1bz.Text));
                
                m1h.m.RotCoord(0.0, Convert.ToDouble(this.textBox_ErrM1h_Incident.Text), 0.0, m1h.m.xc, m1h.m.yc, m1h.m.zc);
                m1v.m.RotCoord(0.0, Convert.ToDouble(this.textBox_ErrM1v_Incident.Text), 0.0, m1v.m.xc, m1v.m.yc, m1v.m.zc);


                //var progress = new Progress<ClsNac.Mirror2D.WaveField.ProgressInfo>(this.ProgressReport);
                #if CL

                ClsNac.WaveOptics.WaveField2D wf_source = new ClsNac.WaveOptics.WaveField2D(m1h.s.divL, m1h.s.divW);
                wf_source.x = m1h.s.x;
                wf_source.y = m1h.s.y;
                wf_source.z = m1h.s.z;
                wf_source.re = m1h.s.real;
                wf_source.im = m1h.s.imag;
                wf_source.lambda = lambda;
                wf_source.CpyReImToComplex();
                
                ClsNac.WaveOptics.WaveField2D wf_m1h = new ClsNac.WaveOptics.WaveField2D(m1h.m.divL, m1h.m.divW);
                wf_m1h.x = m1h.m.x;
                wf_m1h.y = m1h.m.y;
                wf_m1h.z = m1h.m.z;
                wf_m1h.lambda = lambda;

                ClsNac.WaveOptics.WaveField2D wf_m1v = new ClsNac.WaveOptics.WaveField2D(m1v.m.divL, m1v.m.divW);
                wf_m1v.x = m1v.m.x;
                wf_m1v.y = m1v.m.y;
                wf_m1v.z = m1v.m.z;
                wf_m1v.lambda = lambda;

                ClsNac.WaveOptics.WaveField2D wf_focus = new ClsNac.WaveOptics.WaveField2D(m1v.f[0].divL, m1v.f[0].divW);
                wf_focus.x = m1v.f[0].x;
                wf_focus.y = m1v.f[0].y;
                wf_focus.z = m1v.f[0].z;
                wf_focus.lambda = lambda;

                if (radioButton_WO_double.Checked)
                {
                    ClsNac.WaveOptics.FresnelKirchhoff2D.Propagation(wf_source, ref wf_m1h, ClsNac.WaveOptics.FresnelKirchhoff2D.MODE.CUDA);
                    wf_m1h.CpyReImToComplex();
                    ClsNac.WaveOptics.FresnelKirchhoff2D.Propagation(wf_m1h, ref wf_m1v, ClsNac.WaveOptics.FresnelKirchhoff2D.MODE.CUDA);
                    wf_m1v.CpyReImToComplex();
                    ClsNac.WaveOptics.FresnelKirchhoff2D.Propagation(wf_m1v, ref wf_focus, ClsNac.WaveOptics.FresnelKirchhoff2D.MODE.CUDA);
                    wf_focus.CpyReImToComplex();

                }
                else
                {
                    ClsNac.WaveOptics.FresnelKirchhoff2Df.Propagation(wf_source, ref wf_m1h, ClsNac.WaveOptics.FresnelKirchhoff2Df.MODE.CUDA);
                    wf_m1h.CpyReImToComplex();
                    ClsNac.WaveOptics.FresnelKirchhoff2Df.Propagation(wf_m1h, ref wf_m1v, ClsNac.WaveOptics.FresnelKirchhoff2Df.MODE.CUDA);
                    wf_m1v.CpyReImToComplex();
                    ClsNac.WaveOptics.FresnelKirchhoff2Df.Propagation(wf_m1v, ref wf_focus, ClsNac.WaveOptics.FresnelKirchhoff2Df.MODE.CUDA);
                    wf_focus.CpyReImToComplex();
                }

                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m1h_Intensity.txt", wf_m1h.Intensity());
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m1v_Intensity.txt", wf_m1v.Intensity());
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\f_Intensity.txt", wf_focus.Intensity());

                #else

                ClsNac.Mirror2D.WaveField.WaveField wS = new ClsNac.Mirror2D.WaveField.WaveField(ref m1h.s, lambda);
                ClsNac.Mirror2D.WaveField.WaveField wM1h = new ClsNac.Mirror2D.WaveField.WaveField(ref m1h.m, lambda);
                ClsNac.Mirror2D.WaveField.WaveField wM1v = new ClsNac.Mirror2D.WaveField.WaveField(ref m1v.m, lambda);
                ClsNac.Mirror2D.WaveField.WaveField wF = new ClsNac.Mirror2D.WaveField.WaveField(ref m1v.f[0], lambda);

                await Task.Run(() =>
                {
                    strProp = "S -> M1h";
                    wM1h.ForwardPropagation(wS, progress);
                    strProp = "M1h -> M1v";
                    wM1v.ForwardPropagation(wM1h, progress);
                    strProp = "M1v -> F";
                    wF.ForwardPropagation(wM1v, progress);
                });
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m1h_Intensity.txt", wM1h.I2);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m1v_Intensity.txt", wM1v.I2);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\f_Intensity.txt", wF.I2);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\fx.txt", m1v.f[0].x);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\fz.txt", m1v.f[0].z);
#endif
            }
            finally
            {
                sw.Stop();
                this.toolStripStatusLabel_Message.Text = Convert.ToString(sw.ElapsedMilliseconds) + " msec";
                this.button_WaveOptCalc.Enabled = true;
                this.button_FigCalc.Enabled = true;
            }
        }


        //string strProp;
        //void ProgressReport(ClsNac.Mirror2D.WaveField.ProgressInfo info)
        //{
        //    this.toolStripProgressBar.Value = info.Value;
        //    this.toolStripStatusLabel_Message.Text = info.Message + " " + strProp + " " + Convert.ToString(this.sw.ElapsedMilliseconds/1000)+" sec経過";
 
        //}
    }
}
