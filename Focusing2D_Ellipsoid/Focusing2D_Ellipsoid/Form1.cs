using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Focusing2D_Ellipsoid
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void checkBox_MthetaUnit_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_MthetaUnit.Checked)
                checkBox_MthetaUnit.Text = "deg";
            else
                checkBox_MthetaUnit.Text = "rad";
        }
    }
}
