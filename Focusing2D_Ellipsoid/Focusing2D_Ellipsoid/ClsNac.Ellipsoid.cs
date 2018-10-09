using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace ClsNac
{
    class Ellipsoid
    {
        public class Coord
        {
            Type[,] coord_trans<Type>(Type[] _type)
            {
                Type[,] type = new Type[div1, div2];
                for (int i = 0; i < div1; i++)
                {
                    for (int j = 0; j < div2; j++)
                    {
                        type[i, j] = _type[i * div2 + j];
                    }
                }
                return type;
            }

            Type[] coord_trans<Type>(Type[,] _type)
            {
                Type[] type = new Type[div];
                for (int i = 0; i < div1; i++)
                {
                    for (int j = 0; j < div2; j++)
                    {
                        type[i * div2 + j] = _type[i, j];
                    }
                }
                return type;
            }


            #region 変数

            public int div1 { get; private set; }
            public int div2 { get; private set; }
            public int div { get { return div1 * div2; } }
            public double xc, yc, zc;

            public double[,] x { get; set; }
            public double[,] y { get; set; }
            public double[,] z { get; set; }
            public double[] xv;
            public double[] yv;
            public double[] zv;

            public double[] rx { get; set; }
            public double[] ry { get; set; }

            public double[,] Real { get; set; }
            public double[,] Imaginry { get; set; }
            double[] _realv;
            public double[] realv
            {
                get
                {
                    _realv = new double[div];
                    for (int i = 0; i < div1; i++)
                    {
                        for (int j = 0; j < div2; j++)
                        {
                            _realv[i * div1 + j] = u[i, j].Real;
                        }
                    }
                    return _realv;
                }
            }
            double[] _imagv;
            public double[] imagv
            {
                get
                {
                    _imagv = new double[div];
                    for (int i = 0; i < div1; i++)
                    {
                        for (int j = 0; j < div2; j++)
                        {
                            _imagv[i * div1 + j] = u[i, j].Imaginary;
                        }
                    }
                    return _imagv;
                }
            }

            public double[,] Intensity { get; set; }
            public double[,] Phase { get; set; }

            #endregion

            public Coord(int _divW, int _divL)
            {
                Initialize(_divW, _divL);
            }

            public void Initialize(int _divW, int _divL)
            {
                div1 = _divW;
                div2 = _divL;

                x = new double[div1, div2];
                y = new double[div1, div2];
                z = new double[div1, div2];

                rx = new double[div2];
                ry = new double[div2];

                Intensity = new double[div1, div2];
                Phase = new double[div1, div2];

            }

            #region 座標補正
            /// <summary>
            /// xy座標回転
            /// </summary>
            /// <param name="rotTheta">回転角度</param>
            /// <param name="x0">x回転中心</param>
            /// <param name="y0">y回転中心</param>
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

        public class CoordM : Coord

        {
            #region 変数

            public double[,] z_mod
            {
                get
                {
                    double[,] _zmod = new double[div1, div2];

                    double[] _x = new double[div2];
                    double[] _z = new double[div2];
                    for (int j = 0; j < div2; j++)
                    {
                        _x[j] = x[div1 / 2, j];
                        _z[j] = z[div1 / 2, j];
                    }

                    var p = Fit.Line(_x, _z);
                    double min = 1e10;
                    for (int i = 0; i < div1; i++)
                    {
                        for (int j = 0; j < div2; j++)
                        {
                            _zmod[i, j] = z[i, j] - (p.Item2 * _x[j] + p.Item1);
                            if (_zmod[i, j] < min) min = _zmod[i, j];
                        }
                    }

                    for (int i = 0; i < div1; i++)
                    {
                        for (int j = 0; j < div2; j++)
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
                    for (int i = 0; i < i1; i++)
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

            public void Initialize(int _nx, double _dx, int _ny, double _dy, int _nz, double _dz)
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

                for (int i = 0; i < nx; i++)
                {
                    x[i] = new double[ny, nz];
                    y[i] = new double[ny, nz];
                    z[i] = new double[ny, nz];
                    u[i] = new Complex[ny, nz];
                    Intensity[i] = new double[ny, nz];
                }
            }
        }


    }
}
