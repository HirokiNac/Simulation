#define CLI

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
using System.Numerics;

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
                m2h = new Mirror2D(paraH2, m2v);

                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m2h_x.txt", m2h.m.x);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m2h_y.txt", m2h.m.y);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m2h_z.txt", m2h.m.z);

                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m2v_x.txt", m2v.m.x);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m2v_y.txt", m2v.m.y);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m2v_z.txt", m2v.m.z);


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
                    Convert.ToDouble(this.textBox_Detector1bx.Text), Convert.ToDouble(this.textBox_Detector1by.Text), Convert.ToDouble(this.textBox_Detector1bz.Text));

                m1h.m.RotCoord(Convert.ToDouble(textBox_ErrM1h_Rolling.Text), Convert.ToDouble(textBox_ErrM1h_InPlane.Text), Convert.ToDouble(this.textBox_ErrM1h_Incident.Text), m1h.m.xc, m1h.m.yc, m1h.m.zc);
                m1v.m.RotCoord(Convert.ToDouble(textBox_ErrM1v_Rolling.Text), Convert.ToDouble(this.textBox_ErrM1v_Incident.Text), Convert.ToDouble(textBox_ErrM1v_InPlane.Text), m1v.m.xc, m1v.m.yc, m1v.m.zc);


                //var progress = new Progress<ClsNac.Mirror2D.WaveField.ProgressInfo>(this.ProgressReport);

#if CLI

                ClsNac.cliWaveOptics wo = new ClsNac.cliWaveOptics(lambda);
                Complex[] u = new Complex[m1h.m.div];
                m1h.s.u[0] = 1.0;
                wo.Propagate2D(-1, m1h.s.x, m1h.s.y, m1h.s.z, m1h.s.u, m1h.m.x, m1h.m.y, m1h.m.z, ref u);
                m1h.m.u = u;
                u = new Complex[m1v.m.div];
                wo.Propagate2D(-1, m1h.m.x, m1h.m.y, m1h.m.z, m1h.m.u, m1v.m.x, m1v.m.y, m1v.m.z, ref u);
                m1v.m.u = u;
                u = new Complex[m1v.f[0].div];
                wo.Propagate2D(-1, m1v.m.x, m1v.m.y, m1v.m.z, m1v.m.u, m1v.f[0].x, m1v.f[0].y, m1v.f[0].z, ref u);
                m1v.f[0].u = u;

                double[,] intens = new double[m1v.f[0].divL, m1v.f[0].divW];
                for(int i=0;i<m1v.f[0].divL;i++)
                {
                    for (int j = 0; j < m1v.f[0].divW; j++)
                    {
                        intens[i, j] = Math.Pow(m1v.f[0].u[i * m1v.f[0].divW + j].Magnitude, 2.0);
                    }
                }

                //ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m1h_Intensity.txt", m1h.m.Intensity2);
                //ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m1v_Intensity.txt", m1v.m.Intensity2);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\f1_Intensity.txt", intens);


