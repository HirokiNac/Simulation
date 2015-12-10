#define CPP

using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MathNet.Numerics;
using System.Numerics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using static System.Math;

namespace ClsNac
{

    namespace Mirror1D
    {
        //前ミラーが平面の場合は，前ミラー焦点が後ろミラーの中心

        public class Coord
        {
            int div { get; set; }

            public double[] x { get; set; }
            public double[] y { get; set; }

            public double xc { get { return x[div / 2]; } }
            public double yc { get { return y[div / 2]; } }

            /// <summary>補正X座標</summary>
            public double[] x_mod
            {
                get
                {
                    double[] _xmod = new double[div];
                    for (int i = 0; i < div; i++)
                        _xmod[i] = x[i] - x[0];
                    return _xmod;
                }
            }

            /// <summary>補正Y座標</summary>
            public double[] y_mod
            {
                get
                {
                    double[] _ymod = new double[div];
                    var p = Fit.Line(x, y);
                    double min = double.MaxValue;
                    for (int i = 0; i < div; i++)
                    {
                        _ymod[i] = y[i] - (p.Item2 * x[i] + p.Item1);
                        if (_ymod[i] < min) min = _ymod[i];
                    }

                    for (int i = 0; i < div; i++)
                    {
                        _ymod[i] -= min;
                    }

                    return _ymod;
                }
            }


            public Complex[] u { get; set; }
            public double[] real
            {
                get
                {
                    double[] _real = new double[div];
                    for (int i = 0; i < div; i++)
                        _real[i] = u[i].Real;
                    return _real;
                }
            }
            public double[] imag
            {
                get
                {
                    double[] _imag = new double[div];
                    for (int i = 0; i < div; i++)
                        _imag[i] = u[i].Imaginary;
                    return _imag;
                }
            }

            public double[] Intensity { get; set; }
            public double[] Phase { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="_div"></param>
            public Coord(int _div)
            {
                Init(_div);
            }

            /// <summary>
            /// Initialize
            /// </summary>
            /// <param name="_div">要素数</param>
            public void Init(int _div)
            {
                div = _div;

                x = new double[div];
                y = new double[div];
                u = new Complex[div];
            }

            #region 座標補正
            /// <summary>
            /// xy座標回転
            /// </summary>
            /// <param name="rotTheta">回転角度</param>
            /// <param name="x0">x回転中心</param>
            /// <param name="y0">y回転中心</param>
            public void Rot(double rotTheta, double x0, double y0)
            {
                for (int i = 0; i < div; i++)
                {
                    double x_buf = x[i] - x0;
                    double y_buf = y[i] - y0;
                    x[i] = x_buf * Math.Cos(rotTheta) - y_buf * Sin(rotTheta) + x0;
                    y[i] = x_buf * Sin(rotTheta) + y_buf * Math.Cos(rotTheta) + y0;
                }
            }

            /// <summary>
            /// xy座標移動
            /// </summary>
            /// <param name="dx">x移動量</param>
            /// <param name="dy">y移動量</param>
            public void Move(double dx, double dy)
            {
                for (int i = 0; i < div; i++)
                {
                    x[i] += dx;
                    y[i] += dy;
                }
            }
            #endregion

        }

        public class Mirror1D
        {

            public double[] theta { get; private set; }
            public double[] curv { get; private set; }

            public class Parameter
            {
                public enum MirrorType { Plane, Ellipse, Parabolic }
                public MirrorType mirrorType { get; set; }

                public double L1 { get; set; }
                public double L2 { get; set; }
                public double theta_i { get; set; }
                public double theta_s { get; set; }
                public double theta_f { get; set; }

                public double a { get; set; }
                public double b { get; set; }
                public double f { get; set; } = 0.0;

                #region Source
                public enum SourceType { Point, Rectangle, Gaussian }
                public SourceType sourceType { get; set; }
                public double SW { get; set; }
                public int divSW { get; set; } = 0;
                #endregion

