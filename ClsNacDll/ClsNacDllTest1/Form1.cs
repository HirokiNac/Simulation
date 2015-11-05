using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;

namespace ClsNacDllTest1
{
    public partial class Form1 : Form
    {
        double[] x = new double[] { 0, 1, 2, 3, 4 };
        double[] y = new double[] { 2, 3, 4, 5, 6 };
        double[] z = new double[5];
        Complex[] a;
        
        public Form1()
        {
            InitializeComponent();

            a = new Complex[5];
            for (int i = 0; i < 5; i++) a[i] = new Complex(i, 0);
            ClsNacDll.ampwf.fp2(a);

            ClsNacDll.ampwf.fp(x, y,z);
        }
    }
}