#elif CL

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

                ClsNac.WaveOptics.FresnelKirchhoff2D.Propagation(wf_source, ref wf_m1h, ClsNac.WaveOptics.FresnelKirchhoff2D.MODE.CUDA);
                wf_m1h.CpyReImToComplex();
                ClsNac.WaveOptics.FresnelKirchhoff2D.Propagation(wf_m1h, ref wf_m1v, ClsNac.WaveOptics.FresnelKirchhoff2D.MODE.CUDA);
                wf_m1v.CpyReImToComplex();
                ClsNac.WaveOptics.FresnelKirchhoff2D.Propagation(wf_m1v, ref wf_focus, ClsNac.WaveOptics.FresnelKirchhoff2D.MODE.CUDA);
                wf_focus.CpyReImToComplex();

                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m1h_Intensity.txt", wf_m1h.Intensity());
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m1v_Intensity.txt", wf_m1v.Intensity());
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\f1_Intensity.txt", wf_focus.Intensity());

                if (radioButton_WO_double.Checked)
                {
                    //m1vFocusと同じ座標をつくる
                    m2v.Source(
                        Convert.ToInt32(this.textBox_Detector1ny.Text), Convert.ToInt32(this.textBox_Detector1nz.Text),
                        Convert.ToDouble(this.textBox_Detector1dy.Text), Convert.ToDouble(this.textBox_Detector1dz.Text),
                        Convert.ToDouble(this.textBox_Detector1by.Text), Convert.ToDouble(this.textBox_Detector1bz.Text));
                    //回転
                    double t_y = Math.Atan((m2v.m.zc - m2v.s.zc) / (m2v.m.xc - m2v.s.xc));
                    double t_z = Math.Atan((m2v.m.yc - m2v.s.yc) / (m2v.m.xc - m2v.s.xc));
                    m2v.s.RotCoord(0.0, t_y, t_z, -m2v.pm.f, 0.0, 0.0);
                    //波動場いれる
                    m2v.s.u = wf_focus.u;

                    m2h.Focus(Convert.ToInt32(this.textBox_Detector2nx.Text), Convert.ToInt32(this.textBox_Detector2ny.Text), Convert.ToInt32(this.textBox_Detector2nz.Text),
                        Convert.ToDouble(this.textBox_Detector2dx.Text), Convert.ToDouble(this.textBox_Detector2dy.Text), Convert.ToDouble(this.textBox_Detector2dz.Text),
                        Convert.ToDouble(this.textBox_Detector2bx.Text), Convert.ToDouble(this.textBox_Detector2by.Text), Convert.ToDouble(this.textBox_Detector2bz.Text));

                    ClsNac.WaveOptics.WaveField2D wf_source2 = new ClsNac.WaveOptics.WaveField2D(m2v.s.divL, m2v.s.divW);
                    wf_source2.x = m2v.s.x;
                    wf_source2.y = m2v.s.y;
                    wf_source2.z = m2v.s.z;
                    wf_source2.re = m2v.s.real;
                    wf_source2.im = m2v.s.imag;
                    wf_source2.lambda = lambda;
                    wf_source2.CpyReImToComplex();

                    ClsNac.WaveOptics.WaveField2D wf_m2v = new ClsNac.WaveOptics.WaveField2D(m2v.m.divL, m2v.m.divW);
                    wf_m2v.x = m2v.m.x;
                    wf_m2v.y = m2v.m.y;
                    wf_m2v.z = m2v.m.z;
                    wf_m2v.lambda = lambda;

                    ClsNac.WaveOptics.WaveField2D wf_m2h = new ClsNac.WaveOptics.WaveField2D(m2h.m.divL, m2h.m.divW);
                    wf_m2h.x = m2h.m.x;
                    wf_m2h.y = m2h.m.y;
                    wf_m2h.z = m2h.m.z;
                    wf_m2h.lambda = lambda;

                    ClsNac.WaveOptics.WaveField2D wf_focus2 = new ClsNac.WaveOptics.WaveField2D(m2h.f[0].divL, m2h.f[0].divW);
                    wf_focus2.x = m2h.f[0].x;
                    wf_focus2.y = m2h.f[0].y;
                    wf_focus2.z = m2h.f[0].z;
                    wf_focus2.lambda = lambda;

                    ClsNac.WaveOptics.FresnelKirchhoff2D.Propagation(wf_source2, ref wf_m2v, ClsNac.WaveOptics.FresnelKirchhoff2D.MODE.CUDA);
                    wf_m2v.CpyReImToComplex();
                    ClsNac.WaveOptics.FresnelKirchhoff2D.Propagation(wf_m2v, ref wf_m2h, ClsNac.WaveOptics.FresnelKirchhoff2D.MODE.CUDA);
                    wf_m2h.CpyReImToComplex();
                    ClsNac.WaveOptics.FresnelKirchhoff2D.Propagation(wf_m2h, ref wf_focus2, ClsNac.WaveOptics.FresnelKirchhoff2D.MODE.CUDA);
                    wf_focus2.CpyReImToComplex();

                    ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m2h_Intensity.txt", wf_m2h.Intensity());
                    ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\m2v_Intensity.txt", wf_m2v.Intensity());
                    ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\f2_Intensity.txt", wf_focus2.Intensity());

                }




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