                #region Mirror
                public enum Pos { upper = 1, lower = -1 }
                public Pos pos { get; set; }

                public double ML { get; set; }
                public int divML { get; set; } = 0;

                //public double MW { get; set; }
                //public int divMW { get; set; }=0;
                #endregion
                
                #region Focus
                public double Fdx { get; set; }
                public int Fnx { get; set; } = 0;
                public double Fbx { get; set; }

                public double Fdy { get; set; }
                public int Fny { get; set; } = 0;
                public double Fby { get; set; }
                #endregion

                public double theta_all { get; set; }
            }

            public Parameter pm;
            public Coord m;
            double m_xc, m_yc;
            public double[] FigError { get; private set; }
            public double[] PhaseError { get; private set; }
            public Coord s;
            double s_xc, s_yc;
            public Coord[] f;
            double f_xc, f_yc;
            public double[,] fIntensity
            {
                get
                {
                    double[,] _fIntensity = new double[pm.Fnx, pm.Fny];
                    for (int i = 0; i < pm.Fnx; i++)
                    {
                        for (int j = 0; j < pm.Fny; j++)
                        {
                            _fIntensity[i, j] = f[i].Intensity[j];
                        }
                    }
                    return _fIntensity;
                }

            }

            public Mirror1D(Parameter _pm)
            {
                Init(_pm);
            }

            public void Init(Parameter _pm)
            {
                pm = _pm;

                curv = new double[pm.divML];
                theta = new double[pm.divML];
                FigError = new double[pm.divML];

                //ミラー
                m = new Coord(pm.divML);
                switch(pm.mirrorType)
                {
                    case Parameter.MirrorType.Ellipse:
                        Ellipse();
                        break;
                    case Parameter.MirrorType.Parabolic:
                        break;
                    case Parameter.MirrorType.Plane:
                        Plane();
                        break;
                }

                //光源
                if (pm.divSW != 0)
                    s = new Coord(pm.divSW);
                //焦点
                if (pm.Fnx != 0 && pm.Fny != 0)
                {
                    f = new Coord[pm.Fnx];
                    for (int i = 0; i < pm.Fnx; i++)
                        f[i] = new Coord(pm.Fny);
                }

                Move(-s_xc, -s_yc);
            }

            /// <summary>
            /// 平面ミラー
            /// </summary>
            void Plane()
            {
                
                pm.theta_s = (double)pm.pos * Math.Abs(Math.Atan(pm.L2 * Sin(2.0 * pm.theta_i) / (pm.L1 + pm.L2 * Math.Cos(2.0 * pm.theta_i))));
                pm.theta_f = Math.Abs((double)pm.pos * pm.theta_s - 2.0 * pm.theta_i);


                //光源中心座標
                s_xc = 0.0;
                s_yc = 0.0;
                //焦点中心座標
                f_xc = pm.L1 * Math.Cos(pm.theta_s) + pm.L2 * Math.Cos(pm.theta_f);
                f_yc = 0.0;
                //ミラー中心座標
                m_xc = pm.L1 * Math.Cos(pm.theta_s);
                m_yc = pm.L1 * Sin(pm.theta_s);

                //ミラーの一次方程式
                pm.a =Math.Tan( pm.theta_s + pm.theta_i);
                pm.b = m_yc - pm.a * m_xc;

                for(int i=0;i<pm.divML;i++)
                {
                    m.x[i] = m_xc - pm.ML / 2.0 + pm.ML / (pm.divML - 1.0) * i;
                    m.y[i] = x2y(m.x[i]);
                }
            }

