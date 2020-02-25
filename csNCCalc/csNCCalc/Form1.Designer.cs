namespace csNCCalc
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.button_Calc = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_ofd_fig = new System.Windows.Forms.Button();
            this.textBox_ofd_fig = new System.Windows.Forms.TextBox();
            this.button_ofd_spot = new System.Windows.Forms.Button();
            this.textBox_ofd_spot = new System.Windows.Forms.TextBox();
            this.textBox_damp = new System.Windows.Forms.TextBox();
            this.button_Calc2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(273, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(192, 338);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(471, 12);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(192, 338);
            this.pictureBox2.TabIndex = 0;
            this.pictureBox2.TabStop = false;
            // 
            // button_Calc
            // 
            this.button_Calc.Location = new System.Drawing.Point(192, 116);
            this.button_Calc.Name = "button_Calc";
            this.button_Calc.Size = new System.Drawing.Size(75, 23);
            this.button_Calc.TabIndex = 11;
            this.button_Calc.Text = "計算開始";
            this.button_Calc.UseVisualStyleBackColor = true;
            this.button_Calc.Click += new System.EventHandler(this.button_Calc_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "形状データ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 12);
            this.label1.TabIndex = 10;
            this.label1.Text = "スポットデータ";
            // 
            // button_ofd_fig
            // 
            this.button_ofd_fig.Location = new System.Drawing.Point(237, 41);
            this.button_ofd_fig.Name = "button_ofd_fig";
            this.button_ofd_fig.Size = new System.Drawing.Size(30, 23);
            this.button_ofd_fig.TabIndex = 7;
            this.button_ofd_fig.Text = "...";
            this.button_ofd_fig.UseVisualStyleBackColor = true;
            this.button_ofd_fig.Click += new System.EventHandler(this.button_ofd_volume_Click);
            // 
            // textBox_ofd_fig
            // 
            this.textBox_ofd_fig.Location = new System.Drawing.Point(81, 43);
            this.textBox_ofd_fig.Name = "textBox_ofd_fig";
            this.textBox_ofd_fig.ReadOnly = true;
            this.textBox_ofd_fig.Size = new System.Drawing.Size(150, 19);
            this.textBox_ofd_fig.TabIndex = 5;
            // 
            // button_ofd_spot
            // 
            this.button_ofd_spot.Location = new System.Drawing.Point(237, 12);
            this.button_ofd_spot.Name = "button_ofd_spot";
            this.button_ofd_spot.Size = new System.Drawing.Size(30, 23);
            this.button_ofd_spot.TabIndex = 8;
            this.button_ofd_spot.Text = "...";
            this.button_ofd_spot.UseVisualStyleBackColor = true;
            this.button_ofd_spot.Click += new System.EventHandler(this.button_ofd_spot_Click);
            // 
            // textBox_ofd_spot
            // 
            this.textBox_ofd_spot.Location = new System.Drawing.Point(81, 14);
            this.textBox_ofd_spot.Name = "textBox_ofd_spot";
            this.textBox_ofd_spot.ReadOnly = true;
            this.textBox_ofd_spot.Size = new System.Drawing.Size(150, 19);
            this.textBox_ofd_spot.TabIndex = 6;
            // 
            // textBox_damp
            // 
            this.textBox_damp.Location = new System.Drawing.Point(167, 70);
            this.textBox_damp.Name = "textBox_damp";
            this.textBox_damp.Size = new System.Drawing.Size(100, 19);
            this.textBox_damp.TabIndex = 12;
            this.textBox_damp.Text = "5000";
            // 
            // button_Calc2
            // 
            this.button_Calc2.Location = new System.Drawing.Point(192, 145);
            this.button_Calc2.Name = "button_Calc2";
            this.button_Calc2.Size = new System.Drawing.Size(75, 23);
            this.button_Calc2.TabIndex = 11;
            this.button_Calc2.Text = "計算開始";
            this.button_Calc2.UseVisualStyleBackColor = true;
            this.button_Calc2.Click += new System.EventHandler(this.button_Calc2_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBox_damp);
            this.Controls.Add(this.button_Calc2);
            this.Controls.Add(this.button_Calc);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_ofd_fig);
            this.Controls.Add(this.textBox_ofd_fig);
            this.Controls.Add(this.button_ofd_spot);
            this.Controls.Add(this.textBox_ofd_spot);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button button_Calc;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_ofd_fig;
        private System.Windows.Forms.TextBox textBox_ofd_fig;
        private System.Windows.Forms.Button button_ofd_spot;
        private System.Windows.Forms.TextBox textBox_ofd_spot;
        private System.Windows.Forms.TextBox textBox_damp;
        private System.Windows.Forms.Button button_Calc2;
    }
}

