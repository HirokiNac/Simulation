using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using static System.Math;

namespace ClsNac
{
    

    class Mirror2DWolter
    {

        #region 宣言
        /// <summary>Ellipse a</summary>
        public double e_a { get; private set; }
        /// <summary>Ellipse b</summary>
        public double e_b { get; private set; }
        /// <summary>Ellipse f</summary>
        public double e_f { get; private set; }
        /// <summary>Hyperbola a</summary>
        public double h_a { get; private set; }
        /// <summary>Hyperbola b</summary>
        public double h_b { get; private set; }
        /// <summary>Hyperbola f</summary>
        public double h_f { get; private set; }

        /// <summary>Ellipse x centre</summary>
        public double e_xc { get; private set; }
        /// <summary>Hyperbola x centre</summary>
        public double h_xc { get; private set; }
        public double h_x0 { get; private set; }


        /// <summary></summary>
        public double l1 { get; private set; }
        /// <summary></summary>
        public double l2 { get; private set; }
        /// <summary></summary>
        public double l3 { get; private set; }
        /// <summary></summary>
        public double l4 { get; private set; }
        /// <summary>光源角</summary>
        public double theta1 { get; private set; }
        /// <summary>Ellipse入射角</summary>
        public double theta2 { get; private set; }
        /// <summary></summary>
        public double theta3 { get; private set; }
        /// <summary>Hyperbola入射角</summary>
        public double theta4 { get; private set; }
        /// <summary>焦点角</summary>
        public double theta5 { get; private set; }

        public double[,] x, y, z;
        public int enl, enw;
        public double[,] ex, ey, ez;
        public int hnl, hnw;
        public double[,] hx, hy, hz;

        #endregion

        ///// <summary>
        ///// コンストラクタで設計
        ///// </summary>
        ///// <param name="_l1"></param>
        ///// <param name="_l2"></param>
        ///// <param name="_l3"></param>
        ///// <param name="_l4"></param>
        ///// <param name="_theta2"></param>
        //public Mirror2DWolter(double _l1,double _l2,double _l3,double _l4,double _theta2)
        //{
        //    l1 = _l1;
        //    l2 = _l2;
        //    l3 = _l3;
        //    l4 = _l4;
        //    theta2 = _theta2;

        //    theta1 = Atan((l2 * Sin(2.0 * theta2)) / (l1 + l2 * Cos(2.0 * theta2)));
        //    theta3 = Asin(l1 * Sin(theta1) / l2);
        //    e_a = (l1 + l2) / 2.0;
        //    e_b = Sqrt(e_a * e_a - Pow((l2 * Sin(2.0 * theta2)) / (2.0 * Sin(theta1)), 2.0));
        //    e_f = Sqrt(e_a * e_a - e_b * e_b);

        //    e_xc = l1 * Cos(theta1) - e_f;


        //    theta5 = Asin((l2 - l3) * Sin(theta3) / l4);
        //    h_a = (l2 - l3 - l4) / 2.0;
        //    h_b = Sqrt(Pow((l2 - l3) * Cos(theta3) - l4 * Cos(theta5), 2.0) / 4.0 - h_a * h_a);
        //    theta4 = 0.5 * Asin(2.0 * Sqrt(h_a * h_a + h_b * h_b) * Sin(theta3) / l4);