            void Ellipse()
            {
                pm.a = (pm.L1 + pm.L2) / 2.0;
                pm.theta_s = (double)pm.pos * Abs(Atan(pm.L2 * Sin(2.0 * pm.theta_i) / (pm.L1 + pm.L2 * Math.Cos(2.0 * pm.theta_i))));
                pm.theta_f = Math.Abs((double)pm.pos * pm.theta_s - 2.0 * pm.theta_i);

                pm.f = (pm.L1 * Math.Cos(pm.theta_s) + pm.L2 * Math.Cos(pm.theta_f)) / 2.0;
                pm.b = Math.Sqrt(pm.a * pm.a - pm.f * pm.f);

                //光源中心座標
                s_xc = -pm.f;
                s_yc = 0.0;
                //焦点中心座標
                f_xc = pm.f;
                f_yc = 0.0;
                //ミラー中心座標
                m_xc = pm.L1 * Math.Cos(pm.theta_s) - pm.f;
                m_yc = (double)pm.pos * pm.b * Math.Sqrt(1 - Math.Pow((m_xc / pm.a), 2.0));

                for (int i = 0; i < pm.divML; i++)
                {
                    m.x[i] = m_xc - pm.ML / 2.0 + pm.ML / (pm.divML - 1.0) * i;
                    m.y[i] = x2y(m.x[i]);
                }
                //curvature
                curv = Curvature(m.x);

                //incidence angle
                theta = IncAngle(m.x);

                
            }

            public void SetSource(Parameter.SourceType sType,double _SW,int div)
            {
                pm.SW = _SW;

                s = new Coord(div);


                if (sType==Parameter.SourceType.Gaussian)
                {
                    double sigma = pm.SW / (2.0 * Math.Sqrt(2 * Math.Log(2.0)));
                    for (int i = 0; i < div; i++)
                    {
                        s.x[i] = s_xc;
                        s.y[i] = s_yc + 4.0 * (-div / 2.0 + i) * pm.SW / div;
                        s.u[i] = new Complex(Math.Exp(-s.y[i] * s.y[i] / (2.0 * sigma * sigma)) / (Math.Sqrt(2.0 * Math.PI) * sigma), 0.0);
                    }
                }
                else if(sType==Parameter.SourceType.Rectangle)
                {
                    for (int i = 0; i < div; i++)
                    {
                        s.x[i] = s_xc;
                        s.y[i] = s_yc + (-div / 2.0 + i) * pm.SW;
                        s.u[i] = new Complex(1.0, 0.0);
                    }
                }
                else
                {
                    s = new Coord(1);
                    s.x[0]= s_xc;
                    s.y[0] = s_yc;
                    s.u[0] = new Complex(1.0, 0.0);
                }
                s.Rot(pm.theta_s, s.xc, s.yc);

            }

            public void SetDetector(double Fdx,int Fnx,double Fbx,double Fdy,int Fny,double Fby)
            {
                pm.Fdx = Fdx;
                pm.Fdy = Fdy;
                pm.Fnx = Fnx;
                pm.Fny = Fny;
                pm.Fbx = Fbx;
                pm.Fby = Fby;

                f = new Coord[pm.Fnx];

                //まず回転していない座標で設定
                for (int i = 0; i < pm.Fnx; i++)
                {
                    f[i] = new Coord(pm.Fny);
                    for (int j = 0; j < pm.Fny; j++)
                    {
                        f[i].x[j] = f_xc + (-pm.Fnx / 2 + i) * pm.Fdx + pm.Fbx;
                        f[i].y[j] = f_yc + (-pm.Fny / 2 + j) * pm.Fdy + pm.Fby;
                    }
                }

                //光軸にあわせて回転
                //focus_xc,focus_ycとmirror_xc,mirror_ycの直線に対して傾ける
                double theta = Math.Atan((f_yc - m_yc) / (f_xc - m_xc));
                for (int i = 0; i < pm.Fnx; i++)
                    f[i].Rot(theta, f_xc, f_yc);
            }

            #region subroutin

