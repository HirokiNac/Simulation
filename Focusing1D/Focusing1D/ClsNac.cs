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

namespace ClsNac
{

    namespace Mirror
    {

        #region interface
        interface ICoord
        {
            int div { get; }

            double[] x { get; set; }
            double[] y { get; set; }

            Complex[] u { get; set; }
        }

        interface ICoordF
        {
            int n { get; }
            int div { get; }

            double[][] x { get; set; }
            double[][] y { get; set; }

            Complex[][] u { get; set; }
        }
        #endregion

        #region 座標

        /// <summary>
        /// 光源，暫定焦点座標(一次元)
        /// </summary>
        public class Coord : ICoord
        {
            #region 変数
            /// <summary>分割数</summary>
            public int div { get; private set; }

            /// <summary>X中心座標</summary>
            public double xc { get { return x[x.Length / 2]; } }
            /// <summary>Y中心座標</summary>
            public double yc { get { return y[y.Length / 2]; } }

            /// <summary>X座標</summary>
            public double[] x { get; set; }

            /// <summary>Y座標</summary>
            public double[] y { get; set; }

            /// <summary>補正X座標</summary>
            public double[] xmod
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
            public double[] ymod
            {
                get
                {
                    double[] _ymod = new double[div];
                    var p = Fit.Line(x, y);
                    double min = 1e10;
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

            /// <summary>強度分布</summary>
            public double[] Intensity
            {
                get
                {
                    double[] _Intensity = new double[div];
                    for (int i = 0; i < div; i++)
                        _Intensity[i] = Math.Pow(u[i].Magnitude, 2.0);
                    return _Intensity;
                }
            }

            /// <summary>位相分布</summary>
            public double[] Phase
            {
                get
                {
                    double[] _Phase = new double[div];
                    for (int i = 0; i < div; i++)
                        _Phase[i] = u[i].Phase;
                    return _Phase;
                }
            }

            #endregion

            #region constructor
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="_mode">モード Source, Mirror, Focus</param>
            /// <param name="_div">分割数</param>
            public Coord(int _div)
            {
                Initialize(_div);
            }

            /// <summary>
            /// コンストラクタ 分割なし
            /// </summary>
            /// <param name="_mode">モード Source, Mirror, Focus</param>
            public Coord() : this(1) { }

            /// <summary>
            /// 初期化
            /// </summary>
            /// <param name="_mode">モード Source, Mirror, Focus</param>
            /// <param name="_div">分割数</param>
            internal void Initialize(int _div)
            {
                div = _div;

                x = new double[div];
                y = new double[div];
                u = new Complex[div];
            }

            #endregion

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
                    x[i] = x_buf * Math.Cos(rotTheta) - y_buf * Math.Sin(rotTheta) + x0;
                    y[i] = x_buf * Math.Sin(rotTheta) + y_buf * Math.Cos(rotTheta) + y0;
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

        /// <summary>
        /// ミラー座標
        /// </summary>
        public class CoordM : Coord
        {
            public CoordM(int _div, double _ML)
                : base(_div)
            {
                theta = new double[div];
                curv = new double[div];
                ML = _ML;
            }

            public double ML_NA { get { return Math.Abs(x[div - 1] - x[0]); } }

            public double ML { get; private set; }

            /// <summary>ミラー長さ基準補正x座標</summary>
            public double[] xmod2
            {
                get
                {
                    double[] _xmod = new double[div];
                    for (int i = 0; i < div; i++)
                        _xmod[i] = x[i] - xc + ML / 2.0;
                    return _xmod;
                }
            }


            /// <summary>入射角分布</summary>
            public double[] theta { get; set; }

            /// <summary>曲率分布</summary>
            public double[] curv { get; set; }

            public Coord ToCoord()
            {
                Coord c = new Coord(div);
                c.x = x;
                c.y = y;
                c.u = u;
                return c;
            }

            public void CoordTo(Coord _c)
            {
                x = _c.x;
                y = _c.y;
                u = _c.u;
            }

        }

        /// <summary>
        /// 焦点座標(二次元)
        /// </summary>
        public class CoordF : ICoordF
        {
            #region 変数
            #region 座標
            /// <summary>光軸方向分割数</summary>
            public int n { get; private set; }
            /// <summary>垂直方向分割数</summary>
            public int div { get; private set; }

            public double xc { get { return x[n / 2][div / 2]; } }
            public double yc { get { return y[n / 2][div / 2]; } }

            /// <summary>x座標</summary>
            public double[][] x { get; set; }

            /// <summary>y座標</summary>
            public double[][] y { get; set; }

            public double[] xmod { get; set; }
            public double[] ymod { get; set; }

            public double dx { get; private set; }
            public double dy { get; private set; }

            public double bx { get; private set; }
            public double by { get; private set; }
            #endregion
            #region 波動光学
            /// <summary></summary>
            public Complex[][] u { get; set; }

            /// <summary>強度</summary>
            public double[][] IntensityJagged { get; private set; }

            /// <summary>強度</summary>
            public double[,] Intensity { get; private set; }

            /// <summary>位相</summary>
            public double[][] PhaseJagged { get; set; }

            /// <summary>位相</summary>
            public double[,] Phase { get; set; }

            public double[] FWHM1 { get; private set; }
            public double minFWHM1 { get; private set; }
            public int iMinFWHM1 { get; private set; }

            public double[] FWHM2 { get; private set; }
            public double minFWHM2 { get; private set; }
            public int iMinFWHM2 { get; private set; }

            static Tuple<double, double> FWHM(double[] Position, double[] Intensity)
            {
                if (Position.Length > 2)
                {
                    double max = 0.0;
                    int iMax = 0;
                    for (int i = 0; i < Position.Length; i++)
                    {
                        if (Intensity[i] > max)
                        {
                            max = Intensity[i];
                            iMax = i;
                        }
                    }

                    #region FWHM1
                    //外側からピークトップにかけて
                    //カウント/2以上になる境目を探す
                    double dblePos1 = 0.0;
                    double dblePos2 = 0.0;
                    for (int i = 1; i <= iMax; i++)
                    {
                        if (max / 2.0 < Intensity[i])
                        {
                            dblePos1 = (Position[i] - Position[i - 1]) / (Intensity[i] - Intensity[i - 1]) * (max / 2.0 - Intensity[i]) + Position[i];
                            break;
                        }
                    }
                    for (int i = Position.Length - 2; i >= iMax; i--)
                    {
                        if (max / 2.0 < Intensity[i])
                        {
                            dblePos2 = (Position[i] - Position[i + 1]) / (Intensity[i] - Intensity[i + 1]) * (max / 2.0 - Intensity[i]) + Position[i];
                            break;
                        }
                    }
                    double FWHM1 = Math.Abs(dblePos1 - dblePos2);
                    #endregion

                    #region FWHM2
                    //ピークトップから外側にかけて
                    //カウント/2以下になる境目を探す
                    dblePos1 = 0.0;
                    dblePos2 = 0.0;
                    for (int i = iMax - 1; i >= 0; i--)
                    {
                        if (max / 2.0 > Intensity[i])
                        {
                            dblePos1 = (Position[i] - Position[i + 1]) / (Intensity[i] - Intensity[i + 1]) * (max / 2.0 - Intensity[i]) + Position[i];
                            break;
                        }
                    }
                    for (int i = iMax + 1; i <= Position.Length - 1; i++)
                    {
                        if (max / 2.0 > Intensity[i])
                        {
                            dblePos2 = (Position[i] - Position[i - 1]) / (Intensity[i] - Intensity[i - 1]) * (max / 2.0 - Intensity[i]) + Position[i];
                            break;
                        }
                    }
                    double FWHM2 = Math.Abs(dblePos1 - dblePos2);
                    #endregion

                    return Tuple.Create<double, double>(FWHM1, FWHM2);
                }
                else
                    return Tuple.Create<double, double>(0.0, 0.0);

            }
            #endregion

            public double DoF { get; private set; }
            
            #endregion


            #region constructor
            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="_n">焦点方向</param>
            /// <param name="_div"></param>
            public CoordF(int _n, int _div, double _dx, double _dy)
            {
                Initialize(_n, _div, _dx, _dy);
            }

            public CoordF(int _div)
                : this(1, _div, 0.0, 0.0)
            { }

            public void Initialize(int _n, int _div, double _dx, double _dy)
            {
                n = _n;
                div = _div;
                dx = _dx;
                dy = _dy;

                x = new double[n][];
                y = new double[n][];
                u = new Complex[n][];
                for (int i = 0; i < n; i++)
                {
                    x[i] = new double[div];
                    y[i] = new double[div];
                    u[i] = new Complex[div];
                }

                ymod = new double[div];
                for (int j = 0; j < div; j++)
                    ymod[j] = dy * j - dy * (div - 1) / 2.0;

            }

            public void Initialize(int _div)
            {
                Initialize(1, _div, 0.0, 0.0);
            }
            #endregion

            public void Finish()
            {
                #region intensity
                Intensity = new double[n, div];
                IntensityJagged = new double[n][];
                Parallel.For(0, n, i =>
                {
                    IntensityJagged[i] = new double[div];
                    for (int j = 0; j < div; j++)
                    {
                        Intensity[i, j] = Math.Pow(u[i][j].Magnitude, 2.0);
                        IntensityJagged[i][j] = Intensity[i, j];
                    }
                });
                #endregion

                #region phase
                Phase = new double[n, div];
                PhaseJagged = new double[n][];
                Parallel.For(0, n, i =>
                {
                    PhaseJagged[i] = new double[div];
                    for (int j = 0; j < div; j++)
                    {
                        Phase[i, j] = u[i][j].Phase;
                        PhaseJagged[i][j] = Phase[i, j];
                    }
                });


                #endregion

                #region fwhm
                FWHM1 = new double[n];
                FWHM2 = new double[n];

                minFWHM1 = 1e15;
                iMinFWHM1 = 0;
                minFWHM2 = 1e15;
                iMinFWHM2 = 0;

                for (int i = 0; i < n; i++)
                {
                    var fwhm = FWHM(ymod, IntensityJagged[i]);
                    FWHM1[i] = fwhm.Item1;
                    FWHM2[i] = fwhm.Item2;
                    if (FWHM1[i] < minFWHM1)
                    {
                        minFWHM1 = FWHM1[i];
                        iMinFWHM1 = i;
                    }
                    if (FWHM2[i] < minFWHM2)
                    {
                        minFWHM2 = FWHM2[i];
                        iMinFWHM2 = i;
                    }
                }
                #endregion
            }

            #region 補正

            #region 座標変換
            /// <summary>
            /// xy座標中心回転
            /// </summary>
            /// <param name="theta">回転角度</param>
            public void Rot(double theta)
            {
                Rot(theta, xc, yc);
            }

            /// <summary>
            /// xy座標回転
            /// </summary>
            /// <param name="rotTheta"></param>
            /// <param name="x0"></param>
            /// <param name="y0"></param>
            public void Rot(double rotTheta, double x0, double y0)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < div; j++)
                    {
                        double x_buf = x[i][j] - x0;
                        double y_buf = y[i][j] - y0;
                        x[i][j] = x_buf * Math.Cos(rotTheta) - y_buf * Math.Sin(rotTheta) + x0;
                        y[i][j] = x_buf * Math.Sin(rotTheta) + y_buf * Math.Cos(rotTheta) + y0;
                    }
                }

            }

