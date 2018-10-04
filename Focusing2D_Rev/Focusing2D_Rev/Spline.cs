using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using System.Threading;

namespace ClsNk
{

    //2013/04/05更新
    class Spline
    {
        private double[] orgData1;
        private double[][] orgData2;
        private bool[,] zero2;
        private double[] yy1;
        private double[][] yy2;
        private double[] posX1, posX2;
        private int nx1, nx2;
        private int coreN;

        #region コンストラクタ

        /// <summary>
        /// 一次元
        /// </summary>
        /// <param name="_orgData">元データ</param>
        public Spline(double[] _orgData,double[] _posX=null, int _coreN = 1)
        {
            nx1 = _orgData.Length;
            coreN = _coreN;

            orgData1 = new double[nx1];
            Array.Copy(_orgData, orgData1, nx1);

            posX1 = new double[nx1];
            if (_posX == null)
            {
                for (int i = 0; i < nx1; i++)
                    posX1[i] = i;
            }
            else
            {
                Array.Copy(posX1, _posX, nx1);
            }
            //導関数を求めておく
            spline(posX1, orgData1, ref yy1);
        }

        /// <summary>
        /// 二次元
        /// </summary>
        /// <param name="_orgData">元データ</param>
        public Spline(double[,] _orgData, int x1, int x2, int y1, int y2, int _coreN = 1)
        {
            nx1 = _orgData.GetLength(0);
            nx2 = _orgData.GetLength(1);
            coreN = _coreN;

            zero2 = zero(_orgData);
            _orgData = hosei(_orgData, x1, x2, y1, y2);

            orgData2 = new double[nx1][];
            for (int i = 0; i < nx1; i++)
            {
                orgData2[i] = new double[nx2];
                for (int j = 0; j < nx2; j++)
                    orgData2[i][j] = _orgData[i, j];
            }

            posX1 = new double[nx1];
            for (int i = 0; i < nx1; i++)
                posX1[i] = i;

            posX2 = new double[nx2];
            for (int j = 0; j < nx2; j++)
                posX2[j] = j;

            //導関数を求めておく
            spline(posX2, orgData2, ref yy2);
        }

        public Spline(double[,] _orgData, int _coreN = 1)
        {
            nx1 = _orgData.GetLength(0);
            nx2 = _orgData.GetLength(1);
            coreN = _coreN;

            zero2 = zero(_orgData);
            _orgData = hosei(_orgData, 0, _orgData.GetLength(0), 0, _orgData.GetLength(1));

            orgData2 = new double[nx1][];
            for (int i = 0; i < nx1; i++)
            {
                orgData2[i] = new double[nx2];
                for (int j = 0; j < nx2; j++)
                    orgData2[i][j] = _orgData[i, j];
            }

            posX1 = new double[nx1];
            for (int i = 0; i < nx1; i++)
                posX1[i] = i;

            posX2 = new double[nx2];
            for (int j = 0; j < nx2; j++)
                posX2[j] = j;

            //導関数を求めておく
            spline(posX2, orgData2, ref yy2);
        }


        #endregion コンストラクタ

        #region 実行

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sx"></param>
        /// <returns></returns>
        public double Execute(double sx)
        {
            return splint(posX1, orgData1, yy1, sx);
        }

        public double[] Execute(double[] sx)
        {
            double[] y = new double[sx.Length];
            for (int i = 0; i < sx.Length; i++)
                y[i] = splint(posX1, orgData1, yy1, sx[i]);
            return y;
        }

        public double Execute(double sx1, double sx2)
        {
            return splint(posX1, posX2, orgData2, yy2, sx1, sx2);
        }

        public double[] Execute(double[] sx1, double[] sx2)
        {
            double[] y = new double[sx1.Length];
            for (int i = 0; i < sx1.Length; i++)
            {
                y[i] = Execute(sx1[i], sx2[i]);
            }
            return y;
        }