            /// <summary>
            /// x座標to楕円のy
            /// </summary>
            /// <param name="x">x座標</param>
            /// <returns>楕円のy</returns>
            double x2y(double x)
            {
                double y = 0.0;

                switch (pm.mirrorType)
                {
                    case Parameter.MirrorType.Plane:
                        y = pm.a * x + pm.b;
                        break;
                    case Parameter.MirrorType.Ellipse:
                        y = (double)pm.pos * pm.b * Math.Sqrt(1.0 - Math.Pow(x / pm.a, 2.0));
                        break;
                    case Parameter.MirrorType.Parabolic:
                        break;
                }

                return y;
            }

            /// <summary>
            /// X座標toY座標
            /// </summary>
            /// <param name="x">X座標</param>
            /// <returns>Y座標</returns>
            double[] x2y(double[] x)
            {
                double[] y = new double[x.Length];

                for (int i = 0; i < x.Length; i++)
                {
                    y[i] = x2y(x[i]);
                }
                return y;
            }

            /// <summary>
            /// x座標to曲率
            /// </summary>
            /// <param name="x">x座標</param>
            /// <returns>楕円の曲率</returns>
            double Curvature(double x)
            {
                double c = 0.0;

                switch (pm.mirrorType)
                {
                    case Parameter.MirrorType.Plane:
                        break;
                    case Parameter.MirrorType.Ellipse:
                        double a2x2 = Math.Pow(pm.a, 2.0) - Math.Pow(x, 2.0);
                        c = pm.a / (pm.b * (-x * x / Math.Pow(a2x2, 3.0 / 2.0) - 1.0 / Math.Sqrt(a2x2)));
                        break;
                    case Parameter.MirrorType.Parabolic:
                        break;
                }

                return c;
            }

            /// <summary>
            /// x座標to曲率
            /// </summary>
            /// <param name="x">x座標</param>
            /// <returns>楕円の曲率</returns>
            double[] Curvature(double[] x)
            {
                double[] c = new double[x.Length];
                for (int i = 0; i < x.Length; i++)
                {
                    c[i] = Curvature(x[i]);
                }
                return c;
            }

            /// <summary>
            /// X座標to入射角
            /// </summary>
            /// <param name="x">X座標</param>
            /// <returns>入射角</returns>
            double IncAngle(double x)
            {
                double t = 0.0;

                switch(pm.mirrorType)
                {
                    case Parameter.MirrorType.Plane:
                        break;
                    case Parameter.MirrorType.Ellipse:
                        t = pm.b * x / (pm.a * Math.Sqrt(pm.a * pm.a - x * x));
                        break;
                    case Parameter.MirrorType.Parabolic:
                        break;

                }

                return t;
            }

            /// <summary>
            /// X座標to入射角
            /// </summary>
            /// <param name="x">X座標</param>
            /// <returns>入射角</returns>
            double[] IncAngle(double[] x)
            {
                if (x.Length != pm.divML) { }

                double[] t = new double[pm.divML];
                for (int i = 0; i < pm.divML; i++)
                {
                    t[i] = IncAngle(x[i]);
                }

                double inc_c = t[pm.divML / 2];
                for (int i = 0; i < pm.divML; i++)
                    t[i] -= inc_c - pm.theta_i;

                return t;
            }

            /// <summary>
            /// 一次方程式とミラー形状方程式のX交点
            /// </summary>
            /// <param name="al">一次方程式の傾き</param>
            /// <param name="be">一次方程式の切片</param>
            /// <returns>X座標</returns>
            double albe2x(double al, double be)
            {
                double x = 0.0;

                switch (pm.mirrorType)
                {
                    case Parameter.MirrorType.Plane:

                        break;
                    case Parameter.MirrorType.Ellipse:
                        x = (-al * be * pm.a * pm.a + pm.a * pm.b * Math.Sqrt(-be * be + al * al * pm.a * pm.a + pm.b * pm.b)) / (al * al * pm.a * pm.a + pm.b * pm.b);
                        break;
                    case Parameter.MirrorType.Parabolic:
                        break;
                }

                return x;
            }

            #endregion