            /// <summary>
            /// xy座標移動
            /// </summary>
            /// <param name="dx">x移動量</param>
            /// <param name="dy">y移動量</param>
            public void Move(double dx, double dy)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < div; j++)
                    {
                        x[i][j] += dx;
                        y[i][j] += dy;
                    }
                }

            }
            #endregion

            #region cooord変換
            /// <summary>
            /// CoordFをCoordに変換
            /// </summary>
            /// <param name="_n">光軸方向位置</param>
            /// <returns></returns>
            public Coord ToCoord(int _n)
            {
                Coord c = new Coord(div);
                c.x = x[_n];
                c.y = y[_n];
                c.u = u[_n];
                return c;
            }

            /// <summary>
            /// CoordFをCoordに変換
            /// </summary>
            /// <returns></returns>
            public Coord ToCoord()
            {
                return ToCoord(n / 2);
            }

            public void CoordTo(int _n, Coord _c)
            {
                for (int j = 0; j < div; j++)
                {
                    x[_n][j] = _c.x[j];
                    y[_n][j] = _c.y[j];
                    u[_n][j] = _c.u[j];
                }
            }
            #endregion

            #endregion

        }

        #endregion

        namespace Figure
        {
                public enum mType { Ell, Para }
                public enum mDiv { Angle, Even }
                public enum mPos : int { Lower = -1, Upper = 1 }


            public class Mirror1D
            {

                #region 宣言

                mType mtype;
                mDiv mdiv;
                mPos mpos;


                public double ell_a { get; private set; }
                public double ell_b { get; private set; }
                public double ell_f { get; private set; }
                public double NA { get; set; }
                double L1 { get; set; }
                double L2 { get; set; }
                double theta_i, theta_s, theta_f;
                public double ML { get; private set; }
                public int div { get; private set; }
                double dtheta { get; set; }

                const double tol_ML = 1e-6;
                const int tol_max = 1000;

                #region coord
                /// <summary>
                /// 光源座標
                /// </summary>
                public Coord s;
                /// <summary>
                /// ミラー座標
                /// </summary>
                internal CoordM m;
                /// <summary>
                /// 焦点座標
                /// </summary>
                internal Coord f;

                /// <summary>
                /// 波動光学計算用ミラー座標
                /// </summary>
                internal CoordM mw;
                /// <summary>
                /// 波動光学計算用焦点座標
                /// </summary>
                internal CoordF fw;

                #endregion

                #endregion

                /// <summary>
                /// コンストラクタ
                /// </summary>
                /// <param name="_L1"></param>
                /// <param name="_L2"></param>
                /// <param name="_theta_i"></param>
                /// <param name="_div"></param>
                /// <param name="_ML"></param>
                /// <param name="mtype"></param>
                /// <param name="mdiv"></param>
                /// <param name="mpos"></param>
                internal Mirror1D(double _L1, double _L2,
                    double _theta_i, int _div, double _ML,
                    mType _mtype, mDiv _mdiv, mPos _mpos)
                {
                    L1 = _L1;
                    L2 = _L2;
                    theta_i = _theta_i;
                    theta_s = 0.0;
                    theta_f = 0.0;
                    ML = _ML;
                    div = _div;
                    dtheta = 0.0;

                    s = new Coord();
                    f = new Coord();



                    m = new CoordM(div, ML);
                    mw = new CoordM(div, ML);

                    mtype = _mtype;
                    mdiv = _mdiv;
                    mpos = _mpos;

                    if (mtype == mType.Ell)
                    {
                        Ellipse();
                    }
                }

                #region ellipse

                void Ellipse()
                {
                    ell_a = (L1 + L2) / 2.0;
                    theta_s = (double)mpos * Math.Abs(Math.Atan(L2 * Math.Sin(2.0 * theta_i)
                        / (L1 + L2 * Math.Cos(2.0 * theta_i))));
                    theta_f = Math.Abs((double)mpos * theta_s - 2.0 * theta_i);

                    ell_f = (L1 * Math.Cos(theta_s) + L2 * Math.Cos(theta_f)) / 2.0;
                    ell_b = Math.Sqrt(ell_a * ell_a - ell_f * ell_f);
                    s.x[0] = -ell_f;
                    s.y[0] = 0.0;

                    f.x[0] = ell_f;
                    f.y[0] = 0.0;

                    double m_xc = L1 * Math.Cos(theta_s) - ell_f;
                    double m_yc = (double)mpos * ell_b * Math.Sqrt(1 - Math.Pow((m_xc / ell_a), 2.0));


                    if (mdiv == mDiv.Even)
                        even();
                    else
                        angle();

                }

                #region subroutin1
                void even()
                {
                    double m_xc = L1 * Math.Cos(theta_s) - ell_f;

                    for (int i = 0; i < div; i++)
                    {
                        m.x[i] = m_xc - ML / 2.0 + ML / (div - 1.0) * i;
                        m.y[i] = ell_y(m.x[i]);

                        //curvature
                        m.curv[i] = curv(m.x[i]);

                        //incidence angle
                        m.theta[i] = ell_b * m.x[i] / (ell_a * Math.Sqrt(ell_a * ell_a - m.x[i] * m.x[i]));
                    }
                    double inc_c = m.theta[m.div / 2];
                    for (int i = 0; i < m.div; i++)
                        m.theta[i] -= inc_c - theta_i;


                }

                void angle()
                {
                    double m_xc = L1 * Math.Cos(theta_s) - ell_f;
                    double m_yc = (double)mpos * ell_b * Math.Sqrt(1 - Math.Pow((m_xc / ell_a), 2.0));

                    double x0 = m_xc - ML / 2.0;
                    double xn = m_xc + ML / 2.0;
                    double y0 = ell_y(x0);
                    double yn = ell_y(xn);

                    //焦点から見たときの角度

                    //ミラー中心への光線角度
                    double theta_c = m_yc / (m_xc + ell_f);
                    //ミラー上流端への光線角度
                    double theta_0 = y0 / (x0 + ell_f);
                    //ミラー下流端への光線角度
                    double theta_n = yn / (xn + ell_f);

                    double theta_d0n = Math.Abs(theta_n - theta_0) / 2.0;

                    theta_0 = theta_c - theta_d0n;
                    theta_n = theta_c + theta_d0n;

                    double tol_theta = tol_ML / L1;
                    int n = 0;
                    //ミラー長さが許容値以内になるまで角度を調整
                    while (true)
                    {
                        n++;
                        x0 = albe2x(theta_0, ell_f * theta_0);
                        xn = albe2x(theta_n, ell_f * theta_n);
                        //System.Diagnostics.Debug.Print((xn - x0 - ML).ToString());

                        if (Math.Abs(xn - x0 - ML) > tol_ML)
                        {
                            theta_d0n = Math.Abs(xn - x0) - ML < 0.0
                                ? -tol_theta
                                : tol_theta;
                            theta_0 += theta_d0n;
                            theta_n -= theta_d0n;
                        }
                        else if (n > tol_max)
                        {

                            return;
                        }
                        else
                            break;
                    }

                    for (int i = 0; i < div; i++)
                    {
                        double al = theta_0 - (theta_0 - theta_n) / (div - 1.0) * i;
                        double be = ell_f * al;

                        //x and y
                        m.x[i] = albe2x(al, be);
                        m.y[i] = ell_y(m.x[i]);

                        //curvature
                        m.curv[i] = curv(m.x[i]);

                        //incidence angle
                        m.theta[i] = ell_b * m.x[i] / (ell_a * Math.Sqrt(ell_a * ell_a - m.x[i] * m.x[i]));
                    }
                    double inc_c = m.theta[m.div / 2];
                    for (int i = 0; i < m.div; i++)
                        m.theta[i] -= inc_c - theta_i;

                }
                #endregion

                #region subroutin
                /// <summary>
                /// 楕円のyを返す
                /// </summary>
                /// <param name="x">x座標</param>
                /// <returns>y座標</returns>
                double ell_y(double x)
                {
                    return (double)mpos * ell_b * Math.Sqrt(1.0 - Math.Pow(x / ell_a, 2.0));
                }

                /// <summary>
                /// 一次方程式と楕円の交点
                /// </summary>
                /// <param name="al">傾き</param>
                /// <param name="be">切片</param>
                /// <returns>x座標</returns>
                double albe2x(double al, double be)
                {
                    return (-al * be * ell_a * ell_a + ell_a * ell_b *
                        Math.Sqrt(-be * be + al * al * ell_a * ell_a + ell_b * ell_b))
                        / (al * al * ell_a * ell_a + ell_b * ell_b);
                }

                /// <summary>
                /// x座標to楕円のy
                /// </summary>
                /// <param name="x">x座標</param>
                /// <returns>楕円のy</returns>
                double[] ell_y(double[] x)
                {
                    double[] y = new double[x.Length];

                    for (int i = 0; i < x.Length; i++)
                    {
                        y[i] = ell_y(x[i]);
                    }
                    return y;
                }

                /// <summary>
                /// x座標to楕円の曲率
                /// </summary>
                /// <param name="x">x座標</param>
                /// <returns>楕円の曲率</returns>
                double curv(double x)
                {
                    double a2x2 = Math.Pow(ell_a, 2.0) - Math.Pow(x, 2.0);
                    return ell_a / (this.ell_b * (-x * x / Math.Pow(a2x2, 3.0 / 2.0) - 1.0 / Math.Sqrt(a2x2)));
                }

                #endregion

                #endregion

                #region Source&Detector
                /// <summary>
                /// 光源座標設定
                /// </summary>
                /// <param name="n">分割数</param>
                /// <param name="d">ピッチ</param>
                public void Source(int n, double d,bool gauss=false)
                {
                    double s_xc = s.xc;
                    double s_yc = s.yc;

                    s.Initialize(n);



                    if (gauss)
                    {
                        double sigma = d / (2.0 * Math.Sqrt(2 * Math.Log(2.0)));
                        for (int i = 0; i < n; i++)
                        {
                            s.x[i] = s_xc;
                            s.y[i] = s_yc + 4.0 * (-n / 2.0 + i) * d / n;
                            s.u[i] = new Complex(Math.Exp(-s.y[i] * s.y[i] / (2.0 * sigma * sigma)) / (Math.Sqrt(2.0 * Math.PI) * sigma), 0.0);
                            //System.Diagnostics.Debug.Print(s.y[i].ToString() + "," + s.u[i].Real.ToString());
                        }
                    }
                    else
                    {
                        for (int i = 0; i < n; i++)
                        {
                            s.x[i] = s_xc;
                            s.y[i] = s_yc + (-n / 2.0 + i) * d;
                            s.u[i] = new Complex(1.0, 0.0);
                        }
                    }
                    s.Rot(theta_s, s.xc, s.yc);
                }

                /// <summary>
                /// ディテクター座標設定
                /// </summary>
                /// <param name="nx">x方向分割数</param>
                /// <param name="dx">x方向ピッチ</param>
                /// <param name="ny">y方向分割数</param>
                /// <param name="dy">y方向ピッチ</param>
                /// <param name="bx">x方向ずれ</param>
                /// <param name="by">y方向ずれ</param>
                public void Detector(int nx, double dx, int ny, double dy, double bx = 0.0, double by = 0.0)
                {

                    fw = new CoordF(nx, ny, dx, dy);

                    double[][] fw_x = new double[fw.n][];
                    double[][] fw_y = new double[fw.n][];

                    //まず回転していない座標で設定
                    //表示用ディテクター座標設定 focus_x2
                    for (int i = 0; i < nx; i++)
                    {
                        fw_x[i] = new double[fw.div];
                        fw_y[i] = new double[fw.div];
                        for (int j = 0; j < ny; j++)
                        {
                            fw_x[i][j] = f.xc + (-nx / 2 + i) * dx;
                            fw_y[i][j] = f.yc + (-ny / 2 + j) * dy;
                            //focus_x2[i][j] = Math.Sqrt(Math.Pow(focus_x[i][j] - focus_x[i][0], 2.0) + Math.Pow(focus_y[i][j] - focus_y[i][0], 2.0));
                        }
                    }
                    fw.x = fw_x;
                    fw.y = fw_y;
                    //光軸にあわせて回転
                    //focus_xc,focus_ycとmirror_xc,mirror_ycの直線に対して傾ける
                    double theta = (f.yc - m.yc) / (f.xc - m.xc);
                    fw.Move(bx * Math.Cos(theta) + by * Math.Sin(theta), bx * Math.Sin(theta) - by * Math.Sin(theta));
                    fw.Rot(theta);
                }
                #endregion



                public void PlusError(double[] Error)
                {
                    if (Error.Length != div)
                    {
                        //補完する
                        double dx = m.ML_NA / (Error.Length-1);
                        double[] x = new double[Error.Length];
                        for (int i = 0; i < Error.Length; i++)
                            x[i] = dx * i;
                        double[] xmod = m.xmod;
                        for (int i = 0; i < div; i++)
                        {
                            double e = MathNet.Numerics.Interpolate.CubicSpline(x, Error).Interpolate(xmod[i]);
                            m.y[i] += e;
                        }
                    }
                    else
                    {
                        for(int i=0;i<div;i++)
                        { m.y[i] += Error[i]; }
                    }

                }

                #region 補正
                /// <summary>一段目のミラー形状再計算
                /// 波動光学計算用座標
                /// </summary>
                public void ReCalcMirror()
                {
                    //
                    this.mw.x = (double[])this.m.x.Clone();
                    this.mw.y = (double[])this.m.y.Clone();

                    //前段ミラーの座標を焦点原点に変換+f
                    this.movCoord(RotMovFactor.All, this.ell_f);

                    //ミラー上流端からの光線
                    double a0 = (f.yc - m.y[0]) / (f.xc - m.x[0]);
                    //ミラー下流端からの光線
                    double an = (f.yc - m.y[div - 1]) / (f.xc - m.x[div - 1]);
                    //後段ミラー座標系での切片
                    double an_source = (s.yc - m.y[div - 1]) / (s.xc - m.x[div - 1]);

                    NA = na((double)mpos, ell_a, ell_b, an_source, an_source * ell_f, mw.x[0] - ell_f, mw.y[0]);

                }

                /// <summary>二段目以降のミラー形状再計算
                /// 波動光学計算用座標
                /// </summary>
                /// <param name="M1">前段ミラー</param>
                /// <param name="M2">後段ミラー</param>
                public static void ReCalcMirror(Mirror1D M1, Mirror1D M2)
                {
                    //upMのL2とdownMのL1の角度の違いdthetaを求める
                    M2.dtheta = -M2.theta_s + M1.theta_f;

                    //ミラー上流端からの光線傾き
                    double _a0 = (M1.f.yc - M1.m.y[0]) / (M1.f.xc - M1.m.x[0]) -Math.Tan( M2.dtheta);
                    double a0 = -((M1.s.yc - M1.m.y[0]) / (M1.s.xc - M1.m.x[0]) + 2 * ((M1.m.x[0] - M1.ell_f) * M1.ell_b * M1.ell_b) / (M1.m.y[0] * M1.ell_a * M1.ell_a)) - Math.Tan(M2.dtheta);
                    //ミラー下流端からの光線傾き
                    double _an = (M1.f.yc - M1.m.y[M1.div - 1]) / (M1.f.xc - M1.m.x[M1.div - 1]) - Math.Tan(M2.dtheta);
                    double an = -((M1.s.yc - M1.m.y[M1.div - 1]) / (M1.s.xc - M1.m.x[M1.div - 1]) + 2.0 * ((M1.m.x[M1.div - 1] - M1.ell_f) * M1.ell_b * M1.ell_b) / (M1.m.y[M1.div - 1] * M1.ell_a * M1.ell_a)) - Math.Tan(M2.dtheta);

                    //後段ミラー座標系での切片
                    double _x0 = M1.m.x[0];
                    double _xn = M1.m.x[M1.div - 1];
                    double _y0 = M1.m.y[0];
                    double _yn = M1.m.y[M1.div - 1];

                    rotXY(ref _x0, ref _y0, -M2.dtheta, M1.f.xc, M1.f.yc);
                    rotXY(ref _xn, ref _yn, -M2.dtheta, M1.f.xc, M1.f.yc);

                    //ミラー上流端からの光線
                    //double b0 = -a0 * M2.s.xc;
                    double b0 =_y0- a0 * (M2.s.xc-(M1.f.xc-_x0));
                    //ミラー下流端からの光線
                    //double bn = -an * M2.s.xc;
                    double bn =_yn- an * (M2.s.xc - (M1.f.xc - _xn));

                    double x0 = (-a0 * b0 * Math.Pow(M2.ell_a, 2.0)
                                + Math.Sqrt(-Math.Pow(b0 * M2.ell_a * M2.ell_b, 2.0) + Math.Pow(a0 * M2.ell_a * M2.ell_a * M2.ell_b, 2.0) + Math.Pow(M2.ell_a * M2.ell_b * M2.ell_b, 2.0)))
                                / (Math.Pow(a0 * M2.ell_a, 2.0) + Math.Pow(M2.ell_b, 2.0));
                    double y0 = M2.ell_y(x0);
                    double xn = (-an * bn * Math.Pow(M2.ell_a, 2.0)
                                + Math.Sqrt(-Math.Pow(bn * M2.ell_a * M2.ell_b, 2.0) + Math.Pow(an * M2.ell_a * M2.ell_a * M2.ell_b, 2.0) + Math.Pow(M2.ell_a * M2.ell_b * M2.ell_b, 2.0)))
                                / (Math.Pow(an * M2.ell_a, 2.0) + Math.Pow(M2.ell_b, 2.0));
                    double yn = M2.ell_y(xn);

                    M2.mw.x = new double[M2.div];
                    M2.mw.y = new double[M2.div];

                    if (M2.mdiv == mDiv.Angle)
                    {
                        double theta_0 = y0 / (x0 + M2.ell_f);
                        double theta_n = yn / (xn + M2.ell_f);

                        for (int i = 0; i < M2.div; i++)
                        {
                            //前段ミラーからの光線の式を後段ミラーの座標に変換
                            //楕円と光線の交点を求める
                            double al = theta_0 - (theta_0 - theta_n) / (M2.div - 1.0) * i;
                            double be = M2.ell_f * al;
                            M2.mw.x[i] = (-al * be * M2.ell_a * M2.ell_a + M2.ell_a * M2.ell_b * Math.Sqrt(-be * be + al * al * M2.ell_a * M2.ell_a + M2.ell_b * M2.ell_b))
                                / (al * al * M2.ell_a * M2.ell_a + M2.ell_b * M2.ell_b);
                            M2.mw.y[i] = M2.ell_y(M2.mw.x[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < M2.div; i++)
                        {
                            M2.mw.x[i] = x0 + (xn - x0) / ((double)M2.div - 1.0) * i;
                            M2.mw.y[i] = M2.ell_y(M2.mw.x[i]);
                        }

                    }

                    //NA
                    M2.NA = na((double)M2.mpos, M2.ell_a, M2.ell_b, an, bn, x0, y0);


                    //光源中心に後段ミラー全座標をdthetaだけ回転する
                    M2.RotCoord(RotPoint.Source, RotMovFactor.All, M2.dtheta);

                    //後段ミラーを前段座標系に変換（x+downM.f+2*upM.f）
                    M2.movCoord(RotMovFactor.All, M2.ell_f + 2.0 * M1.ell_f);


                }

                static double na(double pos, double ea, double eb, double a2, double b2, double x0, double y0)
                {
                    double _f = pos * eb * x0 / (ea * Math.Sqrt(ea * ea - x0 * x0));
                    double f = pos * eb * Math.Sqrt(ea * ea - x0 * x0) / ea;

                    double x = (x0 / _f + f - b2) / (a2 + 1 / _f);
                    double y = a2 * x + b2;

                    return Math.Sqrt(Math.Pow(x0 - x, 2.0) + Math.Pow(y0 - y, 2.0));
                }

                #region 回転
                public enum RotPoint
                {
                    Assign,
                    Source,
                    Focus,
                    Mirror
                }
                public enum RotMovFactor
                {
                    All,
                    Source,
                    Focus,
                    Mirror
                }

                /// <summary>
                /// 座標回転
                /// </summary>
                /// <param name="rotPoint">回転中心</param>
                /// <param name="rotFactor">回転する座標</param>
                /// <param name="theta">回転角度[rad]</param>
                /// <param name="x0">回転中心x座標</param>
                /// <param name="y0">回転中心y座標</param>
                private void RotCoord(RotPoint rotPoint, RotMovFactor rotFactor, double theta, double x0 = 0.0, double y0 = 0.0)
                {
                    //rotPoint==Assignのときは引数で指定された座標
                    if (rotPoint == RotPoint.Source)
                    {
                        x0 = s.xc;
                        y0 = s.yc;
                    }
                    else if (rotPoint == RotPoint.Mirror)
                    {
                        x0 = m.xc;
                        y0 = m.yc;
                    }
                    else if (rotPoint == RotPoint.Focus)
                    {
                        x0 = f.xc;
                        y0 = f.yc;
                    }

                    //光源座標の回転
                    if (rotFactor == RotMovFactor.Source)
                    {
                        s.Rot(theta, x0, y0);
                    }
                    //ミラー座標の回転
                    else if (rotFactor == RotMovFactor.Mirror)
                    {
                        m.Rot(theta, x0, y0);
                        mw.Rot(theta, x0, y0);
                    }
                    //焦点座標の回転
                    else if (rotFactor == RotMovFactor.Focus)
                    {
                        f.Rot(theta, x0, y0);
                    }
                    //全座標の回転
                    else
                    {
                        //光源座標の回転
                        s.Rot(theta, x0, y0);

                        //ミラー座標の回転
                        m.Rot(theta, x0, y0);
                        mw.Rot(theta, x0, y0);

                        //焦点座標の回転
                        f.Rot(theta, x0, y0);

                        //各角度の回転
                        //this.theta_s += theta;
                        //this.theta_i += theta;
                        //this.theta_f += theta;
                    }

                }

                /// <summary>
                /// 座標の回転1
                /// </summary>
                /// <param name="x">回転したい座標x</param>
                /// <param name="y">回転したい座標y</param>
                /// <param name="theta">回転角度</param>
                /// <param name="x0">回転中心x座標</param>
                /// <param name="y0">回転中心y座標</param>
                private static void rotXY(ref double x, ref double y, double theta, double x0 = 0.0, double y0 = 0.0)
                {
                    double x_buf = x - x0;
                    double y_buf = y - y0;
                    x = x_buf * Math.Cos(theta) - y_buf * Math.Sin(theta) + x0;
                    y = x_buf * Math.Sin(theta) + y_buf * Math.Cos(theta) + y0;
                }

                /// <summary>
                /// 座標の回転2
                /// </summary>
                /// <param name="x">回転したい座標x[]</param>
                /// <param name="y">回転したい座標y[]</param>
                /// <param name="theta">回転角度</param>
                /// <param name="x0">回転中心x座標</param>
                /// <param name="y0">回転中心y座標</param>
                private static void rotXY(ref double[] x, ref double[] y, double theta, double x0 = 0.0, double y0 = 0.0)
                {
                    if (x != null && y != null)
                        for (int i = 0; i < x.Length; i++)
                            rotXY(ref x[i], ref y[i], theta, x0, y0);
                }

                /// <summary>
                /// 座標の回転3
                /// </summary>
                /// <param name="x">回転したい座標x[][]</param>
                /// <param name="y">回転したい座標y[][]</param>
                /// <param name="theta">回転角度</param>
                /// <param name="x0">回転中心x座標</param>
                /// <param name="y0">回転中心y座標</param>
                public static void rotXY(ref double[][] x, ref double[][] y, double theta, double x0 = 0.0, double y0 = 0.0)
                {
                    if (x != null && y != null)
                        for (int i = 0; i < x.GetLength(0); i++)
                            rotXY(ref x[i], ref y[i], theta, x0, y0);
                }
                #endregion

                #region 移動

                /// <summary>
                /// 座標移動
                /// </summary>
                /// <param name="movFactor">移動する座標</param>
                /// <param name="dx">x移動量</param>
                /// <param name="dy">y移動量</param>
                private void movCoord(RotMovFactor movFactor, double dx = 0.0, double dy = 0.0)
                {
                    //光源座標の移動
                    if (movFactor == RotMovFactor.Source)
                    {
                        s.Move(dx, dy);
                    }
                    //ミラー座標の移動
                    else if (movFactor == RotMovFactor.Mirror)
                    {
                        m.Move(dx, dy);
                        mw.Move(dx, dy);
                    }
                    //焦点座標の移動
                    else if (movFactor == RotMovFactor.Focus)
                    {
                        f.Move(dx, dy);
                    }
                    //全座標の移動
                    else
                    {
                        //光源座標の移動
                        s.Move(dx, dy);

                        //ミラー座標の移動
                        m.Move(dx, dy);
                        mw.Move(dx, dy);

                        //焦点座標の移動
                        f.Move(dx, dy);
                    }

                }

                /// <summary>
                /// 座標の移動1
                /// </summary>
                /// <param name="x">移動したい座標x</param>
                /// <param name="y">移動したい座標y</param>
                /// <param name="dx">x移動量</param>
                /// <param name="dy">y移動量</param>
                private static void movXY(ref double x, ref double y, double dx, double dy)
                {
                    x += dx;
                    y += dy;
                }

                /// <summary>
                /// 座標の移動2
                /// </summary>
                /// <param name="x">移動したい座標x[]</param>
                /// <param name="y">移動したい座標y[]</param>
                /// <param name="dx">x移動量</param>
                /// <param name="dy">y移動量</param>
                private static void movXY(ref double[] x, ref double[] y, double dx, double dy)
                {
                    if (x != null && y != null)
                        for (int i = 0; i < x.Length; i++)
                            movXY(ref x[i], ref y[i], dx, dy);
                }

                /// <summary>
                /// 座標の移動3
                /// </summary>
                /// <param name="x">移動したい座標x[][]</param>
                /// <param name="y">移動したい座標y[][]</param>
                /// <param name="dx">x移動量</param>
                /// <param name="dy">y移動量</param>
                private static void movXY(ref double[][] x, ref double[][] y, double dx, double dy)
                {
                    if (x != null && y != null)
                        for (int i = 0; i < x.GetLength(0); i++)
                            movXY(ref x[i], ref y[i], dx, dy);
                }

                #endregion

                #endregion
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

            class WaveField1D
            {

                #region 宣言
                const double h = 6.62607e-34;
                const double e = 1.602e-19;
                const double c = 2.99792458e8;
                public double lambda { get; set; }
                public double Energy
                {
                    get { return h * c / (e * lambda); }
                    set { lambda = h * c / (e * value); }
                }

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

                Coord s;
                CoordM m;
                CoordF f;

                IProgress<ProgressInfo> progress;
                public CancellationToken ct { get; set; }
                #endregion

                #region constructor
                public WaveField1D() { }

                public WaveField1D(Coord _Source, CoordM _Mirror, CoordF _Focus)
                {
                    Initialize(_Source, _Mirror, _Focus);
                }

                #endregion

                #region void

                public void Initialize(Coord _Source, CoordM _Mirror, CoordF _Focus)
                {
                    s = _Source;
                    m = _Mirror;
                    f = _Focus;
                }


                public void Execute(int _core = 1, IProgress<ProgressInfo> _progress = null)
                {
                    //
                    if (s == null && m == null && f == null)
                    {
                        return;
                    }

                    doneNum = 0;

                    core = _core;

                    progress = _progress;

                    //Source->Mirror
                    message = "Source->Mirror";
                    ForwardPropagation(s, ref m, core);
                    //Mirror->Focus
                    message = "Mirror->Focus";
                    ForwardPropagation(m, ref f, core);

                    f.Finish();

                }

                void ForwardPropagation(Coord c1, ref Coord c2, int _core)
                {
                    double k;
                    double[] ds = new double[c1.div];
                    Complex ii = new Complex(0.0, 1.0);     //虚数単位

                    //波数の計算
                    k = 2.0 * Math.PI / lambda;


                    //伝播元の微小長さの計算
                    if (c1.div != 1)
                    {
                        for (int i = 1; i < c1.div; i++)
                        {
                            ds[i] = Math.Sqrt(Math.Pow(c1.x[i] - c1.x[i - 1], 2.0) + Math.Pow(c1.y[i] - c1.y[i - 1], 2.0));
                        }
                        ds[0] = ds[1];
                    }
                    else
                    {
                        ds[0] = 1;
                    }

                    int _div = c2.div;
                    double[] _x = (double[])c2.x.Clone();
                    double[] _y = (double[])c2.y.Clone();
                    Complex[] _u = new Complex[c2.div];

                    //使用コア数の設定
                    ParallelOptions p = new ParallelOptions();
                    p.MaxDegreeOfParallelism = _core;

                    //伝播計算
                    Parallel.For(0, _div, p, i =>
                    {
                        //_u[i] = new Complex(0.0, 0.0);
                        for (int j = 0; j < c1.div; j++)
                        {
                            _u[i] += c1.u[j] * Complex.Exp(-ii * k * Math.Sqrt(Math.Pow(_x[i] - c1.x[j], 2.0) + Math.Pow(_y[i] - c1.y[j], 2.0)))
                                             / Math.Sqrt(Math.Pow(_x[i] - c1.x[j], 2.0) + Math.Pow(_y[i] - c1.y[j], 2.0)) * ds[j];
                        }

                        Interlocked.Increment(ref this.doneNum);
                        report(100 * (this.doneNum) / this.totalNum, message);

                        //cancel
                        this.ct.ThrowIfCancellationRequested();

                    });

                    //uコピーと強度位相計算
                    c2.u = _u;
                }

                void ForwardPropagation(Coord c1, ref CoordM c2, int _core)
                {
                    this.totalNum = c2.div;
                    this.doneNum = 0;
                    report(0, message);

                    Coord _c2 = c2.ToCoord();
                    ForwardPropagation(c1, ref _c2, _core);
                    c2.CoordTo(_c2);
                }

                void ForwardPropagation(Coord c1, ref CoordF c2, int _core)
                {
                    this.totalNum = c2.n * c2.div;
                    this.doneNum = 0;
                    report(0, message);

                    for (int i = 0; i < c2.n; i++)
                    {
                        Coord _c2 = c2.ToCoord(i);
                        ForwardPropagation(c1, ref _c2, _core);
                        c2.CoordTo(i, _c2);
                    }
                }

                void report(int _value, string _message)
                {
                    if (progress != null)
                        progress.Report(new ProgressInfo(_value, _message + string.Format("計算中 {0}/{1}", _value, 100)));
                }

                #endregion

            }


            class WaveField
            {
                public Complex[] u;     //波動場
                public double[] x;      //波動場X座標
                public double[] y;      //波動場Y座標
                public double[] Intensity;
                public double[] Phase;
                public double[] PosX;
                public double lambda;

                //コンストラクタ
                public WaveField(int num, double _lambda)
                {
                    //各値の初期化
                    u = new Complex[num];
                    x = new double[num];
                    y = new double[num];
                    PosX = new double[num];
                    lambda = _lambda;

                    for (int i = 0; i < num; i++)
                    {
                        u[i] = new Complex(0.0, 0.0);
                        x[i] = 0.0;
                        y[i] = 0.0;
                    }
                }

                //順方向伝播(引数：伝播元波動場)
                public void ForwardPropagation(WaveField u_back)
                {
                    double k;
                    double[] ds = new double[u_back.x.Length];
                    Complex ii = new Complex(0.0, 1.0);     //虚数単位

                    //波数の計算
                    k = 2.0 * Math.PI / this.lambda;

                    #region 微小領域をかけない場合の計算
                    //伝播計算
                    //for (int i = 0; i < this.x.Length; i++)
                    //{
                    //    this.u[i] = new Complex(0.0, 0.0);
                    //    for (int j = 0; j < u_back.x.Length; j++)
                    //    {
                    //        this.u[i] += u_back.u[j] * Complex.Exp(-ii * k * Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)))
                    //                        / Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0));
                    //    }
                    //}
                    #endregion

                    #region 微小領域をかけた場合の計算
                    //伝播元の微小長さの計算
                    if (u_back.x.Length != 1)
                    {
                        for (int i = 1; i < u_back.x.Length; i++)
                        {
                            ds[i] = Math.Sqrt(Math.Pow(u_back.x[i] - u_back.x[i - 1], 2.0) + Math.Pow(u_back.y[i] - u_back.y[i - 1], 2.0));
                        }
                        ds[0] = ds[1];
                    }
                    else
                    {
                        ds[0] = 1;
                    }

                    //伝播計算
                    for (int i = 0; i < this.x.Length; i++)
                    {
                        this.u[i] = new Complex(0.0, 0.0);
                        for (int j = 0; j < u_back.x.Length; j++)
                        {
                            this.u[i] += u_back.u[j] * Complex.Exp(-ii * k * Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)))
                                            / Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)) * ds[j];
                        }
                        //for (int j = 0; j < u_back.x.Length; j++)
                        //{
                        //    this.u[i] += u_back.u[j] * Complex.Exp(-ii * k * Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)))
                        //                    / Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0));
                        //}
                    }
                    #endregion

                    //強度計算
                    this.Intensity = new double[this.u.Length];
                    this.Phase = new double[this.u.Length];
                    for (int i = 0; i < this.u.Length; i++)
                    {
                        this.Intensity[i] = Math.Pow(this.u[i].Magnitude, 2.0);
                        this.Phase[i] = this.u[i].Phase;
                    }
                }

                //逆方向伝播（引数；伝播元波動場）
                public void BackwardPropagation(WaveField u_back)
                {
                    double k;
                    double[] ds = new double[u_back.x.Length];
                    Complex ii = new Complex(0.0, 1.0);     //虚数単位

                    //波数の計算
                    k = 2.0 * Math.PI / this.lambda;

                    //伝播元の微小長さの計算
                    if (u_back.x.Length != 1)
                    {
                        for (int i = 1; i < u_back.x.Length; i++)
                        {
                            ds[i] = Math.Sqrt(Math.Pow(u_back.x[i] - u_back.x[i - 1], 2.0) + Math.Pow(u_back.y[i] - u_back.y[i - 1], 2.0));
                        }
                        ds[0] = ds[1];
                    }
                    else
                    {
                        ds[0] = 1;
                    }

                    //伝播計算
                    for (int i = 0; i < this.x.Length; i++)
                    {
                        this.u[i] = new Complex(0.0, 0.0);
                        for (int j = 0; j < u_back.x.Length; j++)
                        {
                            this.u[i] += u_back.u[j] * Complex.Exp(ii * k * Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)))
                                            / Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)) * ds[j];
                        }
                    }
                }

                //FWHM計算
                public static double FWHM(WaveField waveField)
                {
                    double max = 0.0;
                    double iMax = 0;
                    for (int i = 0; i < waveField.PosX.Length; i++)
                        if (waveField.Intensity[i] > max)
                        {
                            max = waveField.Intensity[i];
                            iMax = i;
                        }

                    double dblePos1 = 0.0;
                    double dblePos2 = 0.0;
                    for (int i = 1; i < iMax; i++)
                        if (max / 2.0 < waveField.Intensity[i])
                        {
                            dblePos1 = (waveField.PosX[i] - waveField.PosX[i - 1]) / (waveField.Intensity[i] - waveField.Intensity[i - 1]) * (max / 2.0 - waveField.Intensity[i]) + waveField.PosX[i];
                            break;
                        }
                    for (int i = waveField.PosX.Length - 2; i > iMax; i--)
                        if (max / 2.0 < waveField.Intensity[i])
                        {
                            dblePos2 = (waveField.PosX[i] - waveField.PosX[i + 1]) / (waveField.Intensity[i] - waveField.Intensity[i + 1]) * (max / 2.0 - waveField.Intensity[i]) + waveField.PosX[i];
                            break;
                        }

                    return dblePos2 - dblePos1;
                }

                #region 並列計算ver
                /// <summary>
                /// 
                /// </summary>
                /// <param name="u_back"></param>
                /// <param name="core"></param>
                public void ForwardPropagation(WaveField u_back, int core)
                {
                    double k;
                    double[] ds = new double[u_back.x.Length];
                    Complex ii = new Complex(0.0, 1.0);     //虚数単位

                    //波数の計算
                    k = 2.0 * Math.PI / this.lambda;

                    #region 微小領域をかけない場合の計算
                    //伝播計算
                    //for (int i = 0; i < this.x.Length; i++)
                    //{
                    //    this.u[i] = new Complex(0.0, 0.0);
                    //    for (int j = 0; j < u_back.x.Length; j++)
                    //    {
                    //        this.u[i] += u_back.u[j] * Complex.Exp(-ii * k * Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)))
                    //                        / Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0));
                    //    }
                    //}
                    #endregion

                    #region 微小領域をかけた場合の計算
                    //伝播元の微小長さの計算
                    if (u_back.x.Length != 1)
                    {
                        for (int i = 1; i < u_back.x.Length; i++)
                        {
                            ds[i] = Math.Sqrt(Math.Pow(u_back.x[i] - u_back.x[i - 1], 2.0) + Math.Pow(u_back.y[i] - u_back.y[i - 1], 2.0));
                        }
                        ds[0] = ds[1];
                    }
                    else
                    {
                        ds[0] = 1;
                    }

                    //伝播計算
                    Parallel.For(0, this.x.Length, i =>
                    {
                        this.u[i] = new Complex(0.0, 0.0);
                        for (int j = 0; j < u_back.x.Length; j++)
                        {
                            this.u[i] += u_back.u[j] * Complex.Exp(-ii * k * Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)))
                                            / Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)) * ds[j];
                        }
                    });
                    #endregion

                    //強度計算
                    this.Intensity = new double[this.u.Length];
                    this.Phase = new double[this.u.Length];
                    for (int i = 0; i < this.u.Length; i++)
                    {
                        this.Intensity[i] = Math.Pow(this.u[i].Magnitude, 2.0);
                        this.Phase[i] = this.u[i].Phase;
                    }

                }


                #endregion
            }

        }

    }

    //
    //
    //
    //





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
                    x[i] = x_buf * Math.Cos(rotTheta) - y_buf * Math.Sin(rotTheta) + x0;
                    y[i] = x_buf * Math.Sin(rotTheta) + y_buf * Math.Cos(rotTheta) + y0;
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
                public double FL { get; set; }
                public int divFL { get; set; } = 0;

                public double FW { get; set; }
                public int divFW { get; set; } = 0;
                #endregion

                public double theta_all { get; set; }
            }

            public Parameter pm;
            public Coord m;
            double m_xc, m_yc;
            public Coord s;
            double s_xc, s_yc;
            public Coord[] f;
            double f_xc, f_yc;

            public Mirror1D(Parameter _pm)
            {
                Init(_pm);
            }

            public void Init(Parameter _pm)
            {
                pm = _pm;

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
                if (pm.divFL != 0 && pm.divFW != 0)
                {
                    f = new Coord[pm.divFL];
                    for (int i = 0; i < pm.divFL; i++)
                        f[i] = new Coord(pm.divFW);
                }
            }

            /// <summary>
            /// 平面ミラー
            /// </summary>
            void Plane()
            {
                
                pm.theta_s = (double)pm.pos * Math.Abs(Math.Atan(pm.L2 * Math.Sin(2.0 * pm.theta_i) / (pm.L1 + pm.L2 * Math.Cos(2.0 * pm.theta_i))));
                pm.theta_f = Math.Abs((double)pm.pos * pm.theta_s - 2.0 * pm.theta_i);


                //光源中心座標
                s_xc = 0.0;
                s_yc = 0.0;
                //焦点中心座標
                f_xc = pm.L1 * Math.Cos(pm.theta_s) + pm.L2 * Math.Cos(pm.theta_f);
                f_yc = 0.0;
                //ミラー中心座標
                m_xc = pm.L1 * Math.Cos(pm.theta_s);
                m_yc = pm.L1 * Math.Sin(pm.theta_s);

                //ミラーの一次方程式
                pm.a = (double)pm.pos * Math.Tan((pm.theta_f + pm.theta_s) / 2.0); //(f-s)/2+s
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
                pm.theta_s = (double)pm.pos * Math.Abs(Math.Atan(pm.L2 * Math.Sin(2.0 * pm.theta_i) / (pm.L1 + pm.L2 * Math.Cos(2.0 * pm.theta_i))));
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
                        y = (double)pm.pos * pm.a * x + pm.b;
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
                        double a2x2 = Math.Pow(pm.a, 2.0) - Math.Pow(x, 2.0);
                        c = pm.a / (pm.b * (-x * x / Math.Pow(a2x2, 3.0 / 2.0) - 1.0 / Math.Sqrt(a2x2)));
                        break;
                    case Parameter.MirrorType.Ellipse:
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


            public void rotXY(double _theta_all, double x0, double y0)
            {
                //ミラーの回転
                m.Rot(_theta_all, x0, y0);
                //光源の回転
                s.Rot(_theta_all, x0, y0);
                //焦点の回転
                foreach (var _f in f) _f.Rot(_theta_all, x0, y0);
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

            public Complex[] u;     //波動場
            public double[] x;      //波動場X座標
            public double[] y;      //波動場Y座標
            public double[] Intensity;
            public double[] Phase;
            public double[] PosX;


            public enum PropDir { Forward = -1, Backward = 1 }

            //コンストラクタ
            public WaveField(int num, double _lambda)
            {
                //各値の初期化
                u = new Complex[num];
                x = new double[num];
                y = new double[num];
                PosX = new double[num];
                lambda = _lambda;

                for (int i = 0; i < num; i++)
                {
                    u[i] = new Complex(0.0, 0.0);
                    x[i] = 0.0;
                    y[i] = 0.0;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="wf0">伝播元</param>
            /// <param name="dir">伝播方向</param>
            public void Propagation(WaveField wf0, PropDir dir = PropDir.Forward)
            {
                double[] ds = new double[wf0.x.Length];


                #region 微小領域をかけない場合の計算
                //伝播計算
                //for (int i = 0; i < this.x.Length; i++)
                //{
                //    this.u[i] = new Complex(0.0, 0.0);
                //    for (int j = 0; j < u_back.x.Length; j++)
                //    {
                //        this.u[i] += u_back.u[j] * Complex.Exp(-ii * k * Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)))
                //                        / Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0));
                //    }
                //}
                #endregion

                #region 微小領域をかけた場合の計算
                //伝播元の微小長さの計算
                if (wf0.x.Length != 1)
                {
                    for (int i = 1; i < wf0.x.Length; i++)
                    {
                        ds[i] = Math.Sqrt(Math.Pow(wf0.x[i] - wf0.x[i - 1], 2.0) + Math.Pow(wf0.y[i] - wf0.y[i - 1], 2.0));
                    }
                    ds[0] = ds[1];
                }
                else
                {
                    ds[0] = 1;
                }

                //伝播計算
                for (int i = 0; i < this.x.Length; i++)
                {
                    this.u[i] = new Complex(0.0, 0.0);
                    for (int j = 0; j < wf0.x.Length; j++)
                    {
                        this.u[i] += wf0.u[j] * Complex.Exp((double)dir * Complex.ImaginaryOne * k * Math.Sqrt(Math.Pow(this.x[i] - wf0.x[j], 2.0) + Math.Pow(this.y[i] - wf0.y[j], 2.0)))
                                        / Math.Sqrt(Math.Pow(this.x[i] - wf0.x[j], 2.0) + Math.Pow(this.y[i] - wf0.y[j], 2.0)) * ds[j];
                    }
                    //for (int j = 0; j < u_back.x.Length; j++)
                    //{
                    //    this.u[i] += u_back.u[j] * Complex.Exp(-ii * k * Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)))
                    //                    / Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0));
                    //}
                }
                #endregion

                //強度計算
                this.Intensity = new double[this.u.Length];
                this.Phase = new double[this.u.Length];
                for (int i = 0; i < this.u.Length; i++)
                {
                    this.Intensity[i] = Math.Pow(this.u[i].Magnitude, 2.0);
                    this.Phase[i] = this.u[i].Phase;
                }
            }

            //FWHM計算
            public static double FWHM(WaveField waveField)
            {
                double max = 0.0;
                double iMax = 0;
                for (int i = 0; i < waveField.PosX.Length; i++)
                    if (waveField.Intensity[i] > max)
                    {
                        max = waveField.Intensity[i];
                        iMax = i;
                    }

                double dblePos1 = 0.0;
                double dblePos2 = 0.0;
                for (int i = 1; i < iMax; i++)
                    if (max / 2.0 < waveField.Intensity[i])
                    {
                        dblePos1 = (waveField.PosX[i] - waveField.PosX[i - 1]) / (waveField.Intensity[i] - waveField.Intensity[i - 1]) * (max / 2.0 - waveField.Intensity[i]) + waveField.PosX[i];
                        break;
                    }
                for (int i = waveField.PosX.Length - 2; i > iMax; i--)
                    if (max / 2.0 < waveField.Intensity[i])
                    {
                        dblePos2 = (waveField.PosX[i] - waveField.PosX[i + 1]) / (waveField.Intensity[i] - waveField.Intensity[i + 1]) * (max / 2.0 - waveField.Intensity[i]) + waveField.PosX[i];
                        break;
                    }

                return dblePos2 - dblePos1;
            }

            #region 並列計算ver
            /// <summary>
            /// 
            /// </summary>
            /// <param name="u_back"></param>
            /// <param name="core"></param>
            public void ForwardPropagation(WaveField u_back, int core)
            {
                double k;
                double[] ds = new double[u_back.x.Length];

                //波数の計算
                k = 2.0 * Math.PI / this.lambda;

                #region 微小領域をかけない場合の計算
                //伝播計算
                //for (int i = 0; i < this.x.Length; i++)
                //{
                //    this.u[i] = new Complex(0.0, 0.0);
                //    for (int j = 0; j < u_back.x.Length; j++)
                //    {
                //        this.u[i] += u_back.u[j] * Complex.Exp(-ii * k * Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)))
                //                        / Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0));
                //    }
                //}
                #endregion

                #region 微小領域をかけた場合の計算
                //伝播元の微小長さの計算
                if (u_back.x.Length != 1)
                {
                    for (int i = 1; i < u_back.x.Length; i++)
                    {
                        ds[i] = Math.Sqrt(Math.Pow(u_back.x[i] - u_back.x[i - 1], 2.0) + Math.Pow(u_back.y[i] - u_back.y[i - 1], 2.0));
                    }
                    ds[0] = ds[1];
                }
                else
                {
                    ds[0] = 1;
                }

                //伝播計算
                Parallel.For(0, this.x.Length, i =>
                {
                    this.u[i] = new Complex(0.0, 0.0);
                    for (int j = 0; j < u_back.x.Length; j++)
                    {
                        this.u[i] += u_back.u[j] * Complex.Exp(-Complex.ImaginaryOne * k * Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)))
                                        / Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)) * ds[j];
                    }
                });
                #endregion

                //強度計算
                this.Intensity = new double[this.u.Length];
                this.Phase = new double[this.u.Length];
                for (int i = 0; i < this.u.Length; i++)
                {
                    this.Intensity[i] = Math.Pow(this.u[i].Magnitude, 2.0);
                    this.Phase[i] = this.u[i].Phase;
                }

            }


            #endregion
        }

    }


}
