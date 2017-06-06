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
            myPlane.Draw(m2d.m.z3_mod);
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

        private void button_WaveOptCalc_Click(object sender, EventArgs e)
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

            try
            {

                if (m2d == null)
                {
                    MessageBox.Show("形状がありません");
                    return;
                }

                double lambda = 1e-10 * Convert.ToDouble(this.textBox_WavelengthEnergy.Text);

                #region 光源設定
                wS = new ClsNac.Mirror2D.WaveField.WaveField2D(lambda);

                if (this.radioButton_SourceGauss.Checked)
                {
                    m2d.Source(Convert.ToInt32(this.textBox_SourceDivY.Text), Convert.ToInt32(this.textBox_SourceDivZ.Text),
                    Convert.ToDouble(this.textBox_SourceSizeZ.Text), Convert.ToDouble(this.textBox_SourceSizeZ.Text),
                    ClsNac.Mirror2D.Mirror2D.source.gauss);
                }
                else if (this.radioButton_SourceRectangle.Checked)
                    m2d.Source(Convert.ToInt32(this.textBox_SourceDivY.Text), Convert.ToInt32(this.textBox_SourceDivZ.Text),
                    Convert.ToDouble(this.textBox_SourceSizeZ.Text), Convert.ToDouble(this.textBox_SourceSizeZ.Text));
                else
                {
                    m2d.Source(1, 1, 0, 0);

                }

                wS.Initialize(m2d.s.x, m2d.s.y, m2d.s.z, _u: m2d.s.u);
                #endregion

                #region ミラー設定
                wM = new ClsNac.Mirror2D.WaveField.WaveField2D(lambda);
                wM.Initialize(m2d.m.x3, m2d.m.y3, m2d.m.z3, m2d.m.reflect);
                #endregion

                #region 焦点設定
                //焦点設定
                m2d.Focus(
                    Convert.ToInt32(this.textBox_Dnx.Text), Convert.ToDouble(this.textBox_Ddx.Text),
                    Convert.ToInt32(this.textBox_Dny.Text), Convert.ToDouble(this.textBox_Ddy.Text),
                    Convert.ToInt32(this.textBox_Dnz.Text), Convert.ToDouble(this.textBox_Ddz.Text));
                wF = new ClsNac.Mirror2D.WaveField.WaveField2D[m2d.f.nx];
                for (int i = 0; i < m2d.f.nx; i++)
                {
                    wF[i] = new ClsNac.Mirror2D.WaveField.WaveField2D(lambda);
                    wF[i].Initialize(m2d.f.x[i], m2d.f.y[i], m2d.f.z[i]);
                }
                #endregion

            }
            catch (Exception ex)
            { }


            //伝播計算
            wM.ForwardPropagation(wS);
            for (int i = 0; i < wF.Length; i++)
                wF[i].ForwardPropagation(wM);
            //

            //表示
            ClsNac.Graphic.Plot2dPlane myPlane = new ClsNac.Graphic.Plot2dPlane(this.pictureBox_Focus);
            myPlane.Draw(wF[wF.Length / 2].Intensity);
            //
        }

        private void button_DetectorOutput_Click(object sender, EventArgs e)
        {
            if (this.fbd.ShowDialog() == DialogResult.OK)
            {
                ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\IntensitySource.txt", m2d.s.Intensity);
                ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\IntensityMirror.txt", wM.Intensity);
                for (int i = 0; i < wF.Length; i++)
                    ClsNac.FileIO.FileIO.writeFile(this.fbd.SelectedPath + "\\IntensityFocus" + i.ToString("D2") + ".txt", wF[i].Intensity);
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

        private void textBox_ML_TextChanged(object sender, EventArgs e)
        {
            double ML = double.Parse(textBox_ML.Text);
            double Pitch = double.Parse(textBox_PitchML.Text);
            textBox_DivML.Text = Convert.ToString((int)(ML / Pitch) + 1);
        }

        private void textBox_PitchML_TextChanged(object sender, EventArgs e)
        {
            double ML = double.Parse(textBox_ML.Text);
            double Pitch = double.Parse(textBox_PitchML.Text);
            textBox_DivML.Text = Convert.ToString((int)(ML / Pitch) + 1);
        }

        private void textBox_MW_TextChanged(object sender, EventArgs e)
        {
            try
            {
                double MW = double.Parse(textBox_MW.Text);
                double Pitch = double.Parse(textBox_PitchMW.Text);
                textBox_DivMW.Text = Convert.ToString((int)(MW / Pitch) + 1);
            }
            catch (Exception)
            { }
        }

        private void textBox_PitchMW_TextChanged(object sender, EventArgs e)
        {
            try
            {
                double MW = double.Parse(textBox_MW.Text);
                double Pitch = double.Parse(textBox_PitchMW.Text);
                textBox_DivMW.Text = Convert.ToString((int)(MW / Pitch) + 1);
            }
            catch (Exception)
            { }
        }
    }
}