            public void Move(double dx, double dy)
            {
                Move(ref s_xc, ref s_yc, dx, dy);
                Move(ref m_xc, ref m_yc, dx, dy);
                Move(ref f_xc, ref f_yc, dx, dy);

                if (s != null) s.Move(dx, dy);
                m.Move(dx, dy);
                if (f != null)
                    foreach (var _f in f) _f.Move(dx, dy);
            }

            public static void Move(ref double x,ref double y,double dx,double dy)
            {
                x += dx;
                y += dy;
            }

            public void Rot(double theta, double x0 = 0.0, double y0 = 0.0)
            {
                //基本座標の回転
                Rot(ref s_xc, ref s_yc, theta, x0, y0);
                Rot(ref m_xc, ref m_yc, theta, x0, y0);
                Rot(ref f_xc, ref f_yc, theta, x0, y0);

                if (s != null)
                    s.Rot(theta, x0, y0);
                //ミラーの回転
                m.Rot(theta, x0, y0);
                //焦点の回転
                if (f != null)
                    foreach (var _f in f) _f.Rot(theta, x0, y0);
            }

            public static void Rot(ref double x, ref double y, double theta, double x0, double y0)
            {
                double x_buf = x - x0;
                double y_buf = y - y0;
                x = x_buf * Math.Cos(theta) - y_buf * Sin(theta) + x0;
                y = x_buf * Sin(theta) + y_buf * Math.Cos(theta) + y0;

            }

            public void PlusError(double[] _Ey)
            {

                double[] subError = new double[pm.divML];
                FigError.CopyTo(subError, 0);

                if (pm.divML != _Ey.Length)
                {
                    //Error = new double[pm.divML];

                    double[] _Ex = new double[_Ey.Length];
                    for (int i = 0; i < _Ey.Length; i++)
                    {
                        _Ex[i] = (double)i;
                    }

                    for (int i = 0; i < pm.divML; i++)
                        FigError[i] = Interpolate.CubicSpline(_Ex, _Ey).Interpolate(_Ey.Length / (double)pm.divML * i);
                }
                else
                    FigError = _Ey;

                for (int i = 0; i < pm.divML; i++)
                {
                    m.y[i] += FigError[i];
                    FigError[i] += subError[i];
                }
            }

        }
    }

    namespace WaveField
    {
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


        class WaveField
        {
            const double h = Constants.PlancksConstant;     //6.62606896e-34
            const double e = Constants.ElementaryCharge;    //1.602176487e-19
            const double c = Constants.SpeedOfLight;        //2.99792458e8

            double _lambda = 0.0;
            public double lambda
            {
                get { return _lambda; }
                set
                {
                    _lambda = value;
                    k = 2.0 * PI / value;
                }
            }

            public double Energy
            {
                get { return h * c / (e * lambda); }
                set { lambda = h * c / (e * value); }
            }

            public double k { get; private set; }

            int _core;
            public int core
            {
                get { return _core; }
                set
                {
                    if (value > Environment.ProcessorCount)
                        _core = Environment.ProcessorCount;
                    else if (value < 1)
                        _core = 1;
                    else
                        _core = value;
                }
            }

            public int totalNum { get; private set; }
            int doneNum;
            string message { get; set; }

            IProgress<ProgressInfo> progress;
            public CancellationToken ct { get; set; }

            public enum PropDir { Forward = -1, Backward = 1 }

            //コンストラクタ
            public WaveField(double _lambda)
            {
                lambda = _lambda;
            }

            public WaveField()
            { }