        //    h_xc =e_f- l4 * Cos(theta5)- 2.0 * Sqrt(h_a * h_a + h_b * h_b);
        //    h_x0 = e_f - Sqrt(h_a * h_a + h_b * h_b);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_l1">光源-Ellipse間</param>
        /// <param name="_l3">Ellipse-Hyperbola間</param>
        /// <param name="_l4">Hyperbola-焦点間</param>
        /// <param name="_theta2">Ellipse斜入射角</param>
        /// <param name="_theta4">Hyperbola斜入射角</param>
        public Mirror2DWolter(double _l1, double _l3, double _l4, double _theta2, double _theta4)
        {
            l1 = _l1;
            l3 = _l3;
            l4 = _l4;
            theta2 = _theta2;
            theta4 = _theta4;

            #region l2計算
            double a1 = (l3 * Sin(2 * theta2) + l4 * Sin(2 * (theta2 + theta4)))
                        / (l1 + l3 * Cos(2 * theta2) + l4 * Cos(2 * (theta2 + theta4)));
            double a2 = Tan(2 * theta2);
            double b2 = -l1 * Tan(2 * theta2);

            double x3 = b2 / (a1 - a2);
            double y3 = a1 * x3;

            double x4 = l1 + l3 * Cos(2 * theta2);
            double y4 = l3 * Sin(2 * theta2);

            l2 = l3 + Sqrt(Pow(x3 - x4, 2.0) + Pow(y3 - y4, 2.0));
            #endregion

            theta1 = Atan((l2 * Sin(2.0 * theta2)) / (l1 + l2 * Cos(2.0 * theta2)));
            theta3 = Asin(l1 * Sin(theta1) / l2);
            e_a = (l1 + l2) / 2.0;
            e_b = Sqrt(e_a * e_a - Pow((l2 * Sin(2.0 * theta2)) / (2.0 * Sin(theta1)), 2.0));
            e_f = Sqrt(e_a * e_a - e_b * e_b);

            e_xc = l1 * Cos(theta1) - e_f;


            theta5 = Asin((l2 - l3) * Sin(theta3) / l4);
            h_a = (l2 - l3 - l4) / 2.0;
            h_b = Sqrt(Pow((l2 - l3) * Cos(theta3) - l4 * Cos(theta5), 2.0) / 4.0 - h_a * h_a);
            //theta4 = 0.5 * Asin(2.0 * Sqrt(h_a * h_a + h_b * h_b) * Sin(theta3) / l4);

            h_xc = e_f - l4 * Cos(theta5) - 2.0 * Sqrt(h_a * h_a + h_b * h_b);
            h_x0 = e_f - Sqrt(h_a * h_a + h_b * h_b);


        }

        public void wolter(double _e_l, int _e_nl, double _e_w, int _e_nw,
            double _h_l, int _h_nl, double _h_w, int _h_nw)
        {
            ellipse(_e_l, _e_nl, _e_w, _e_nw);
            hyperbola(_h_l, _h_nl, _h_w, _h_nw);
        }

        public void SetWolterPos()
        {
        }

        #region ellipse


        /// <summary>
        /// Ellipse 2D
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_z"></param>
        /// <param name="_l"></param>
        /// <param name="_n"></param>
        public void ellipse(out double[] _x, out double[] _z, double _l, int _n)
        {
            _x = new double[_n];
            _z = new double[_n];
            double dx = _l / (_n - 1);

            for (int i = 0; i < _n; i++)
            {
                _x[i] = (i - (_n - 1) / 2.0) * dx + e_xc;
                _z[i] = ellipse(_x[i]);
            }
        }


        /// <summary>
        /// Ellipse 2D point
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        double ellipse(double x)
        {
            return -e_b * Sqrt(1.0 - Pow(x / e_a, 2.0));
        }

        /// <summary>
        /// Ellipse 3D point
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <returns></returns>
        public double ellipse(double _x, double _y)
        {
            return -e_b * Sqrt(1.0 - (Pow(_x / e_a, 2.0) + Pow(_y / e_b, 2.0)));
        }

        public void ellipse(out double[,] _x, out double[,] _y, out double[,] _z, double _l, int _nl, double _w, int _nw)
        {
            enl = _nl;
            enw = _nw;

            _x = new double[_nw, _nl];
            _y = new double[_nw, _nl];
            _z = new double[_nw, _nl];

            double dx = _l / (_nl - 1);
            double dy = _w / (_nw - 1);

            for (int iw = 0; iw < _nw; iw++)
            {
                for (int il = 0; il < _nl; il++)
                {
                    _x[iw, il] = (il - (_nl - 1) / 2.0) * dx + e_xc;
                    _y[iw, il] = (iw - (_nw - 1) / 2.0) * dy;
                    _z[iw, il] = ellipse(_x[iw, il], _y[iw, il]);
                }
            }
        }

