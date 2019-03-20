using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace Focusing2D_Rev
{
    public partial class Form1 : Form
    {
        ClsNac.Mirror2D.Mirror2D m2d;
        ClsNac.Mirror2D.WaveField.WaveField2D wS;
        ClsNac.Mirror2D.WaveField.WaveField2D wM;
        ClsNac.Mirror2D.WaveField.WaveField2D[] wF;

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
            m2d = new ClsNac.Mirror2D.Mirror2D(
                Convert.ToDouble(this.textBox_LSM.Text), Convert.ToDouble(this.textBox_LMF.Text),
                Convert.ToDouble(this.textBox_Mtheta.Text), Convert.ToInt32(this.textBox_DivMW.Text), Convert.ToInt32(this.textBox_DivML.Text),
                Convert.ToDouble(this.textBox_MW.Text), Convert.ToDouble(this.textBox_ML.Text));
            
            ClsNac.Graphic.Plot2dPlane myPlane = new ClsNac.Graphic.Plot2dPlane(this.pictureBox_Figure);
            //myPlane.Draw(m2d.m.z3_mod);
            this.textBox_rx.Text = Convert.ToString(m2d.m.rx[m2d.m.divL / 2]);
            this.textBox_ry.Text = Convert.ToString(m2d.m.ry[m2d.m.divL / 2]);
        }

        private void button_FigOutput_Click(object sender, EventArgs e)
        {
            if (m2d != null)
            {
                if (this.fbd.ShowDialog() == DialogResult.OK)
                {
                    ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\x.txt", m2d.m.x3);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\y.txt", m2d.m.y3);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\z.txt", m2d.m.z3);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\z_raw.txt", m2d.m.z);
                    ClsNac.FileIO.FileIO.writeFile(fbd.SelectedPath + "\\z_raw_plane.txt", m2d.m.z_mod);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\z_mod.txt", m2d.m.z3_mod);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\z_torus.txt", m2d.m.z_torus);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\z_torus_sub.txt", m2d.m.z_torus_sub);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\rx.txt", m2d.m.rx);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\ry.txt", m2d.m.ry);
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("L1:" + this.textBox_LSM.Text);
                    sb.AppendLine("L2:" + this.textBox_LMF.Text);
                    sb.AppendLine("ML:" + this.textBox_ML.Text);
                    sb.AppendLine("MW:" + this.textBox_MW.Text);
                    sb.AppendLine("theta:" + this.textBox_Mtheta.Text);
                    System.IO.File.WriteAllText(this.fbd.SelectedPath + "\\setting.txt", sb.ToString());
                }
            }
            else
            { MessageBox.Show("形状がありません．"); }
        }

        private async void button_WaveOptCalc_Click(object sender, EventArgs e)
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
                wS = new ClsNac.Mirror2D.WaveField.WaveField2D(lambda);

                if (this.radioButton_SourceGauss.Checked)
                {
                    m2d.Source(Convert.ToInt32(this.textBox_SourceDivY.Text), Convert.ToInt32(this.textBox_SourceDivZ.Text),
                    Convert.ToDouble(this.textBox_SourceSizeY.Text), Convert.ToDouble(this.textBox_SourceSizeZ.Text),
                    ClsNac.Mirror2D.Mirror2D.source.gauss);
                }
                else if (this.radioButton_SourceRectangle.Checked)
                    m2d.Source(Convert.ToInt32(this.textBox_SourceDivY.Text), Convert.ToInt32(this.textBox_SourceDivZ.Text),
                    Convert.ToDouble(this.textBox_SourceSizeY.Text), Convert.ToDouble(this.textBox_SourceSizeZ.Text));
                else
                {
                    m2d.Source(1, 1, 0, 0);

                }

                wS.Initialize(m2d.s.x, m2d.s.y, m2d.s.z, _real: m2d.s.real, _imag: m2d.s.imag);
                #endregion

                #region ミラー設定
                wM = new ClsNac.Mirror2D.WaveField.WaveField2D(lambda);
                wM.Initialize(m2d.m.x, m2d.m.y, m2d.m.z);

                //厳密な計算

                #endregion

                #region 焦点設定
                //焦点設定
                m2d.Focus(
                    Convert.ToInt32(this.textBox_Dnx.Text), Convert.ToDouble(this.textBox_Ddx.Text),
                    Convert.ToInt32(this.textBox_Dny.Text), Convert.ToDouble(this.textBox_Ddy.Text),
                    Convert.ToInt32(this.textBox_Dnz.Text), Convert.ToDouble(this.textBox_Ddz.Text),
                    Convert.ToDouble(textBox_Dbx.Text), Convert.ToDouble(textBox_Dby.Text), Convert.ToDouble(textBox_Dbz.Text));

                wF = new ClsNac.Mirror2D.WaveField.WaveField2D[m2d.f.nx];
                for (int i = 0; i < m2d.f.nx; i++)
                {
                    wF[i] = new ClsNac.Mirror2D.WaveField.WaveField2D(lambda);
                    wF[i].Initialize(m2d.f.x[i], m2d.f.y[i], m2d.f.z[i]);
                }
                #endregion
                #endregion

                //全要素数計算
                progressBar.Value = 0;
                progressBar.Maximum = 100;
                long progressMax = wS.div * wM.div + (long)wM.div * wF.Length * wF[0].div;
                int progress_sm = (int)(100.0 * wS.div * wM.div / progressMax);
                int progress_mf = (int)(100.0 * wM.div * wF[0].div / progressMax);
                
                
                await Task.Run(() =>
                {
                    //伝播計算
                    
                    BeginInvoke((Action)(() => { ProgressReport(0, "Source->Mirror"); }));
                    wM.ForwardPropagation2(wS);

                    BeginInvoke((Action)(() => { ProgressReport(progress_sm, "Mirror->Focus0"); }));

                    for (int i = 0; i < wF.Length; i++)
                    {
                        wF[i].ForwardPropagation2(wM);

                        BeginInvoke((Action)(() => { ProgressReport(progress_mf, string.Format("Mirror->Focus{0}", i + 1)); }));
                    }
                    //
                });

                //表示
                ClsNac.Graphic.Plot2dPlane myPlane = new ClsNac.Graphic.Plot2dPlane(this.pictureBox_Focus);
                myPlane.Draw(wF[wF.Length / 2].Intensity);
                //listbox追加
                listBox1.Items.Clear();
                
                //
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sw.Stop();
                progressBar.Value = 100;
                label_Progress.Text = "Finish  " + sw.Elapsed.ToString(@"hh\:mm\:ss");
                this._cts = null;
                button_WaveOptCalc.Enabled = true;
            }


        }

        private void ProgressReport(int _progressValue, string _message)
        {
            progressBar.Value += _progressValue;
            label_Progress.Text = _message;
        }


        private void button_DetectorOutput_Click(object sender, EventArgs e)
        {
            if (this.fbd.ShowDialog() == DialogResult.OK)
            {
                //ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\IntensitySource.txt", m2d.s.Intensity);
                //ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\PhaseSource.txt", m2d.s.Phase);

                //ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\IntensityMirror.txt", wM.Intensity);
                //ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\PhaseMirror.txt", wM.Phase);
                for (int i = 0; i < wF.Length; i++)
                {
                    ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\IntensityFocus" + i.ToString("D2") + ".txt", wF[i].Intensity);
                    ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\PhaseFocus" + i.ToString("D2") + ".txt", wF[i].Phase);
                }
            }

        }

        private void comboBox_LambdaEnergy_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.label_WavelengthEnergy.Text = this.comboBox_LambdaEnergy.SelectedIndex == 0 ? "[Å]" : "[keV]";

        }

        #region radiobutton
        private void radioButton_SourcePoint_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton_SourcePoint.Checked)
            {
                this.textBox_SourceDivY.Enabled = false;
                this.textBox_SourceDivZ.Enabled = false;
                this.textBox_SourceSizeY.Enabled = false;
                this.textBox_SourceSizeZ.Enabled = false;
            }
        }

        private void radioButton_SourceRectangle_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton_SourceRectangle.Checked)
            {
                this.textBox_SourceDivY.Enabled = true;
                this.textBox_SourceDivZ.Enabled = true;
                this.textBox_SourceSizeY.Enabled = true;
                this.textBox_SourceSizeZ.Enabled = true;

                this.label_SourceDivY.Text = "Y分割数";
                this.label_SourceDivZ.Text = "Z分割数";
                this.label_SourceSizeY.Text = "Y光源幅";
                this.label_SourceSizeZ.Text = "Z光源幅";
            }
        }

        private void radioButton_SourceGauss_CheckedChanged(object sender, EventArgs e)
        {
            if (this.radioButton_SourceGauss.Checked)
            {
                this.textBox_SourceDivY.Enabled = true;
                this.textBox_SourceDivZ.Enabled = true;
                this.textBox_SourceSizeY.Enabled = true;
                this.textBox_SourceSizeZ.Enabled = true;

                this.label_SourceDivY.Text = "Y全幅分割数";
                this.label_SourceDivZ.Text = "Z全幅分割数";
                this.label_SourceSizeY.Text = "Y半値幅";
                this.label_SourceSizeZ.Text = "Z半値幅";
            }
        }
        #endregion

        private void button_TorusCalc_Click(object sender, EventArgs e)
        {
            m2d.torus(Convert.ToDouble(this.textBox_rx.Text) - Convert.ToDouble(this.textBox_ry.Text), Convert.ToDouble(this.textBox_ry.Text));
        }


        private void textBox_ML_Leave(object sender, EventArgs e)
        {
            try
            {
                double ML = double.Parse(textBox_ML.Text);
                double Pitch = double.Parse(textBox_PitchML.Text);
                textBox_DivML.Text = Convert.ToString((int)(ML / Pitch) + 1);
            }
            catch (Exception)
            { }

        }

        private void textBox_DivML_Leave(object sender, EventArgs e)
        {
            try
            {
                double ML = double.Parse(textBox_ML.Text);
                int Div = int.Parse(textBox_DivML.Text);
                textBox_PitchML.Text = Convert.ToString((double)(ML / Div));
            }
            catch (Exception)
            { }

        }

        private void textBox_PitchML_Leave(object sender, EventArgs e)
        {
            try
            {
                double ML = double.Parse(textBox_ML.Text);
                double Pitch = double.Parse(textBox_PitchML.Text);
                textBox_DivML.Text = Convert.ToString((int)(ML / Pitch + 1));
            }
            catch (Exception)
            { }

        }

        private void textBox_MW_Leave(object sender, EventArgs e)
        {
            try
            {
                double MW = double.Parse(textBox_MW.Text);
                double Pitch = double.Parse(textBox_PitchMW.Text);
                textBox_DivMW.Text = Convert.ToString((int)(MW / Pitch + 1));
            }
            catch (Exception)
            { }

        }

        private void textBox_PitchMW_Leave(object sender, EventArgs e)
        {
            try
            {
                double MW = double.Parse(textBox_MW.Text);
                double Pitch = double.Parse(textBox_PitchMW.Text);
                textBox_DivMW.Text = Convert.ToString((int)(MW / Pitch + 1));
            }
            catch (Exception)
            { }

        }

        private void textBox_DivMW_Leave(object sender, EventArgs e)
        {
            try
            {
                double MW = double.Parse(textBox_MW.Text);
                int Div = int.Parse(textBox_DivMW.Text);
                textBox_PitchMW.Text = Convert.ToString((double)(MW / Div));
            }
            catch (Exception)
            { }

        }



        private void button_Err_FigXY_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog_FigError = new OpenFileDialog();
                //誤差データ読み込み
                if (openFileDialog_FigError.ShowDialog() == DialogResult.OK)
                {
                    textBox_Err_FigXY.Text = openFileDialog_FigError.FileName;
                    double[,] data_Error = null;
                    ClsNac.FileIO.FileIO.readFile(openFileDialog_FigError.FileName, ref data_Error);

                    //理想形状データ数確認
                    double[,] data_Error2 = new double[m2d.divW, m2d.divL];
                    if (m2d.divW == data_Error.GetLength(0) && m2d.divL == data_Error.GetLength(1))
                    {
                        Array.Copy(data_Error, data_Error2, m2d.divW * m2d.divL);
                    }
                    else
                    {
                        double[] posx = new double[m2d.divW];
                        double[] posy = new double[m2d.divL];

                        double dx = data_Error.GetLength(0) / (double)m2d.divW;
                        double dy = data_Error.GetLength(1) / (double)m2d.divL;

                        for (int i = 0; i < m2d.divW; i++) posx[i] = i * dx;
                        for (int j = 0; j < m2d.divL; j++) posy[j] = j * dy;


                        ClsNac.Spline spline = new ClsNac.Spline(data_Error);
                        data_Error2 = spline.Execute2(posx, posy);

                        //ClsNac.MathNetMod.Spline2D spline2D = new ClsNac.MathNetMod.Spline2D(data_Error);
                        //data_Error2 = spline2D.Interpolation(m2d.divW, m2d.divL);
                    }
                

                //誤差データ数補完
                    
                    //理想形状データ＋誤差データ
                    for(int i_W=0;i_W<m2d.divW;i_W++)
                    {
                        for (int i_L = 0; i_L < m2d.divL; i_L++)
                        {
                            m2d.m.z[i_W, i_L] += data_Error2[i_W, i_L];
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            
        }




        public class Setting
        {
            public double L1;
            public double L2;
            public double ML;
            public double MW;
            public double Mtheta;
            public double MLd;
            public double MWd;

            public double lambda_energy;
            public enum type_source { point,square,gauss};
            public type_source source;
            public double source_YW;
            public int source_YN;
            public double source_ZW;
            public int source_ZN;

            public int focus_XN;
            public int focus_YN;
            public int focus_ZN;
            public double focus_Xd;
            public double focus_Yd;
            public double focus_Zd;
        }

        private void toolStripButton_SettingSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.xml|*.xml";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Setting setting = new Setting();

                setting.L1 = double.Parse(textBox_LSM.Text);
                setting.L2 = double.Parse(textBox_LMF.Text);
                setting.ML = double.Parse(textBox_ML.Text);
                setting.MW = double.Parse(textBox_MW.Text);
                setting.Mtheta = double.Parse(textBox_Mtheta.Text);
                setting.MLd = double.Parse(textBox_PitchML.Text);
                setting.MWd = double.Parse(textBox_PitchMW.Text);

                setting.lambda_energy = double.Parse(textBox_WavelengthEnergy.Text);
                setting.source_YW = double.Parse(textBox_SourceSizeY.Text);
                setting.source_ZW = double.Parse(textBox_SourceSizeZ.Text);
                setting.source_YN = int.Parse(textBox_SourceDivY.Text);
                setting.source_ZN = int.Parse(textBox_SourceDivZ.Text);

                if (radioButton_SourceGauss.Checked)
                    setting.source = Setting.type_source.gauss;
                else if (radioButton_SourceRectangle.Checked)
                    setting.source = Setting.type_source.square;
                else
                    setting.source = Setting.type_source.point;


                setting.focus_XN = int.Parse(textBox_Dnx.Text);
                setting.focus_YN = int.Parse(textBox_Dny.Text);
                setting.focus_ZN = int.Parse(textBox_Dnz.Text);
                setting.focus_Xd = double.Parse(textBox_Ddx.Text);
                setting.focus_Yd = double.Parse(textBox_Ddy.Text);
                setting.focus_Zd = double.Parse(textBox_Ddz.Text);

                FileStream fs = new FileStream(sfd.FileName, FileMode.Create);
                StreamWriter writer = new StreamWriter(fs, Encoding.UTF8);

                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add(String.Empty, String.Empty);

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Setting));
                xmlSerializer.Serialize(writer, setting, ns);

                writer.Flush();
                writer.Close();
                writer = null;
            }


        }




        private void toolStripButton_SettingLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog()==DialogResult.OK)
            {
                FileStream fs = new FileStream(ofd.FileName,FileMode.Open);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Setting));
                Setting setting = (Setting)xmlSerializer.Deserialize(fs);
                fs.Close();

                textBox_LSM.Text = setting.L1.ToString();
                textBox_LMF.Text = setting.L2.ToString();
                textBox_ML.Text = setting.ML.ToString();
                textBox_MW.Text = setting.MW.ToString();
                textBox_Mtheta.Text = setting.Mtheta.ToString();
                textBox_PitchML.Text = setting.MLd.ToString();

            }

        }

    }
}
