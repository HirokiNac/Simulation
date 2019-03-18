

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
    namespace Mirror2D
    {

        public class Coord
        {
            #region 変数

            public int divW { get; private set; }
            public int divL { get; private set; }

            public double xc, yc, zc;

            public double[,] x { get; set; }
            public double[,] y { get; set; }
            public double[,] z { get; set; }

            public double[] rx { get; set; }
            public double[] ry { get; set; }

            //public Complex[,] u { get; set; }
            public double[,] real { get; set; }
            public double[,] imag { get; set; }
            public double[,] Intensity { get; set; }
            public double[,] Phase { get; set; }

            #endregion

            public Coord(int _divW,int _divL)
            {
                Initialize(_divW, _divL);
            }

            public void Initialize(int _divW,int _divL)
            {
                divW = _divW;
                divL = _divL;

                x = new double[divW, divL];
                y = new double[divW, divL];
                z = new double[divW, divL];

                rx = new double[divL];
                ry = new double[divL];

                //u = new Complex[divW, divL];
                real = new double[divW, divL];
                imag = new double[divW, divL];
                Intensity = new double[divW, divL];
                Phase = new double[divW, divL];

            }

            #region 座標補正
            /// <summary>
            /// xy座標回転
            /// </summary>
            /// <param name="rotTheta">回転角度</param>
            /// <param name="x0">x回転中心</param>
            /// <param name="y0">y回転中心</param>
            public static void Rot(ref double[] x,ref double[] y,double rotTheta, double x0, double y0)
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
            public static void Move(ref double[] x,ref double[] y,double dx, double dy)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    x[i] += dx;
                    y[i] += dy;
                }
            }
            #endregion

        }

        public class CoordM:Coord

        {
            #region 変数

            public double[,] z_mod
            {
                get
                {
                    double[,] _zmod = new double[divW, divL];

                    double[] _x = new double[divL];
                    double[] _z = new double[divL];
                    for (int j = 0; j < divL; j++)
                    {
                        _x[j] = x[divW / 2, j];
                        _z[j] = z[divW / 2, j];
                    }

                    var p = Fit.Line(_x, _z);
                    double min = 1e10;
                    for (int i = 0; i < divW; i++)
                    {
                        for (int j = 0; j < divL; j++)
                        {
                            _zmod[i, j] = z[i, j] - (p.Item2 * _x[j] + p.Item1);
                            if (_zmod[i, j] < min) min = _zmod[i, j];
                        }
                    }

                    for (int i = 0; i < divW; i++)
                    {
                        for (int j = 0; j < divL; j++)
                            _zmod[i, j] -= min;
                    }
                    return _zmod;
                }
            }

            //座標拡張1
            public int divW2;
            public int divL2;
            /// <summary>[長手][短手]</summary>
            public double[][] x2;
            /// <summary>[長手][短手]</summary>
            public double[][] y2;
            /// <summary>[長手][短手]</summary>
            public double[][] z2;


            //座標拡張2
            public int divW3;
            public int divL3;
            public double[,] x3;
            public double[,] y3;
            public double[,] z3;
            public double[,] z_torus;
            public double[,] z_torus_sub;
            public bool[,] reflect;
            public Complex[,] u3;

            public double[,] z3_mod
            {
                get
                {
                    int i1 = z3.GetLength(0);
                    int i2 = z3.GetLength(1);

                    double[,] _zmod = new double[i1, i2];

                    double[] _x = new double[i2];
                    double[] _z = new double[i2];
                    for (int j = 0; j < i2; j++)
                    {
                        _x[j] = x3[i1 / 2, j];
                        _z[j] = z3[i1 / 2, j];
                    }

                    var p = Fit.Line(_x, _z);
                    double min = 1e10;
                    for (int i = 0; i <i1; i++)
                    {
                        for (int j = 0; j < i2; j++)
                        {
                            _zmod[i, j] = z3[i, j] - (p.Item2 * _x[j] + p.Item1);
                            if (_zmod[i, j] < min) min = _zmod[i, j];
                        }
                    }

                    for (int i = 0; i < i1; i++)
                    {
                        for (int j = 0; j < i2; j++)
                            if (reflect[i, j])
                                _zmod[i, j] -= min;
                            else
                                _zmod[i, j] = 0.0;
                    }
                    return _zmod;
                }
            }


            #endregion

            public void Initialize3(int _divW3, int _divL3)
            {
                divW3 = _divW3;
                divL3 = _divL3;
                x3 = new double[divW3, divL3];
                y3 = new double[divW3, divL3];
                z3 = new double[divW3, divL3];
                reflect = new bool[divW3, divL3];
                u3 = new Complex[divW3, divL3];
            }

            public CoordM(int _div1, int _div2)
                : base(_div1, _div2)
            { }
        }

        public class CoordF
        {
            public double xc { get; set; }
            public double yc { get; set; }
            public double zc { get; set; }

            public int nx { get; private set; }
            public int ny { get; private set; }
            public int nz { get; private set; }
            
            public double dx { get; private set; }
            public double dy { get; private set; }
            public double dz { get; private set; }

            public double[][,] x { get; set; }
            public double[][,] y { get; set; }
            public double[][,] z { get; set; }

            public Complex[][,] u { get; set; }
            public double[][,] Intensity { get; set; }

            public CoordF(int _nx, double _dx, int _ny, double _dy, int _nz, double _dz)
            { Initialize(_nx, _dx, _ny, _dy, _nz, _dz); }

            public CoordF()
                : this(1, 0, 1, 0, 1, 0) { }

            public void Initialize(int _nx,double _dx,int _ny,double _dy,int _nz,double _dz)
            {
                nx = _nx;
                ny = _ny;
                nz = _nz;

                dx = _dx;
                dy = _dy;
                dz = _dz;

                x = new double[nx][,];
                y = new double[nx][,];
                z = new double[nx][,];
                u = new Complex[nx][,];
                Intensity = new double[nx][,];

                for(int i=0;i<nx;i++)
                {
                    x[i] = new double[ny, nz];
                    y[i] = new double[ny, nz];
                    z[i] = new double[ny, nz];
                    u[i] = new Complex[ny, nz];
                    Intensity[i] = new double[ny, nz];
                }
            }
        }

        public class Mirror2D
        {
            #region 変数

            const double mpos = 1.0;

            public double ell_a { get; private set; }
            public double ell_b { get; private set; }
            public double ell_f { get; private set; }
            
            double L1 { get; set; }
            double L2 { get; set; }
            double theta_i, theta_s, theta_f;
            public double MW { get; private set; }
            public double ML { get; private set; }
            public int divW { get; private set; }
            public int divL { get; private set; }

            public Coord s;
            public CoordM m;
            public CoordM m2;
            public CoordF f;
            #endregion

            internal Mirror2D(double _L1, double _L2, 
                double _theta_i, int _divW, int _divL, 
                double _MW, double _ML)
            {
                L1 = _L1;
                L2 = _L2;

                theta_i = _theta_i;
                divL = _divL;
                divW = _divW;

                MW = _MW;
                ML = _ML;

                m = new CoordM(divW, divL);
                Ellipsoid();
            }


            void Ellipsoid()
            {
                ell_a = (L1 + L2) / 2.0;
                theta_s = mpos * Math.Abs(Math.Atan(L2 * Math.Sin(2.0 * theta_i) / (L1 + L2 * Math.Cos(2.0 * theta_i))));
                theta_f = Math.Abs(mpos * theta_s - 2.0 * theta_i);

                ell_f = (L1 * Math.Cos(theta_s) + L2 * Math.Cos(theta_f)) / 2.0;
                ell_b = Math.Sqrt(ell_a * ell_a - ell_f * ell_f);

                #region x,y,z中心座標決定(座標系基準)
                
                m.xc = L1 * Math.Cos(theta_s) - ell_f;
                m.yc = 0.0;
                m.zc = ell_z(m.xc, m.yc);

                #endregion

                //x,y座標増分
                double dx = ML / (divL - 1.0);
                double dy = MW / (divW - 1.0);

                #region 拡張なしver.
                Parallel.For(0, divL, j =>
                //for (int j = 0; j < divL; j++)
                {
                    for (int i = 0; i < divW; i++)
                    {
                        m.x[i, j] = m.xc - ML / 2.0 + dx * j;
                        m.y[i, j] = m.yc - MW / 2.0 + dy * i;
                        m.z[i, j] = -ell_b * Math.Sqrt(1 - (Math.Pow(m.x[i, j] / ell_a, 2.0) + Math.Pow(m.y[i, j] / ell_b, 2.0)));
                    }
                    m.rx[j] = curv(m.x[0, j]);
                    m.ry[j] = m.z[0, j];
                }
                );
                #endregion

                #if kakuchou
                #region 拡張ありver.
                #region 平面
                //上流端・下流端の先端座標から平面の式を求める
                double x1 = m.xc - ML / 2.0;
                double x2 = m.xc - ML / 2.0;
                double x3 = m.xc + ML / 2.0;
                double y1 = m.yc - MW / 2.0;
                double y2 = m.yc + MW / 2.0;
                double y3 = m.yc + MW / 2.0;

                double z1 = ell_z(x1, y1);
                double z2 = ell_z(x2, y2);
                double z3 = ell_z(x3, y3);

                double pa = (y2 - y1) * (z3 - z1) - (z2 - z1) * (y3 - y1);
                double pb = (z2 - z1) * (x3 - x1) - (x2 - x1) * (z3 - z1);
                double pc = (x2 - x1) * (y3 - y1) - (y2 - y1) * (x3 - x1);

                #endregion

                #region 回転楕円と平面と接するところを探す

                List<List<double>> listX = new List<List<double>>();
                List<List<double>> listY = new List<List<double>>();
                List<List<double>> listZ = new List<List<double>>();

                int iMax = 0;

                for (int j = 0; j < divL; j++)
                {
                    List<double> lX = new List<double>();
                    List<double> lY = new List<double>();
                    List<double> lZ = new List<double>();

                    int i = 0;

                    double x = m.xc - ML / 2.0 + dx * j;

                    while(true)
                    {

                        double y = m.yc + dy * i;
                        double z = ell_z(x, y);
                        double pz = (-pa * x - pb * y + (pa * x1 + pb * y1 + pc * z1)) / pc;
                        
                        lX.Add(x);
                        lY.Add(y);
                        lZ.Add(z);
                        i++;

                        if (pz < z)
                            break;
                    }

                    i = 1;

                    do
                    {
                        double y = m.yc - dy * i;
                        double z = ell_z(x, y);
                        double pz = (-pa * x - pb * y + (pa * x1 + pb * y1 + pc * z1)) / pc;

                        lX.Insert(0, x);
                        lY.Insert(0, y);
                        lZ.Insert(0, z);
                        i++;

                        if (pz < z)
                            break;

                    } while (true);

                    iMax = iMax < i ? i : iMax;
                    listX.Add(lX);
                    listY.Add(lY);
                    listZ.Add(lZ);
                }

                #endregion

                m.x2 = new double[divL][];
                m.y2 = new double[divL][];
                m.z2 = new double[divL][];

                for (int j = 0; j < divL; j++)
                {
                    m.x2[j] = (double[])listX[j].ToArray();
                    m.y2[j] = (double[])listY[j].ToArray();
                    m.z2[j] = (double[])listZ[j].ToArray();
                }

#region 座標の補正

                m.Initialize3(2 * iMax - 1, divL);

                for (int j = 0; j < divL; j++)
                {
                    int i1 = (2 * iMax - 1 - listX[j].Count) / 2;

                    for (int i = 0; i < i1; i++)
                    {
                        m.x3[i, j] = listX[j][0];
                        m.y3[i, j] = listY[j][0];
                        m.z3[i, j] = listZ[j][0];
                        m.reflect[i, j] = false;
                    }
                    for (int i = i1; i < 2 * iMax - 1 - i1; i++)
                    {
                        m.x3[i, j] = listX[j][i - i1];
                        m.y3[i, j] = listY[j][i - i1];
                        m.z3[i, j] = listZ[j][i - i1];
                        m.reflect[i, j] = true;
                    }
                    for (int i = 2 * iMax - 1 - i1; i < 2 * iMax - 1; i++)
                    {
                        m.x3[i, j] = listX[j][listX[j].Count - 1];
                        m.y3[i, j] = listY[j][listY[j].Count - 1];
                        m.z3[i, j] = listZ[j][listZ[j].Count - 1];
                        m.reflect[i, j] = false;
                    }
                }
#endregion

#endregion

                torus(m.rx[divL / 2] - m.ry[divL / 2], m.ry[divL / 2]);
                #endif

#region 角度基準を計算
                //m2 = m;

                //for(int i=0;i<m2.divW;i++)
                //{
                //    for (int j = 0; j < m2.divL; j++)
                //    {

                //    }
                //}

#endregion
            }

            double ell_z(double x,double y)
            {
                return -ell_b * Math.Sqrt(1 - (Math.Pow(x / ell_a, 2.0) + Math.Pow(y / ell_b, 2.0)));
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

            public void torus(double Ra,double Rb)
            {
                int imax=m.x3.GetLength(0);
                int jmax=m.x3.GetLength(1);
                m.z_torus = new double[imax, jmax];
                m.z_torus_sub = new double[imax, jmax];


                for(int i=0;i<imax;i++)
                {
                    for (int j = 0; j < jmax; j++)
                    {
                        m.z_torus[i, j] = torus(m.x3[i, j] - m.xc, m.y3[i, j] - m.yc, Ra, Rb);
                    }
                }


                //double[] _x = new double[jmax];
                //double[] _z = new double[jmax];
                //for (int j = 0; j < jmax; j++)
                //{
                //    _x[j] = m.x3[imax / 2, j];
                //    _z[j] = m.z_torus[imax / 2, j];
                //}

                //var p = Fit.Line(_x, _z);
                //double min = 1e10;
                //for (int i = 0; i < imax; i++)
                //{
                //    for (int j = 0; j < jmax; j++)
                //    {
                //        m.z_torus[i,j] -= (p.Item2 * _x[j] + p.Item1);
                //        if (m.z_torus[i, j] < min) min = m.z_torus[i, j];
                //    }
                //}

                //
                plane_min(m.x3,ref m.z_torus);
                for (int i = 0; i < imax; i++)
                {
                    for (int j = 0; j < jmax; j++)
                        m.z_torus[i, j] = m.reflect[i, j] ? m.z_torus[i, j] : 0.0;
                }
                //

                double[,] z_mod = m.z3_mod;
                for (int i = 0; i < imax; i++)
                {
                    for (int j = 0; j < jmax; j++)
                    {
                        m.z_torus_sub[i, j] = m.z_torus[i, j] - z_mod[i, j];
                    }
                }

                plane(m.x3,ref m.z_torus_sub);
                for (int i = 0; i < imax; i++)
                {
                    for (int j = 0; j < jmax; j++)
                        m.z_torus_sub[i, j] = m.reflect[i, j] ? m.z_torus_sub[i, j] : 0.0;
                }
            }

            static void plane_min(double[,] x,ref double[,] z)
            {
                int imax = x.GetLength(0);
                int jmax = x.GetLength(1);

                double[] _x = new double[jmax];
                double[] _z = new double[jmax];

                for (int j = 0; j < jmax; j++)
                {
                    _x[j] = x[imax / 2, j];
                    _z[j] = z[imax / 2, j];
                }

                var p = Fit.Line(_x, _z);
                double min = 1e10;
                for (int i = 0; i < imax; i++)
                {
                    for (int j = 0; j < jmax; j++)
                    {
                        z[i, j] -=  (p.Item2 * _x[j] + p.Item1);
                        if (z[i, j] < min) min = z[i, j];
                    }
                }

                for (int i = 0; i < imax; i++)
                {
                    for (int j = 0; j < jmax; j++)
                        z[i, j] -= min;
                }

            }

            static void plane(double[,] x, ref double[,] z)
            {
                int imax = x.GetLength(0);
                int jmax = x.GetLength(1);

                double[] _x = new double[jmax];
                double[] _z = new double[jmax];

                for (int j = 0; j < jmax; j++)
                {
                    _x[j] = x[imax / 2, j];
                    _z[j] = z[imax / 2, j];
                }

                var p = Fit.Line(_x, _z);
                for (int i = 0; i < imax; i++)
                {
                    for (int j = 0; j < jmax; j++)
                    {
                        z[i, j] -= (p.Item2 * _x[j] + p.Item1);
                    }
                }


            }



            double torus(double x, double y, double Ra, double Rb)
            {
                double z = 0.0;
                double p = Math.Asin(y / Rb);
                double t = Math.Acos(x / (Ra + Rb * Math.Cos(p)));
                z = (Ra + Rb * Math.Cos(p)) * Math.Sin(t);
                return z;
            }

#region Source&Detector

            public enum source { point, gauss, rectangle }

            /// <summary>
            /// 光源座標設定
            /// </summary>
            /// <param name="n">分割数</param>
            /// <param name="d">ピッチ</param>
            public void Source(int ny, int nz, double wy, double wz, source _source = source.point)
            {
                s = new Coord(ny, nz);

                s.xc = -ell_f;
                s.yc = 0.0;
                s.zc = 0.0;

                //幅->1pixサイズ
                double dy = _source == source.gauss ? 4.0 * Math.Sqrt(2.0 * Math.Log10(2.0)) * wy / (ny + 1.0) : wy / (ny + 1.0);
                double dz = _source == source.gauss ? 4.0 * Math.Sqrt(2.0 * Math.Log10(2.0)) * wz / (nz + 1.0) : wz / (nz + 1.0);

                double[] x = new double[nz];
                double[] z = new double[nz];
                

                for (int j = 0; j < nz; j++)
                {
                    x[j] = s.xc;
                    z[j] = s.zc + (-nz / 2 + j) * dz;
                }

                //光軸に対して垂直になるように回転
                Coord.Rot(ref x, ref z, -theta_s, s.xc, s.zc);

                double sigy = wy / (2.0 * Math.Sqrt(2.0 * Math.Log(2.0,Math.E)));
                double sigz = wz / (2.0 * Math.Sqrt(2.0 * Math.Log(2.0,Math.E)));

                for (int i = 0; i < ny; i++)
                {
                    for (int j = 0; j < nz; j++)
                    {
                        s.x[i, j] = x[j];
                        s.y[i, j] = s.yc + (-ny / 2 + i) * dy;
                        s.z[i, j] = z[j];

                        if(_source==source.gauss)
                        {
                            s.real[i, j] = gauss(s.y[i, j], s.zc + (-nz / 2 + j) * dz, sigy, sigz);
                            s.imag[i, j] = 0.0;
                        }
                        else
                        {
                            s.real[i, j] = 1.0;
                            s.imag[i, j] = 0.0;
                        }

                        s.Intensity[i, j] = Math.Sqrt(s.real[i, j] * s.real[i, j] + s.imag[i, j] * s.imag[i, j]);
                    }
                }

            }

            static double gauss(double y,double z,double sigmay,double sigmaz)
            {
                return Math.Exp(-y * y / (2.0 * sigmay * sigmay)) / (Math.Sqrt(2.0 * Math.PI) * sigmay)
                    * Math.Exp(-z * z / (2.0 * sigmaz * sigmaz)) / (Math.Sqrt(2.0 * Math.PI) * sigmaz);
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
            public void Focus(int nx, double dx, int ny, double dy, int nz, double dz, double bx = 0.0, double by = 0.0, double bz = 0.0)
            {
                f = new CoordF(nx, dx, ny, dy, nz, dz);

                f.xc = ell_f;
                f.yc = 0.0;
                f.zc = 0.0;

                double[][] x = new double[f.nx][];
                double[][] z = new double[f.nx][];

                //まず回転していない座標で設定
                //表示用ディテクター座標設定 focus_x2
                for (int n = 0; n < nx; n++)
                {
                    x[n] = new double[nz];
                    z[n] = new double[nz];

                    for (int j = 0; j < nz; j++)
                    {
                        x[n][j] = f.xc + (-nx / 2 + n) * dx + bx;
                        z[n][j] = f.zc + (-nz / 2 + j) * dz + bz;
                        //focus_x2[i][j] = Math.Sqrt(Math.Pow(focus_x[i][j] - focus_x[i][0], 2.0) + Math.Pow(focus_y[i][j] - focus_y[i][0], 2.0));
                    }
                    //double theta = (f.zc - m.zc) / (f.xc - m.xc);
                    Coord.Rot(ref x[n], ref z[n], theta_f, f.xc, f.yc);

                    for (int i = 0; i < ny; i++)
                    {
                        for (int j = 0; j < nz; j++)
                        {
                            f.x[n][i, j] = x[n][j];
                            f.y[n][i, j] = f.yc + (-ny / 2 + i) * dy + by;
                            f.z[n][i, j] = z[n][j];
                        }
                    }
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
                const double h = 6.62607e-34;
                const double e = 1.602e-19;
                const double c = 2.99792458e8;
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

                public WaveField2D() { }

                public void Initialize(double[,] _x, double[,] _y, double[,] _z, bool[,] _reflect = null, double[,] _real=null,double[,] _imag=null)
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
                            rev[i + divW* j] = Re[i, j];
                            imv[i + divW * j] = Im[i, j];
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

            public class Mirror1D
            {

#region 宣言

                internal enum mType { Ell, Para }
                internal enum mDiv { Angle, Even }
                internal enum mPos : int { Lower = -1, Upper = 1 }

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


#region 等分割
                    if (mdiv == mDiv.Even)
                    {
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
#endregion

#region 角度分割
                    else
                    {
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

                        const double tol_theta = tol_ML / 1e4;
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

                }

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
                public void Source(int n, double d)
                {
                    double s_xc = s.xc;
                    double s_yc = s.yc;

                    s.Initialize(n);

                    for (int i = 0; i < n; i++)
                    {
                        s.x[i] = s_xc;
                        s.y[i] = s_yc + (-n / 2.0 + i) * d;
                        s.u[i] = new Complex(1.0, 0.0);
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
                    double theta = (f.yc - mw.yc) / (f.xc - mw.xc);
                    fw.Move(bx * Math.Cos(theta) + by * Math.Sin(theta), bx * Math.Sin(theta) - by * Math.Sin(theta));
                    fw.Rot(theta);
                }
#endregion



                public void PlusError(double[] Error)
                {
                    if (Error.Length != div)
                    {
                        //補完する
                        double dx = mw.ML_NA / Error.Length;
                        double[] x = new double[Error.Length];
                        for (int i = 0; i < Error.Length; i++)
                            x[i] = dx * i;
                        double[] xmod = mw.xmod;
                        for (int i = 0; i < div; i++)
                        {
                            double e = MathNet.Numerics.Interpolate.CubicSpline(x, Error).Interpolate(xmod[i]);
                            mw.y[i] += e;
                        }


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


    namespace Graphic
    {
        public class Plot2dPlane
        {
            public enum enumGradation
            {
                mono,
                fullcolor1,
                fullcolor2,
                fullcolor3
            }


            //グラデーションモードの設定
            public enumGradation gradationMode = enumGradation.fullcolor3;

            private int intDataColumn, intDataRow, intBmpColumn, intBmpRow, intPictureColumn, intPictureRow;

            //ビットマップを作る際の画像縦横のズームサイズ
            private int intColumnZoom, intRowZoom;

            //描画値の最大最小
            public double dbleDataMax, dbleDataMin;

            //二色バージョン時最大値最小値
            public double dbleData1Max, dbleData1Min, dbleData2Max, dbleData2Min;

            private Bitmap bmpGraph;

            private PictureBox pictureBoxGraph;

            //コンストラクタ(引数：結果を描画するピクチャボックス)
            public Plot2dPlane(PictureBox pictureGraphArea)
            {
                try
                {
                    //ピクチャサイズの取得
                    intPictureColumn = pictureGraphArea.Width;
                    intPictureRow = pictureGraphArea.Height;

                    this.pictureBoxGraph = pictureGraphArea;
                    this.pictureBoxGraph.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message + "\r\nグラフの初期化に失敗しました。", "Class:KimGraph Method:Plot2dPlane");
                }
            }

            //グラフの描画命令
            public bool Draw(double[,] data)
            {
                //データの行数列数の取得
                intDataColumn = data.GetLength(0);
                intDataRow = data.GetLength(1);

                intColumnZoom = (int)(this.intPictureColumn / this.intDataColumn) + 1;
                intRowZoom = (int)(this.intPictureRow / this.intDataRow) + 1;

                intBmpColumn = this.intColumnZoom * this.intDataColumn;
                intBmpRow = this.intRowZoom * this.intDataRow;

                this.bmpGraph = new Bitmap(this.intBmpColumn, this.intBmpRow);

                //データ最大値最小値の取得
                bool test = this.GetMaxMin(data);

                //ビットマップの作成
                test = this.SetBmp(data);

                //ピクチャボックスへの描画
                this.pictureBoxGraph.Image = this.bmpGraph;

                this.bmpGraph.Save("test.bmp");

                return true;
            }

            //最大値最小値読込みバージョン
            public bool Draw(double max, double min, double[,] data)
            {
                //データの行数列数の取得
                intDataColumn = data.GetLength(0);
                intDataRow = data.GetLength(1);

                intColumnZoom = (int)(this.intPictureColumn / this.intDataColumn) + 1;
                intRowZoom = (int)(this.intPictureRow / this.intDataRow) + 1;

                intBmpColumn = this.intColumnZoom * this.intDataColumn;
                intBmpRow = this.intRowZoom * this.intDataRow;

                this.bmpGraph = new Bitmap(this.intBmpColumn, this.intBmpRow);

                //データ最大値最小値の取得
                bool test = this.GetMaxMin(data);

                //ビットマップの作成
                test = this.SetBmp(max, min, data);

                //ピクチャボックスへの描画
                this.pictureBoxGraph.Image = this.bmpGraph;

                this.bmpGraph.Save("test.bmp");

                return true;
            }

            /// <summary>
            /// 二色色分けで二つのデータを比較グラフ
            /// </summary>
            /// <param name="max1">データ１の最大値</param>
            /// <param name="min1">データ１の最小値</param>
            /// <param name="data1">データ１</param>
            /// <param name="max2">データ２の最大値</param>
            /// <param name="min2">データ２の最小値</param>
            /// <param name="data2">データ２</param>
            /// <returns></returns>
            public bool Draw(double max1, double min1, double[,] data1, double max2, double min2, double[,] data2)
            {
                //データの行数列数の取得
                intDataColumn = data1.GetLength(0);
                intDataRow = data1.GetLength(1);

                intColumnZoom = (int)(this.intPictureColumn / this.intDataColumn) + 1;
                intRowZoom = (int)(this.intPictureRow / this.intDataRow) + 1;

                intBmpColumn = this.intColumnZoom * this.intDataColumn;
                intBmpRow = this.intRowZoom * this.intDataRow;

                this.bmpGraph = new Bitmap(this.intBmpColumn, this.intBmpRow);

                //データ最大値最小値の取得
                this.dbleData1Max = max1;
                this.dbleData1Min = min1;
                this.dbleData2Max = max2;
                this.dbleData2Min = min2;

                //ビットマップの作成
                bool test = this.SetBmp(data1, data2);

                //ピクチャボックスへの描画
                this.pictureBoxGraph.Image = this.bmpGraph;

                this.bmpGraph.Save("test.bmp");

                return true;
            }

            //ビットマップへのデータ描画
            private bool SetBmp(double[,] data)
            {
                try
                {
                    Rectangle rect = new Rectangle(0, 0, this.intBmpColumn, this.intBmpRow);
                    BitmapData bmpData = this.bmpGraph.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                    int stride = bmpData.Stride;

                    unsafe
                    {
                        byte* p = (byte*)(void*)bmpData.Scan0;
                        int nResidual = stride - this.bmpGraph.Width * 3;

                        for (int i = 0; i < this.intDataRow; i++)
                        {
                            for (int n = 0; n < this.intRowZoom; n++)
                            {
                                for (int j = 0; j < this.intDataColumn; j++)
                                {
                                    for (int m = 0; m < this.intColumnZoom; m++)
                                    {
                                        switch (this.gradationMode)
                                        {
                                            case (enumGradation.mono):
                                                {
                                                    if (data[j, i] > dbleDataMax)
                                                    {
                                                        p[2] = (byte)255.0;
                                                        p[1] = (byte)255.0;
                                                        p[0] = (byte)255.0;
                                                    }
                                                    else if (data[j, i] < dbleDataMin)
                                                    {
                                                        p[2] = (byte)0.0;
                                                        p[1] = (byte)0.0;
                                                        p[0] = (byte)0.0;
                                                    }
                                                    else
                                                    {
                                                        p[2] = (byte)(((data[j, i] - (this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin)) * 255.0));
                                                        p[1] = (byte)(((data[j, i] - (this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin)) * 255.0));
                                                        p[0] = (byte)(((data[j, i] - (this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin)) * 255.0));
                                                    }
                                                    break;
                                                }
                                            case (enumGradation.fullcolor1):
                                                {
                                                    if (data[j, i] > dbleDataMax)
                                                    {
                                                        p[2] = (byte)255.0;
                                                        p[1] = (byte)0.0;
                                                        p[0] = (byte)0.0;
                                                    }
                                                    else if (data[j, i] < dbleDataMin)
                                                    {
                                                        p[2] = (byte)0.0;
                                                        p[1] = (byte)0.0;
                                                        p[0] = (byte)255.0;
                                                    }
                                                    else if (data[j, i] >= ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 2.0 + this.dbleDataMin))
                                                    {
                                                        p[2] = (byte)(((data[j, i] - ((this.dbleDataMax + this.dbleDataMin) / 2.0)) / ((this.dbleDataMax - this.dbleDataMin) / 2.0) * 255.0));
                                                        p[1] = (byte)(255 - ((data[j, i] - ((this.dbleDataMax + this.dbleDataMin) / 2.0)) / ((this.dbleDataMax - this.dbleDataMin) / 2.0) * 255.0));
                                                        p[0] = (byte)0;
                                                    }
                                                    else
                                                    {
                                                        p[2] = (byte)0;
                                                        p[1] = (byte)((((data[j, i] - this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 2.0) * 255.0));
                                                        p[0] = (byte)(255 - ((data[j, i] - (this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 2.0) * 255.0));
                                                    }
                                                    break;
                                                }
                                            case (enumGradation.fullcolor2):
                                                {
                                                    if (data[j, i] > dbleDataMax)
                                                    {
                                                        p[2] = (byte)255.0;
                                                        p[1] = (byte)0.0;
                                                        p[0] = (byte)0.0;
                                                    }
                                                    else if (data[j, i] < dbleDataMin)
                                                    {
                                                        p[2] = (byte)0.0;
                                                        p[1] = (byte)0.0;
                                                        p[0] = (byte)255.0;
                                                    }
                                                    else if (data[j, i] >= ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 3.0 + this.dbleDataMin))
                                                    {
                                                        p[2] = (byte)255;
                                                        p[1] = (byte)(255 - ((data[j, i] - ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 3.0 + this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                        p[0] = (byte)0;
                                                    }
                                                    else if (data[j, i] >= ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 2.0 + this.dbleDataMin))
                                                    {
                                                        p[2] = (byte)(((data[j, i] - ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 2.0 + this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                        p[1] = (byte)255;
                                                        p[0] = (byte)0;
                                                    }
                                                    else if (data[j, i] >= ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 1.0 + this.dbleDataMin))
                                                    {
                                                        p[2] = (byte)0;
                                                        p[1] = (byte)255;
                                                        p[0] = (byte)(255 - ((data[j, i] - ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 1.0 + this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                    }
                                                    else
                                                    {
                                                        p[2] = (byte)0;
                                                        p[1] = (byte)((((data[j, i] - this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                        p[0] = (byte)255;
                                                    }
                                                    break;
                                                }
                                            case (enumGradation.fullcolor3):
                                                {
                                                    if (data[j, i] == 0.0)
                                                    {
                                                        p[2] = (byte)0;
                                                        p[1] = (byte)0;
                                                        p[0] = (byte)0;
                                                    }
                                                    else if (data[j, i] > dbleDataMax)
                                                    {
                                                        p[2] = (byte)255.0;
                                                        p[1] = (byte)0.0;
                                                        p[0] = (byte)0.0;
                                                    }
                                                    else if (data[j, i] < dbleDataMin)
                                                    {
                                                        p[2] = (byte)0.0;
                                                        p[1] = (byte)0.0;
                                                        p[0] = (byte)255.0;
                                                    }
                                                    else if (data[j, i] >= ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 3.0 + this.dbleDataMin))
                                                    {
                                                        p[2] = (byte)255;
                                                        p[1] = (byte)(255 - ((data[j, i] - ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 3.0 + this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                        p[0] = (byte)0;
                                                    }
                                                    else if (data[j, i] >= ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 2.0 + this.dbleDataMin))
                                                    {
                                                        p[2] = (byte)(((data[j, i] - ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 2.0 + this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                        p[1] = (byte)255;
                                                        p[0] = (byte)0;
                                                    }
                                                    else if (data[j, i] >= ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 1.0 + this.dbleDataMin))
                                                    {
                                                        p[2] = (byte)0;
                                                        p[1] = (byte)255;
                                                        p[0] = (byte)(255 - ((data[j, i] - ((this.dbleDataMax - this.dbleDataMin) / 4.0 * 1.0 + this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                    }
                                                    else
                                                    {
                                                        p[2] = (byte)0;
                                                        p[1] = (byte)((((data[j, i] - this.dbleDataMin)) / ((this.dbleDataMax - this.dbleDataMin) / 4.0) * 255.0));
                                                        p[0] = (byte)255;
                                                    }
                                                    break;
                                                }

                                        }
                                        p += 3;
                                    }
                                }
                                p += nResidual;
                            }
                        }

                        this.bmpGraph.UnlockBits(bmpData);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message + "\r\nビットマップへのデータの描画に失敗しました。", "Glass:KimGraph Method:SetBmp");
                    return false;
                }
            }

            //ビットマップへのデータ描画:最大値最小値指定バージョン
            private bool SetBmp(double dbleInnerDataMax, double dbleInnerDataMin, double[,] data)
            {
                try
                {
                    Rectangle rect = new Rectangle(0, 0, this.intBmpColumn, this.intBmpRow);
                    BitmapData bmpData = this.bmpGraph.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                    int stride = bmpData.Stride;

                    unsafe
                    {
                        byte* p = (byte*)(void*)bmpData.Scan0;
                        int nResidual = stride - this.bmpGraph.Width * 3;

                        for (int i = 0; i < this.intDataRow; i++)
                        {
                            for (int n = 0; n < this.intRowZoom; n++)
                            {
                                for (int j = 0; j < this.intDataColumn; j++)
                                {
                                    for (int m = 0; m < this.intColumnZoom; m++)
                                    {
                                        switch (this.gradationMode)
                                        {
                                            case (enumGradation.mono):
                                                {
                                                    if (data[j, i] > dbleInnerDataMax)
                                                    {
                                                        p[2] = (byte)255.0;
                                                        p[1] = (byte)255.0;
                                                        p[0] = (byte)255.0;
                                                    }
                                                    else if (data[j, i] < dbleInnerDataMin)
                                                    {
                                                        p[2] = (byte)0.0;
                                                        p[1] = (byte)0.0;
                                                        p[0] = (byte)0.0;
                                                    }
                                                    else
                                                    {
                                                        p[2] = (byte)(((data[j, i] - (dbleDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin)) * 255.0));
                                                        p[1] = (byte)(((data[j, i] - (dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin)) * 255.0));
                                                        p[0] = (byte)(((data[j, i] - (dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin)) * 255.0));
                                                    }
                                                    break;
                                                }
                                            case (enumGradation.fullcolor1):
                                                {
                                                    if (data[j, i] > dbleInnerDataMax)
                                                    {
                                                        p[2] = (byte)255.0;
                                                        p[1] = (byte)0.0;
                                                        p[0] = (byte)0.0;
                                                    }
                                                    else if (data[j, i] < dbleInnerDataMin)
                                                    {
                                                        p[2] = (byte)0.0;
                                                        p[1] = (byte)0.0;
                                                        p[0] = (byte)255.0;
                                                    }
                                                    else if (data[j, i] >= ((dbleInnerDataMax - dbleInnerDataMin) / 4.0 * 2.0 + dbleInnerDataMin))
                                                    {
                                                        p[2] = (byte)(((data[j, i] - ((dbleInnerDataMax + dbleInnerDataMin) / 2.0)) / ((dbleInnerDataMax - dbleInnerDataMin) / 2.0) * 255.0));
                                                        p[1] = (byte)(255 - ((data[j, i] - ((dbleInnerDataMax + dbleInnerDataMin) / 2.0)) / ((dbleInnerDataMax - dbleInnerDataMin) / 2.0) * 255.0));
                                                        p[0] = (byte)0;
                                                    }
                                                    else
                                                    {
                                                        p[2] = (byte)0;
                                                        p[1] = (byte)((((data[j, i] - dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin) / 2.0) * 255.0));
                                                        p[0] = (byte)(255 - ((data[j, i] - (dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin) / 2.0) * 255.0));
                                                    }
                                                    break;
                                                }
                                            case (enumGradation.fullcolor2):
                                                {
                                                    if (data[j, i] > dbleInnerDataMax)
                                                    {
                                                        p[2] = (byte)255.0;
                                                        p[1] = (byte)0.0;
                                                        p[0] = (byte)0.0;
                                                    }
                                                    else if (data[j, i] < dbleInnerDataMin)
                                                    {
                                                        p[2] = (byte)0.0;
                                                        p[1] = (byte)0.0;
                                                        p[0] = (byte)255.0;
                                                    }
                                                    else if (data[j, i] >= ((dbleInnerDataMax - dbleInnerDataMin) / 4.0 * 3.0 + dbleInnerDataMin))
                                                    {
                                                        p[2] = (byte)255;
                                                        p[1] = (byte)(255 - ((data[j, i] - ((dbleInnerDataMax - dbleInnerDataMin) / 4.0 * 3.0 + dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin) / 4.0) * 255.0));
                                                        p[0] = (byte)0;
                                                    }
                                                    else if (data[j, i] >= ((dbleInnerDataMax - dbleInnerDataMin) / 4.0 * 2.0 + dbleInnerDataMin))
                                                    {
                                                        p[2] = (byte)(((data[j, i] - ((dbleInnerDataMax - dbleInnerDataMin) / 4.0 * 2.0 + dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin) / 4.0) * 255.0));
                                                        p[1] = (byte)255;
                                                        p[0] = (byte)0;
                                                    }
                                                    else if (data[j, i] >= ((dbleInnerDataMax - dbleInnerDataMin) / 4.0 * 1.0 + dbleInnerDataMin))
                                                    {
                                                        p[2] = (byte)0;
                                                        p[1] = (byte)255;
                                                        p[0] = (byte)(255 - ((data[j, i] - ((dbleInnerDataMax - dbleInnerDataMin) / 4.0 * 1.0 + dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin) / 4.0) * 255.0));
                                                    }
                                                    else
                                                    {
                                                        p[2] = (byte)0;
                                                        p[1] = (byte)((((data[j, i] - dbleInnerDataMin)) / ((dbleInnerDataMax - dbleInnerDataMin) / 4.0) * 255.0));
                                                        p[0] = (byte)255;
                                                    }
                                                    break;
                                                }
                                        }
                                        p += 3;
                                    }
                                }
                                p += nResidual;
                            }
                        }

                        this.bmpGraph.UnlockBits(bmpData);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message + "\r\nビットマップへのデータの描画に失敗しました。", "Glass:KimGraph Method:SetBmp");
                    return false;
                }
            }

            //ビットマップへのデータ描画：二色バージョン
            private bool SetBmp(double[,] data1, double[,] data2)
            {
                try
                {
                    Rectangle rect = new Rectangle(0, 0, this.intBmpColumn, this.intBmpRow);
                    BitmapData bmpData = this.bmpGraph.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

                    int stride = bmpData.Stride;

                    unsafe
                    {
                        byte* p = (byte*)(void*)bmpData.Scan0;
                        int nResidual = stride - this.bmpGraph.Width * 3;

                        for (int i = 0; i < this.intDataRow; i++)
                        {
                            for (int n = 0; n < this.intRowZoom; n++)
                            {
                                for (int j = 0; j < this.intDataColumn; j++)
                                {
                                    for (int m = 0; m < this.intColumnZoom; m++)
                                    {
                                        p[1] = (byte)0.0;

                                        if (data1[j, i] > dbleData1Max)
                                        {
                                            p[0] = (byte)255.0;
                                        }
                                        else if (data1[j, i] < dbleData1Min)
                                        {
                                            p[0] = (byte)0.0;
                                        }
                                        else
                                        {
                                            p[0] = (byte)(((data1[j, i] - (this.dbleData1Min)) / ((this.dbleData1Max - this.dbleData1Min)) * 255.0));
                                        }

                                        if (data2[j, i] > dbleData2Max)
                                        {
                                            p[2] = (byte)255.0;
                                        }
                                        else if (data2[j, i] < dbleData2Min)
                                        {
                                            p[2] = (byte)0.0;
                                        }
                                        else
                                        {
                                            p[2] = (byte)(((data2[j, i] - (this.dbleData2Min)) / ((this.dbleData2Max - this.dbleData2Min)) * 255.0));
                                        }

                                        p += 3;
                                    }
                                }
                                p += nResidual;
                            }
                        }

                        this.bmpGraph.UnlockBits(bmpData);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message + "\r\nビットマップへのデータの描画に失敗しました。", "Glass:KimGraph Method:SetBmp");
                    return false;
                }
            }


            //データの最大値最小値の取得
            public bool GetMaxMin(double[,] data)
            {
                try
                {
                    int column, row;

                    //要素数の取得
                    column = data.GetLength(0);
                    row = data.GetLength(1);

                    //最大値最小値の初期化
                    this.dbleDataMax = -1.0e100;
                    this.dbleDataMin = 1.0e100;

                    //最大値最小値の取得
                    for (int i = 0; i < column; i++)
                    {
                        for (int j = 0; j < row; j++)
                        {
                            if (data[i, j] > dbleDataMax)
                            {
                                dbleDataMax = data[i, j];
                            }
                            else if (data[i, j] < dbleDataMin)
                            {
                                dbleDataMin = data[i, j];
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message + "\r\n最大値最小値の取得に失敗しました。", "Glass:KimGraph Method:GetMaxMin");
                    return false;
                }
            }
        }

    }

    namespace FileIO
    {
        public static class FileIO
        {

#region FileInput


            public static void readFile(string filePath, ref double[,] readData)
            {
                //データ読み込み用変数
                string subStr;
                string[] subStrArr;
                string[] subStrArrColumn;
                string[] sep = new string[] { "  ", " ", "\t", ",", ",          " };
                string[] sepCol = new string[] { "\r\n", "\n", "\r" };

                StreamReader sr = new StreamReader(filePath);
                subStr = sr.ReadToEnd();
                subStrArrColumn = subStr.Split(sepCol, StringSplitOptions.RemoveEmptyEntries);

                readData = new double[subStrArrColumn[0].Split(sep, StringSplitOptions.RemoveEmptyEntries).Length, subStrArrColumn.Length];

                //'08/11/11追記
                int intX = subStrArrColumn[0].Split(sep, StringSplitOptions.RemoveEmptyEntries).Length;

                for (int i = 0; i < subStrArrColumn.Length; i++)
                {
                    subStrArr = subStrArrColumn[i].Split(sep, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < intX; j++)
                    {
                        Double.TryParse(subStrArr[j], out readData[j, i]);
                    }
                }
                sr.Close();
            }



            public static void readFile(string filePath, ref string[,] readString)
            {
                //データ読み込み用変数
                string subStr;
                string[] subStrArr;
                string[] subStrArrColumn;
                string[] sep = new string[] { "  ", " ", "\t", ",", ",          " };
                string[] sepCol = new string[] { "\r\n", "\n", "\r" };

                StreamReader sr = new StreamReader(filePath);
                subStr = sr.ReadToEnd();
                subStrArrColumn = subStr.Split(sepCol, StringSplitOptions.RemoveEmptyEntries);

                readString = new string[subStrArrColumn[0].Split(sep, StringSplitOptions.RemoveEmptyEntries).Length, subStrArrColumn.Length];

                for (int i = 0; i < subStrArrColumn.Length; i++)
                {
                    subStrArr = subStrArrColumn[i].Split(sep, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < subStrArr.Length; j++)
                    {
                        readString[j, i] = subStrArr[j];
                    }
                }
                sr.Close();
            }

            /// <summary>
            /// 一列のデータ読み込み用：列がたくさんある場合は最初の列の値を読み込む
            /// </summary>
            /// <param name="filePath">読み込みファイルネーム</param>
            /// <param name="readData">出力データファイル</param>
            public static void readFile(string filePath, ref double[] readData)
            {
                //データ読み込み用変数
                string subStr;
                string[] subStrArr;
                string[] subStrArrColumn;
                string[] sep = new string[] { "  ", " ", "\t", ",", ",          " };
                string[] sepCol = new string[] { "\r\n", "\n", "\r" };

                StreamReader sr = new StreamReader(filePath);
                subStr = sr.ReadToEnd();
                subStrArrColumn = subStr.Split(sepCol, StringSplitOptions.RemoveEmptyEntries);

                readData = new double[subStrArrColumn.Length];

                for (int i = 0; i < subStrArrColumn.Length; i++)
                {
                    subStrArr = subStrArrColumn[i].Split(sep, StringSplitOptions.RemoveEmptyEntries);

                    //この"0"の値を変えることで異なる列の値を読みだすことが可能
                    readData[i] = Convert.ToDouble(subStrArr[0]);
                }
                sr.Close();
            }

#endregion FileInput

#region FileOutput

            /// <summary>
            /// データ書込
            /// </summary>
            /// <typeparam name="Type"></typeparam>
            /// <param name="filePath">ファイルパス</param>
            /// <param name="writeData">書き込むデータ</param>
            /// <param name="append">false:上書き true:追記</param>
            public static void writeFile<Type>(string filePath, Type writeData, bool append = false)
            {
                StringBuilder sbData = new StringBuilder();
                sbData.AppendLine(Convert.ToString(writeData));
                if (append)
                    File.AppendAllText(filePath, sbData.ToString());
                else
                    File.WriteAllText(filePath, sbData.ToString());
            }

            /// <summary>
            /// 1次元データ書込
            /// </summary>
            /// <typeparam name="Type"></typeparam>
            /// <param name="filePath">ファイルパス</param>
            /// <param name="writeData">書き込むデータ</param>
            /// <param name="append">false:上書き true:追記</param>
            public static void writeFile<Type>(string filePath, Type[] writeData, bool append = false)
            {
                StringBuilder sbData = new StringBuilder();
                for (int i = 0; i < writeData.Length; i++)
                    sbData.AppendLine(Convert.ToString(writeData[i]));

                if (append)
                    File.AppendAllText(filePath, sbData.ToString());
                else
                    File.WriteAllText(filePath, sbData.ToString());
            }

            /// <summary>
            /// 2次元データ書込
            /// </summary>
            /// <typeparam name="Type"></typeparam>
            /// <param name="filePath">ファイルパス</param>
            /// <param name="writeData">書き込むデータ</param>
            /// <param name="append">false:上書き true:追記</param>
            public static void writeFile<Type>(string filePath, Type[,] writeData, bool append = false)
            {
                StringBuilder sbData = new StringBuilder();
                for (int j = 0; j < writeData.GetLength(1); j++)
                {
                    for (int i = 0; i < writeData.GetLength(0); i++)
                        sbData.Append(Convert.ToString(writeData[i, j])).Append(" ");
                    sbData.AppendLine("");
                }

                if (append)
                    File.AppendAllText(filePath, sbData.ToString());
                else
                    File.WriteAllText(filePath, sbData.ToString());

            }

            /// <summary>
            /// 一次元×2データ書き込み
            /// </summary>
            /// <param name="filePath">ファイルパス</param>
            /// <param name="writeData1"></param>
            /// <param name="writeData2"></param>
            /// <param name="append"></param>
            public static void writeFile<Type>(string filePath, Type[] writeData1, Type[] writeData2, bool append = false)
            {
                StringBuilder sbData = new StringBuilder();
                for (int i = 0; i < writeData1.Length; i++)
                    sbData.AppendLine(Convert.ToString(writeData1[i]) + "," + Convert.ToString(writeData2[i]));

                if (append)
                    File.AppendAllText(filePath, sbData.ToString());
                else
                    File.WriteAllText(filePath, sbData.ToString());

            }

#endregion FileOutput
        }

    }

    namespace OpenCL
    {

    }

}