        /// <summary>
        /// Ellipse 3D point e_xcを足し算して計算
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        public void ellipse(double[,] _x, double[,] _y, out double[,] _z)
        {
            int nw = _x.GetLength(0);
            int nl = _x.GetLength(1);

            _z = new double[nw, nl];
            for (int iw = 0; iw < nw; iw++)
            {
                for (int il = 0; il < nl; il++)
                {
                    _z[iw, il] = ellipse(_x[iw, il] + e_xc, _y[iw, il]);
                }
            }
        }

        public void ellipse(double _l, int _nl, double _w, int _nw)
        {

            ellipse(out ex, out ey, out ez, _l, _nl, _w, _nw);
        }

        #endregion

        #region hyperbola

        /// <summary>
        /// Hyperbola 2D point
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        double hyperbola(double x)
        {
            return -h_b * Sqrt(Pow((x - h_x0) / h_a, 2.0) - 1.0);
        }
        
        /// <summary>
        /// Hyperbola 3D point
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <returns></returns>
        public double hyperbola(double _x, double _y)
        {
            return -h_b * Sqrt(-1.0 + (Pow((_x - h_x0) / h_a, 2.0) - Pow(_y / h_b, 2.0)));
        }

        /// <summary>
        /// Hyperbola 3D point h_xcを足し算して計算
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_y"></param>
        /// <param name="_z"></param>
        public void hyperbola(double[,] _x, double[,] _y, out double[,] _z)
        {
            int nw = _x.GetLength(0);
            int nl = _x.GetLength(1);

            _z = new double[nw, nl];
            for (int iw = 0; iw < nw; iw++)
            {
                for (int il = 0; il < nl; il++)
                {
                    _z[iw, il] = hyperbola(_x[iw, il] + h_xc, _y[iw, il]);
                }
            }


        }

        /// <summary>
        /// Hyperbola 2D
        /// </summary>
        /// <param name="_x"></param>
        /// <param name="_z"></param>
        /// <param name="_l"></param>
        /// <param name="_n"></param>
        public void hyperbola(out double[] _x, out double[] _z, double _l, int _n)
        {
            _x = new double[_n];
            _z = new double[_n];
            double dx = _l / (_n - 1);
            for (int i = 0; i < _n; i++)
            {
                _x[i] = (i - (_n - 1) / 2.0) * dx + h_xc;
                _z[i] = hyperbola(_x[i]);
            }

        }


        public void hyperbola(out double[,] _x, out double[,] _y, out double[,] _z, double _l, int _nl, double _w, int _nw)
        {
            hnl = _nl;
            hnw = _nw;

            _x = new double[_nw, _nl];
            _y = new double[_nw, _nl];
            _z = new double[_nw, _nl];

            double dx = _l / (_nl - 1);
            double dy = _w / (_nw - 1);

            for (int iw = 0; iw < _nw; iw++)
            {
                for (int il = 0; il < _nl; il++)
                {
                    _x[iw, il] = (il - (_nl - 1) / 2.0) * dx + h_xc;
                    _y[iw, il] = (iw - (_nw - 1) / 2.0) * dy;
                    _z[iw, il] = hyperbola(_x[iw, il], _y[iw, il]);
                }
            }
        }

        public void hyperbola(double _l, int _nl, double _w, int _nw)
        {
            hyperbola(out hx, out hy, out hz, _l, _nl, _w, _nw);
        }

        #endregion

        #region source

        public enum SourceType { point, gauss, rectangle }