            //FWHM計算
            public static double FWHM(Mirror1D.Coord f)
            {
                int div = f.Intensity.Length;
                double[] PosX = new double[div];
                double max = 0.0;
                double iMax = 0;
                for (int i = 0; i < div; i++)
                {
                    if (f.Intensity[i] > max)
                    {
                        max = f.Intensity[i];
                        iMax = i;
                    }
                    PosX[i] = Math.Sqrt(Math.Pow(f.y[i] - f.y[0], 2.0) + Math.Pow(f.x[i] - f.x[0], 2.0));
                }

                double dblePos1 = 0.0;
                double dblePos2 = 0.0;
                for (int i = 1; i < iMax; i++)
                    if (max / 2.0 < f.Intensity[i])
                    {
                        dblePos1 = (PosX[i] - PosX[i - 1]) / (f.Intensity[i] - f.Intensity[i - 1]) * (max / 2.0 - f.Intensity[i]) + PosX[i];
                        break;
                    }
                for (int i = div - 2; i > iMax; i--)
                    if (max / 2.0 < f.Intensity[i])
                    {
                        dblePos2 = (PosX[i] - PosX[i + 1]) / (f.Intensity[i] - f.Intensity[i + 1]) * (max / 2.0 - f.Intensity[i]) + PosX[i];
                        break;
                    }

                return dblePos2 - dblePos1;
            }

            #region 並列計算ver

            public void ForwardPropagation(Mirror1D.Coord Opt1,ref Mirror1D.Coord Opt2)
            {
                Complex[] _u;
                ForwardPropagation(Opt1.x, Opt1.y, Opt1.u, Opt2.x, Opt2.y, out _u);
                Opt2.u = _u;
                InP(ref Opt2);
                _u = null;
            }

            public void ForwardPropagation(double[] x1, double[] y1, Complex[] u1, double[] x2, double[] y2, out Complex[] u2)
            {
                if (lambda == 0.0)
                {
                    throw new InvalidOperationException("波長orエネルギーが設定されていません");
                }

                if(x1.Length!=y1.Length||x1.Length!=u1.Length)
                {
                    throw new ArgumentException();
                }
                int div1 = x1.Length;

                if (x2.Length != y2.Length)
                {
                    throw new ArgumentException();
                }
                int div2 = x2.Length;

                #region 伝播元の微小長さの計算
                double[] ds = new double[div1];

                //伝播元の微小長さの計算
                if (div1 != 1)
                {
                    for (int i = 1; i < div1; i++)
                    {
                        ds[i] = Math.Sqrt(Math.Pow(x1[i] - x1[i - 1], 2.0) + Math.Pow(y1[i] - y1[i - 1], 2.0));
                    }
                    ds[0] = ds[1];
                }
                else
                {
                    ds[0] = 1;
                }
                #endregion

                #region 伝播計算
                Complex[] _u2 = new Complex[div2];

#if CPP
                cppWaveField.WF.fProp(lambda, x1, y1, u1, x2, y2,_u2);
#else
                Parallel.For(0, div2, i =>
                {
                    _u2[i] = new Complex(0.0, 0.0);
                    for (int j = 0; j < div1; j++)
                    {
                        _u2[i] += u1[j] * Complex.Exp(-Complex.ImaginaryOne * k * Math.Sqrt(Math.Pow(x2[i] - x1[j], 2.0) + Math.Pow(y2[i] - y1[j], 2.0)))
                                        / Math.Sqrt(Math.Pow(x2[i] - x1[j], 2.0) + Math.Pow(y2[i] - y1[j], 2.0)) * ds[j];
                    }
                });
#endif
                u2 = _u2;

#endregion

            }

            public static void InP(ref Mirror1D.Coord coord)
            {
                double[] _Intensity;
                double[] _Phase;
                InP(coord.u, out _Intensity, out _Phase);
                coord.Intensity = _Intensity;
                coord.Phase = _Phase;

            }

            public static void InP(Complex[] u, out double[] Intensity, out double[] Phase)
            {
                int div = u.Length;
                //強度計算
                Intensity = new double[div];
                Phase = new double[div];
                for (int i = 0; i < div; i++)
                {
                    Intensity[i] = Math.Pow(u[i].Magnitude, 2.0);
                    Phase[i] = u[i].Phase;
                }
            }
#endregion
        }

    }


}