        //こいつはもっと高速化できる．
        public double[,] Execute2(double[] sx1, double[] sx2)
        {
            double[,] y = new double[sx1.Length, sx2.Length];
            Parallel.For(0, sx1.Length, i =>
            {
                for (int j = 0; j < sx2.Length; j++)
                    y[i, j] = splint(posX1, posX2, orgData2, yy2, sx1[i], sx2[j]);
            });
            return y;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sx1"></param>
        /// <param name="sx2"></param>
        /// <returns></returns>
        public double[,] Execute(double[,] sx1, double[,] sx2)
        {
            double[,] y = new double[sx1.GetLength(0), sx2.GetLength(1)];

            Parallel.For(0, sx1.GetLength(0), i =>
            {
                for (int j = 0; j < sx2.GetLength(1); j++)
                {
                    if (sx1[i, j] < posX1[0] && sx1[i, j] > posX1[nx1 - 1]
                        && sx2[i, j] < posX2[0] && sx2[i, j] > posX2[nx2 - 1])
                        y[i, j] = 0.0;
                    else if ((int)sx1[i, j] >= 0 && (int)sx1[i, j] < sx1.GetLength(0)
                            && (int)sx2[i, j] >= 0 && (int)sx2[i, j] < sx2.GetLength(1)
                            && zero2[(int)sx1[i, j], (int)sx2[i, j]] == true)
                    {
                        y[i, j] = 0.0;
                    }
                    else
                        y[i, j] = splint(posX1, posX2, orgData2, yy2, sx1[i, j], sx2[i, j]);


                    for (int ii = 0; ii < sx1.GetLength(0); ii++)
                    {
                        if (j < sx2[ii, 0] || j > sx2[ii, sx2.GetLength(1) - 1])
                        {
                            y[i, j] = 0.0;
                            break;
                        }
                    }
                    for (int jj = 0; jj < sx2.GetLength(1); jj++)
                    {
                        if (i < sx1[0, jj] || i > sx1[sx1.GetLength(0) - 1, jj])
                        {
                            y[i, j] = 0.0;
                            break;
                        }
                    }


                }
            });

            //もともと0のところを0になおす
            //Parallel.For(0, sx1.GetLength(0), i =>
            //{
            //    for (int j = 0; j < sx2.GetLength(1); j++)
            //    {
            //        for (int ii = 0; ii < sx1.GetLength(0); ii++)
            //        {
            //            for (int jj = 0; jj < sx2.GetLength(1); jj++)
            //            {
            //                if (i < sx1[ii, jj] || i > sx1[ii, jj]
            //                    || j < sx2[ii, jj] || j > sx2[ii, jj])
            //                {
            //                    y[i, j] = 0.0;
            //                }
            //            }
            //        }

            //        if ((int)sx1[i, j] >= 0 && (int)sx1[i, j] < sx1.GetLength(0)
            //            && (int)sx2[i, j] >= 0 && (int)sx2[i, j] < sx2.GetLength(1))
            //            if (zero2[(int)sx1[i, j], (int)sx2[i, j]] == true)
            //            {
            //                y[i, j] = 0.0;
            //            }
            //    }
            //}, coreN);

            return y;
        }

        #endregion

        #region splineサブルーチン


        /// <summary>
        /// ２階導関数の導出(1D)
        /// </summary>
        /// <param name="x">x座標</param>
        /// <param name="y">y値</param>
        /// <param name="y2">２階導関数</param>
        private static void spline(double[] x, double[] y, ref double[] y2)
        {
            int iMax = x.Length;
            y2 = new double[iMax];
            double[] u = new double[iMax];
            double sig, p;

            y2[0] = u[0] = 0.0;
            for (int i = 1; i < iMax - 1; i++)
            {
                sig = (x[i] - x[i - 1]) / (x[i + 1] - x[i - 1]);
                p = sig * y2[i - 1] + 2.0;
                y2[i] = (sig - 1.0) / p;
                u[i] = (y[i + 1] - y[i]) / (x[i + 1] - x[i]) - (y[i] - y[i - 1]) / (x[i] - x[i - 1]);
                u[i] = (6.0 * u[i] / (x[i + 1] - x[i - 1]) - sig * u[i - 1]) / p;
            }
            y2[iMax - 1] = u[iMax - 1] = 0.0;
            for (int i = iMax - 2; i >= 0; i--)
                y2[i] = y2[i] * y2[i + 1] + u[i];

        }

        /// <summary>
        /// ２階導関数の導出(2D)
        /// </summary>
        /// <param name="x">x座標</param>
        /// <param name="y">y値</param>
        /// <param name="y2">２階導関数</param>
        private static void spline(double[] x, double[][] y, ref double[][] y2)
        {
            y2 = new double[y.GetLength(0)][];
            for (int i = 0; i < y.GetLength(0); i++)
                spline(x, y[i], ref y2[i]);
        }

        /// <summary>
        /// 補間値を導出(1D)
        /// </summary>
        /// <param name="x">x座標</param>
        /// <param name="y">y値</param>
        /// <param name="y2">２階導関数</param>
        /// <param name="sx"></param>
        /// <returns></returns>
        private static double splint(double[] x, double[] y, double[] y2, double sx)
        {
            //sxを計算するxの範囲を探す
            int kLo = 0;
            int kHi = x.Length - 1;
            int k = 0;
            while (kHi - kLo > 1)
            {
                k = (kHi + kLo) >> 1;
                if (x[k] > sx) kHi = k;
                else kLo = k;
            }
            
            //スプライン補間
            double h = x[kHi] - x[kLo];
            double a = (x[kHi] - sx) / h;
            double b = (sx - x[kLo]) / h;

            return a * y[kLo] + b * y[kHi] + ((Math.Pow(a, 3.0) - a) * y2[kLo] + (Math.Pow(b, 3.0) - b) * y2[kHi]) * (h * h) / 6.0;
        }

        /// <summary>
        /// 補間値を導出(2D)
        /// </summary>
        /// <param name="x1">x1座標</param>
        /// <param name="x2">x2座標</param>
        /// <param name="y">y値</param>
        /// <param name="y2">２階導関数</param>
        /// <param name="sx1">補間x1座標</param>
        /// <param name="sx2">補間x2座標</param>
        /// <returns></returns>
        private static double splint(double[] x1, double[] x2, double[][] y, double[][] y2, double sx1, double sx2, int n = 1)
        {
            double[] ytmp = new double[x1.Length];
            double[] yytmp = new double[x1.Length];
            for (int i = 0; i < x1.Length; i++)
            {
                yytmp[i] = splint(x2, y[i], y2[i], sx2);
            }
            spline(x1, yytmp, ref ytmp);
            return splint(x1, yytmp, ytmp, sx1);
        }


        #endregion


        private double[,] hosei(double[,] data, int x1, int x2, int y1, int y2)
        {
            //_orgDataのデータ抜けを補正

            int i1 = 0, j1 = 0;
            for (int i = x1; i < x2; i++)
            {
                for (int j = y1; j < y2; j++)
                {
                    if (data[i, j] != 0.0)
                    {
                        i1 = i;
                        j1 = j;
                        goto out1;
                    }
                }
            }

        out1:

            int i2 = 0, j2 = 0;
            for (int j = y1; j < y2; j++)
            {
                for (int i = x1; i < x2; i++)
                {
                    if (data[i, j] != 0.0)
                    {
                        i2 = i;
                        j2 = j;
                        goto out2;
                    }
                }
            }

        out2:

            int i3 = 0, j3 = 0;
            for (int i = x2-1; i >= x1; i--)
            {
                for (int j = y1; j < y2; j++)
                {
                    if (data[i, j] != 0.0)
                    {
                        i3 = i;
                        j3 = j;
                        goto out3;
                    }
                }
            }

        out3:

            int i4 = 0, j4 = 0;
            for (int j = y2-1; j >= y1; j--)
            {
                for (int i = x1; i < x2; i++)
                {
                    if (data[i, j] != 0.0)
                    {
                        i4 = i;
                        j4 = j;
                        goto out4;
                    }
                }
            }
        out4:

            for (int i = i1; i < i2; i++)
            {
                for (int j = y1; j < y2; j++)
                {
                    if (data[i, j] != 0.0)
                    {
                        for (int jj = 0; jj < j; jj++)
                        {
                            data[i, jj] = data[i, j];
                        }
                        break;
                    }
                }
            }

            for (int i = i4; i < i3; i++)
            {
                for (int j = y2-1; j >= y1; j--)
                {
                    if (data[i, j] != 0.0)
                    {
                        for (int jj = j; jj < 480; jj++)
                        {
                            data[i, jj] = data[i, j];
                        }
                        break;
                    }
                }
            }

            for (int j = j1; j < j4; j++)
            {
                for (int i = x1; i < x2; i++)
                {
                    if (data[i, j] != 0.0)
                    {
                        for (int ii = 0; ii < i; ii++)
                        {
                            data[ii, j] = data[i, j];
                        }
                        break;
                    }
                }
            }

            for (int j = j2; j < j3; j++)
            {
                for (int i = x2-1; i >= x1; i--)
                {
                    if (data[i, j] != 0.0)
                    {
                        for (int ii = i; ii < 640; ii++)
                        {
                            data[ii, j] = data[i, j];
                        }
                        break;
                    }
                }
            }

            for (int i = 0; i < i1; i++)
            {
                for (int j = 0; j < j1; j++)
                {
                    data[i, j] = data[i1, j1];
                }
            }

            for (int i = i2; i < 640; i++)
            {
                for (int j = 0; j < j2; j++)
                {
                    data[i, j] = data[i2, j2];
                }
            }

            for (int i = i3; i < 640; i++)
            {
                for (int j = j3; j < 480; j++)
                {
                    data[i, j] = data[i3, j3];
                }
            }

            for (int i = 0; i < i4; i++)
            {
                for (int j = j4; j < 480; j++)
                {
                    data[i, j] = data[i4, j4];
                }
            }

            for (int i = 0; i < 640; i++)
            {
                for (int j = 0; j < 480; j++)
                {
                    if (data[i, j] == 0.0)
                    {
                        for (int jj = j; jj < 480; jj++)
                        {
                            if (data[i, jj] != 0.0)
                            {
                                double a = (data[i, j] - data[i, jj]) / (double)(j - jj);
                                double b = data[i, j] - a * j;
                                for (int jjj = j; jjj < jj; jjj++)
                                {
                                    data[i, jjj] = a * jjj + b;
                                }
                                j = jj;
                                break;
                            }
                        }
                    }
                }
            }

            return data;


        }

        private bool[,] zero(double[,] data)
        {
            bool[,] zero = new bool[data.GetLength(0), data.GetLength(1)];

            for (int i = 0; i < data.GetLength(0); i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    if (data[i, j] == 0.0)
                    {
                        zero[i, j] = true;
                    }
                    else
                    {
                        zero[i, j] = false;
                    }
                }
            }

            return zero;
        }
    }
}