        public void source(out double[,] _x, out double[,] _y, out double[,] _z, out double[,] _real, out double[,] _imag,
            double wy, int ny, double wz, int nz, SourceType _source = SourceType.point)
        {


            double xc = -e_f;
            double yc = 0.0;
            double zc = 0.0;

            if (_source == SourceType.point)
            {
                ny = 1;
                nz = 1;
            }
            _x = new double[ny, nz];
            _y = new double[ny, nz];
            _z = new double[ny, nz];
            _real = new double[ny, nz];
            _imag = new double[ny, nz];


            //幅->1pixサイズ
            double dy = _source == SourceType.gauss ? 4.0 * Math.Sqrt(2.0 * Math.Log10(2.0)) * wy / (ny + 1.0) : wy / (ny + 1.0);
            double dz = _source == SourceType.gauss ? 4.0 * Math.Sqrt(2.0 * Math.Log10(2.0)) * wz / (nz + 1.0) : wz / (nz + 1.0);

            double[] x = new double[nz];
            double[] z = new double[nz];

            for (int j = 0; j < nz; j++)
            {
                x[j] = xc;
                z[j] = zc + (-nz / 2 + j) * dz;
            }

            //光軸に対して垂直になるように回転
            Rot(ref x, ref z, -theta1, xc, zc);

            double sigy = wy / (2.0 * Math.Sqrt(2.0 * Math.Log(2.0, Math.E)));
            double sigz = wz / (2.0 * Math.Sqrt(2.0 * Math.Log(2.0, Math.E)));

            for (int i = 0; i < ny; i++)
            {
                for (int j = 0; j < nz; j++)
                {
                    _x[i, j] = x[j];
                    _y[i, j] = yc + (-ny / 2 + i) * dy;
                    _z[i, j] = z[j];

                    if (_source == SourceType.gauss)
                    {
                        _real[i, j] = gauss(_y[i, j], zc + (-nz / 2 + j) * dz, sigy, sigz);
                        _imag[i, j] = 0.0;
                    }
                    else
                    {
                        _real[i, j] = 1.0;
                        _imag[i, j] = 0.0;
                    }
                }
            }
        }
        static double gauss(double y, double z, double sigmay, double sigmaz)
        {
            return Math.Exp(-y * y / (2.0 * sigmay * sigmay)) / (Math.Sqrt(2.0 * Math.PI) * sigmay)
                * Math.Exp(-z * z / (2.0 * sigmaz * sigmaz)) / (Math.Sqrt(2.0 * Math.PI) * sigmaz);
        }

        #endregion


        #region focus

        public void focus(out double[,] _x, out double[,] _y, out double[,] _z,
            double _dy, int _ny, double _dz, int _nz, double _bx = 0.0, double _by = 0.0, double _bz = 0.0)
        {

            double xc = e_f - 2 * h_f;
            double yc = 0.0;
            double zc = 0.0;

            _x = new double[_ny, _nz];
            _y = new double[_ny, _nz];
            _z = new double[_ny, _nz];

            double[] x = new double[_nz];
            double[] z = new double[_nz];

            //まず回転していない座標で設定
            //表示用ディテクター座標設定 focus_x2

            for (int j = 0; j < _nz; j++)
            {
                x[j] = xc + _bx;
                z[j] = zc + (-_nz / 2 + j) * _dz + _bz;
                //focus_x2[i][j] = Math.Sqrt(Math.Pow(focus_x[i][j] - focus_x[i][0], 2.0) + Math.Pow(focus_y[i][j] - focus_y[i][0], 2.0));
            }
            Rot(ref x, ref z, theta5, xc, yc);

            for (int i = 0; i < _ny; i++)
            {
                for (int j = 0; j < _nz; j++)
                {
                    _x[i, j] = x[j];
                    _y[i, j] = yc + (-_ny / 2 + i) * _dy + _by;
                    _z[i, j] = z[j];
                }
            }


        }


        #endregion

        #region 回転移動
        public static void Rot(ref double[] x, ref double[] y, double rotTheta, double x0, double y0)
        {
            for (int i = 0; i < x.Length; i++)
            {
                double x_buf = x[i] - x0;
                double y_buf = y[i] - y0;
                x[i] = x_buf * Math.Cos(rotTheta) - y_buf * Math.Sin(rotTheta) + x0;
                y[i] = x_buf * Math.Sin(rotTheta) + y_buf * Math.Cos(rotTheta) + y0;
            }
        }

        /// <summary>
        /// xy座標移動
        /// </summary>
        /// <param name="dx">x移動量</param>
        /// <param name="dy">y移動量</param>
        public static void Move(ref double[] x, ref double[] y, double dx, double dy)
        {
            for (int i = 0; i < x.Length; i++)
            {
                x[i] += dx;
                y[i] += dy;
            }
        }
        #endregion
    }




    namespace WaveField
    {

        struct jag
        {
            double[][] x;
            double[][] y;
            double[][] z;
            Complex[][] u;
        }

        struct oned
        {
            double[] x;
            double[] y;
            double[] z;
            Complex[] u;
        }

        struct twod
        {
            double[,] x;
            double[,] y;
            double[,] z;
            Complex[,] u;
        }

        class ProgressInfo
        {
            public ProgressInfo(int _Value, string _Message)
            {
                Value = _Value;
                Message = _Message;
            }
            public int Value { get; private set; }
            public string Message { get; private set; }
        }

        class WaveField2D
        {
            #region 宣言



            #region constant
            const double h = 6.62607004e-34;
            const double e = 1.60217662e-19;
            const double c = 299792458;
            double _lambda;
            public double lambda
            {
                get { return _lambda; }
                set
                {
                    _lambda = value;
                    k = 2.0 * Math.PI / value;
                }
            }
            public double Energy
            {
                get { return h * c / (e * lambda); }
                set { lambda = h * c / (e * value); }
            }
            double k { get; set; }

            #endregion

            //public Complex[,] u;     //波動場
            public double[,] x;      //波動場X座標
            public double[,] y;      //波動場Y座標
            public double[,] z;
            public bool[,] reflect;
            public double[,] Intensity;
            public double[,] Phase;
            public double[,] Re;
            public double[,] Im;

            public double[] xv;
            public double[] yv;
            public double[] zv;
            public double[] rev;
            public double[] imv;


            public int divW { get; private set; }
            public int divL { get; private set; }
            public int div { get { return divW * divL; } }


            int doneNum;

            #endregion

            //コンストラクタ
            public WaveField2D(double _lambda)
            {
                lambda = _lambda;
            }

            public WaveField2D(double _lambda, int _divW, int _divL)
            {
                lambda = _lambda;
                divW = _divW;
                divL = _divL;
            }

            public void Initialize(double[,] _x, double[,] _y, double[,] _z, bool[,] _reflect = null, double[,] _real = null, double[,] _imag = null)
            {
                divW = _x.GetLength(0);
                divL = _x.GetLength(1);

                x = _x;
                y = _y;
                z = _z;
                reflect = _reflect;
                //u = _u != null ? _u : new Complex[divW, divL];

                if (_real == null) _real = new double[divW, divL];
                if (_imag == null) _imag = new double[divW, divL];

                Re = new double[divW, divL];
                Im = new double[divW, divL];
                Intensity = new double[divW, divL];
                Phase = new double[divW, divL];
                xv = new double[divW * divL];
                yv = new double[divW * divL];
                zv = new double[divW * divL];
                rev = new double[divW * divL];
                imv = new double[divW * divL];

                for (int i = 0; i < divW; i++)
                {
                    for (int j = 0; j < divL; j++)
                    {
                        Re[i, j] = _real[i, j];
                        Im[i, j] = _imag[i, j];
                        xv[i + divW * j] = x[i, j];
                        yv[i + divW * j] = y[i, j];
                        zv[i + divW * j] = z[i, j];
                        rev[i + divW * j] = Re[i, j];
                        imv[i + divW * j] = Im[i, j];
                    }
                }
            }

            public void Initialize()
            {

            }

            public void two2one()
            {
                for (int i = 0; i < divW; i++)
                {
                    for (int j = 0; j < divL; j++)
                    {
                        xv[i + divW * j] = x[i, j];
                        yv[i + divW * j] = y[i, j];
                        zv[i + divW * j] = z[i, j];
                        rev[i + divW * j] = Re[i, j];
                        imv[i + divW * j] = Im[i, j];
                    }
                }
            }

            public void one2two()
            {
                for (int i = 0; i < divW; i++)
                {
                    for (int j = 0; j < divL; j++)
                    {
                        x[i, j] = xv[i + divW * j];
                        y[i, j] = yv[i + divW * j];
                        z[i, j] = zv[i + divW * j];
                        Re[i, j] = rev[i + divW * j];
                        Im[i, j] = imv[i + divW * j];
                    }
                }
            }

            //順方向伝播(引数：伝播元波動場)
#if CSPARA
                public void ForwardPropagation(WaveField2D u_back)
                {
                    double[,] ds = new double[u_back.divW, u_back.divL];
                    
            #region 微小領域をかけた場合の計算
                    //伝播元の微小長さの計算
                    if (u_back.x.Length != 1)
                    {
                        for (int m = 1; m < u_back.divW; m++)
                        {
                            for (int n = 1; n < u_back.divL; n++)
                            {
                                ds[m, n] = Math.Sqrt(Math.Pow(u_back.y[m, n] - u_back.y[m - 1, n], 2.0) + Math.Pow(u_back.z[m, n] - u_back.z[m - 1, n], 2.0))
                                    * Math.Sqrt(Math.Pow(u_back.x[m, n] - u_back.x[m, n - 1], 2.0) + Math.Pow(u_back.z[m, n] - u_back.z[m, n - 1], 2.0));
                            }
                            ds[m, 0] = ds[m, 1];
                        }
                        for (int n = 0; n < u_back.divL; n++)
                        {
                            ds[0, n] = ds[1, n];
                        }
                    }
                    else
                    {
                        ds[0,0] = 1;
                    }

            #endregion

            #region 伝播計算
                    doneNum = 0;
                    this.Intensity = new double[this.divW, this.divL];
                    this.Phase = new double[this.divW, this.divL];

                    Parallel.For(0, this.divW, i =>
                    {
                        for (int j = 0; j < this.divL; j++)
                        {
                            this.u[i, j] = new Complex(0.0, 0.0);

                            if (this.reflect == null || this.reflect[i, j])
                            {
                                for (int m = 0; m < u_back.divW; m++)
                                {
                                    for (int n = 0; n < u_back.divL; n++)
                                    {
                                        this.u[i, j] += u_back.u[m, n] * Complex.Exp(-Complex.ImaginaryOne * k * Math.Sqrt(Math.Pow(this.x[i, j] - u_back.x[m, n], 2.0) + Math.Pow(this.y[i, j] - u_back.y[m, n], 2.0) + Math.Pow(this.z[i, j] - u_back.z[m, n], 2.0)))
                                                        / Math.Sqrt(Math.Pow(this.x[i, j] - u_back.x[m, n], 2.0) + Math.Pow(this.y[i, j] - u_back.y[m, n], 2.0) + Math.Pow(this.z[i, j] - u_back.z[m, n], 2.0)) * ds[m, n];
                                    }
                                }
                            }

                            //強度位相計算
                            this.Intensity[i, j] = Math.Pow(this.u[i, j].Magnitude, 2.0);
                            this.Phase[i, j] = this.u[i, j].Phase;
                        }
                        doneNum++;
                    });
            #endregion
                }
#endif
            public void ForwardPropagation2(WaveField2D u_back)
            {
                double[] ds = new double[u_back.divW * u_back.divL];

                #region 微小領域をかけた場合の計算
                //伝播元の微小長さの計算
                if (u_back.x.Length != 1)
                {
                    for (int m = 1; m < u_back.divW; m++)
                    {
                        for (int n = 1; n < u_back.divL; n++)
                        {
                            ds[m + n * u_back.divW] = Math.Sqrt(Math.Pow(u_back.yv[m + n * u_back.divW] - u_back.yv[m - 1 + n * u_back.divW], 2.0) + Math.Pow(u_back.zv[m + n * u_back.divW] - u_back.zv[m - 1 + n * u_back.divW], 2.0))
                                * Math.Sqrt(Math.Pow(u_back.xv[m + n * u_back.divW] - u_back.xv[m + (n - 1) * u_back.divW], 2.0) + Math.Pow(u_back.zv[m + n * u_back.divW] - u_back.zv[m + (n - 1) * u_back.divW], 2.0));
                        }
                        ds[m] = ds[m + 1 * u_back.divW];
                    }
                    for (int n = 0; n < u_back.divL; n++)
                    {
                        ds[0 + n * u_back.divW] = ds[1 + n * u_back.divW];
                    }
                }
                else
                {
                    ds[0] = 1;
                }

                #endregion


                ClsNac.WaveOpticsCpp_Wrapper.Prop2D(lambda, -1,
                    u_back.divL * u_back.divW, u_back.xv, u_back.yv, u_back.zv, u_back.rev, u_back.imv, ds,
                    divL * divW, xv, yv, zv, rev, imv);

                for (int i = 0; i < divW; i++)
                {
                    for (int j = 0; j < divL; j++)
                    {
                        Re[i, j] = rev[i + divW * j];
                        Im[i, j] = imv[i + divW * j];
                        Intensity[i, j] = Math.Sqrt(Re[i, j] * Re[i, j] + Im[i, j] * Im[i, j]);
                        Phase[i, j] = Math.Atan2(Re[i, j], Im[i, j]);
                    }
                }

            }

            ////逆方向伝播（引数；伝播元波動場）
            //public void BackwardPropagation(WaveField u_back)
            //{
            //    double k;
            //    double[] ds = new double[u_back.x.Length];
            //    Complex ii = new Complex(0.0, 1.0);     //虚数単位

            //    //波数の計算
            //    k = 2.0 * Math.PI / this.lambda;

            //    //伝播元の微小長さの計算
            //    if (u_back.x.Length != 1)
            //    {
            //        for (int i = 1; i < u_back.x.Length; i++)
            //        {
            //            ds[i] = Math.Sqrt(Math.Pow(u_back.x[i] - u_back.x[i - 1], 2.0) + Math.Pow(u_back.y[i] - u_back.y[i - 1], 2.0));
            //        }
            //        ds[0] = ds[1];
            //    }
            //    else
            //    {
            //        ds[0] = 1;
            //    }

            //    //伝播計算
            //    for (int i = 0; i < this.x.Length; i++)
            //    {
            //        this.u[i] = new Complex(0.0, 0.0);
            //        for (int j = 0; j < u_back.x.Length; j++)
            //        {
            //            this.u[i] += u_back.u[j] * Complex.Exp(ii * k * Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)))
            //                            / Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)) * ds[j];
            //        }
            //    }
            //}

            ////FWHM計算
            //public static double FWHM(WaveField waveField)
            //{
            //    double max = 0.0;
            //    double iMax = 0;
            //    for (int i = 0; i < waveField.PosX.Length; i++)
            //        if (waveField.Intensity[i] > max)
            //        {
            //            max = waveField.Intensity[i];
            //            iMax = i;
            //        }

            //    double dblePos1 = 0.0;
            //    double dblePos2 = 0.0;
            //    for (int i = 1; i < iMax; i++)
            //        if (max / 2.0 < waveField.Intensity[i])
            //        {
            //            dblePos1 = (waveField.PosX[i] - waveField.PosX[i - 1]) / (waveField.Intensity[i] - waveField.Intensity[i - 1]) * (max / 2.0 - waveField.Intensity[i]) + waveField.PosX[i];
            //            break;
            //        }
            //    for (int i = waveField.PosX.Length - 2; i > iMax; i--)
            //        if (max / 2.0 < waveField.Intensity[i])
            //        {
            //            dblePos2 = (waveField.PosX[i] - waveField.PosX[i + 1]) / (waveField.Intensity[i] - waveField.Intensity[i + 1]) * (max / 2.0 - waveField.Intensity[i]) + waveField.PosX[i];
            //            break;
            //        }

            //    return dblePos2 - dblePos1;
            //}

        }

    }


}
