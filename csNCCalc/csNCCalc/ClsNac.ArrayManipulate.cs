using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;

namespace ClsNac
{
    /// <summary>
    /// 2016/06/02更新
    /// 逆行列
    /// かけ算
    /// </summary>
    class ArrayManipulate
    {
        public enum Mode { Plus, Minus, Replace, Average }

        public static void Replace(double[,] Source, ref double[,] Destination, Mode mode = Mode.Replace, int xStart = 0, int yStart = 0)
        {
            int xSource = Source.GetLength(0);
            int ySource = Source.GetLength(1);

            if (xSource + xStart > Destination.GetLength(0))
                return;
            if (ySource + yStart > Destination.GetLength(1))
                return;

            for (int i = 0; i < xSource; i++)
            {
                for (int j = 0; j < ySource; j++)
                {
                    switch (mode)
                    {
                        case Mode.Replace:
                            Destination[i + xStart, j + yStart] = Source[i, j];
                            break;
                        case Mode.Plus:
                            Destination[i + xStart, j + yStart] += Source[i, j];
                            break;
                        case Mode.Minus:
                            Destination[i + xStart, j + yStart] -= Source[i, j];
                            break;
                        case Mode.Average:
                            Destination[i + xStart, j + yStart] = (Destination[i + xStart, j + yStart] + Source[i, j]) / 2.0;
                            break;
                    }
                }
            }
        }

        #region 抜出し
        /// <summary>
        /// 抜き出し
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static double[,] ExtractAND(double[,] data, Rectangle rect)
        {

            double[,] output = new double[rect.Width, rect.Height];
            for (int i = rect.Left; i < rect.Right; i++)
            {
                for (int j = rect.Top; j < rect.Bottom; j++)
                {
                    output[i - rect.Left, j - rect.Top] = data[i, j];
                }
            }

            return output;
        }

        /// <summary>
        /// 抜き出し 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="rect"></param>
        /// <param name="zero">0の部分があるときtrue</param>
        /// <returns></returns>
        public static double[,] ExtractAND(double[,] data, Rectangle rect, out bool zero)
        {
            zero = false;

            double[,] output = new double[rect.Width, rect.Height];
            for (int i = rect.Left; i < rect.Right; i++)
            {
                for (int j = rect.Top; j < rect.Bottom; j++)
                {
                    output[i - rect.Left, j - rect.Top] = data[i, j];
                    zero |= data[i, j] == 0.0;
                }
            }

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pitch">元形状のピッチ/新形状のピッチ</param>
        /// <param name="rect"></param>
        /// <returns></returns>
        //public static double[,] ExtractAND(double[,] data, double pitch, Rectangle rect)
        //{
        //    double[,] output = new double[rect.Width, rect.Height];

        //    //pitchでスプライン
        //    int nx = data.GetLength(0);
        //    int ny = data.GetLength(1);
        //    double[][] data_jag = new double[nx][];
        //    for (int i = 0; i < nx; i++)
        //    {
        //        data_jag[i] = new double[ny];
        //        for (int j = 0; j < ny; j++)
        //        {
        //            data_jag[i][j] = data[i, j];
        //        }
        //    }

        //    double[] posx = new double[nx];
        //    for (int i = 0; i < nx; i++)
        //    {
        //        posx[i] = i;
        //    }
        //    double[] posy = new double[ny];
        //    for (int j = 0; j < ny; j++)
        //    {
        //        posy[j] = j;
        //    }

        //    //y方向のスプライン補間
        //    double[][] data2_jspline = new double[rect.Height][];
        //    for (int j = 0; j < rect.Height; j++)
        //    {
        //        data2_jspline[j] = new double[nx];
        //    }

        //    Parallel.For(0, nx, i =>
        //    //for (int i = 0; i < nx; i++)
        //    {
        //        var spline = MathNet.Numerics.Interpolate.CubicSpline(posy, data_jag[i]);
        //        for (int j = 0; j < rect.Height; j++)
        //        {
        //            data2_jspline[j][i] = spline.Interpolate((j + rect.Top) * pitch);
        //        }
        //    }
        //    );

        //    Parallel.For(0, rect.Height, j =>
        //    //for (int j = 0; j < rect.Height; j++)
        //    {
        //        var spline = MathNet.Numerics.Interpolate.CubicSpline(posx, data2_jspline[j]);
        //        for (int i = 0; i < rect.Width; i++)
        //        {
        //            output[i, j] = spline.Interpolate((i + rect.Left) * pitch);
        //        }
        //    }
        //    );
        //    return output;
        //}

        public static double[,] ExtractNAND(double[,] _data,Rectangle _rect)
        {
            double[,] output = new double[_data.GetLength(0), _data.GetLength(1) - _rect.Height];
            for(int i=0;i<_data.GetLength(0);i++)
            {
                for (int j = 0; j < _rect.Top; j++)
                {
                    output[i, j] = _data[i, j];
                }
                for(int j=_rect.Bottom;j<_data.GetLength(1);j++)
                {
                    output[i, j - _rect.Height] = _data[i, j];
                }
            }
            return output;
        }

        #endregion

        /// <summary>
        /// 差分 data1-data2
        /// </summary>
        /// <param name="data1"></param>
        /// <param name="data2"></param>
        /// <returns></returns>
        public static double[,] Subtract(double[,] data1, double[,] data2)
        {
            if (data1.GetLength(0) != data2.GetLength(0)) return null;
            if (data1.GetLength(1) != data2.GetLength(1)) return null;

            int nx = data1.GetLength(0);
            int ny = data1.GetLength(1);

            double[,] data0 = new double[nx, ny];

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    data0[i, j] = data1[i, j] - data2[i, j];
                }
            }
            return data0;
        }

        public static double[,] Subtract(double[,] data1, double data2)
        {

            int nx = data1.GetLength(0);
            int ny = data1.GetLength(1);

            double[,] data0 = new double[nx, ny];

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    data0[i, j] = data1[i, j] - data2;
                }
            }
            return data0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data1">array</param>
        /// <param name="data2">line</param>
        /// <returns></returns>
        public static double[,] SubtractLine(double[,] data1, double[,] data2)
        {
            //if (data1.GetLength(0) != data2.GetLength(0)) return null;
            if (data1.GetLength(1) != data2.GetLength(1)) return null;

            int nx = data1.GetLength(0);
            int ny = data1.GetLength(1);

            double[,] data0 = new double[nx, ny];

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    data0[i, j] = data1[i, j] - data2[0, j];
                }
            }
            return data0;
        }


        public static double[,] Add(double[,] data1, double[,] data2)
        {
            if (data1.GetLength(0) != data2.GetLength(0)) return null;
            if (data1.GetLength(1) != data2.GetLength(1)) return null;

            int nx = data1.GetLength(0);
            int ny = data1.GetLength(1);

            double[,] data0 = new double[nx, ny];

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    data0[i, j] = data1[i, j] + data2[i, j];
                }
            }
            return data0;

        }

        public static double[,] Add(double[,] data1, double data2)
        {
            int nx = data1.GetLength(0);
            int ny = data1.GetLength(1);

            double[,] data0 = new double[nx, ny];

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    data0[i, j] = data1[i, j] + data2;
                }
            }
            return data0;
        }

        public static double[,] AddLine(double[,] data1, double[,] data2)
        {
            //if (data1.GetLength(0) != data2.GetLength(0)) return null;
            if (data1.GetLength(1) != data2.GetLength(1)) return null;

            int nx = data1.GetLength(0);
            int ny = data1.GetLength(1);

            double[,] data0 = new double[nx, ny];

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    data0[i, j] = data1[i, j] + data2[0, j];
                }
            }
            return data0;

        }

        /// <summary>
        /// 平均
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double[,] Average(double[][,] data)
        {
            int nMax = data.Length;
            int iMax = data[0].GetLength(0);
            int jMax = data[0].GetLength(1);
            double[,] average = new double[iMax, jMax];

            for (int i = 0; i < iMax; i++)
            {
                for (int j = 0; j < jMax; j++)
                {
                    for (int n = 0; n < nMax; n++)
                    {
                        average[i, j] += data[n][i, j];
                    }
                    average[i, j] /= (double)nMax;
                }
            }
            return average;
        }

        public static double Average(double[,] data)
        {
            int iMax = data.GetLength(0);
            int jMax = data.GetLength(1);
            double average = 0.0;
            for(int i=0;i<iMax;i++)
            {
                for(int j=0;j<jMax;j++)
                {
                    average += data[i, j];
                }
            }
            return average / data.Length;
        }

        /// <summary>
        /// 列の上下を入れ替え
        /// </summary>
        /// <param name="_data">入力</param>
        /// <returns></returns>
        public static double[,] UpSideDown(double[,] _data)
        {
            int nx = _data.GetLength(0);
            int ny = _data.GetLength(1);
            double[,] data = new double[nx, ny];
            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    data[i, j] = _data[i, nx - j - 1];
                }
            }
            return data;
        }

        public static double[,] Inverse(double[,] _data)
        {
            //正方行列かどうか
            if (_data.GetLength(0) != _data.GetLength(1))
            { return null; }

            int n = _data.GetLength(0);
            double[,] a = new double[n, n];
            Array.Copy(_data, a, n * n);

            //出力行列
            double[,] inv_a = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                inv_a[i, i] = 1.0;
            }

            Parallel.For(0, n, new ParallelOptions { MaxDegreeOfParallelism = 8 }, k =>
            //for(int k=0;k<n;k++)
            {
                double buf1 = 1.0 / a[k, k];
                for (int j = n - 1; j >= 0; j--)
                {
                    inv_a[k, j] *= buf1;
                    a[k, j] *= buf1;
                }
                for (int i = 0; i < n; i++)
                {
                    if (i != k)
                    {
                        double buf2 = a[i, k];
                        for (int j = n - 1; j >= 0; j--)
                        {
                            inv_a[i, j] -= buf2 * inv_a[k, j];
                            a[i, j] -= buf2 * a[k, j];
                        }
                    }
                }
            }
            );
            return inv_a;
        }

        public static Complex[,] Inverse(Complex[,] _data)
        {
            //正方行列かどうか
            if (_data.GetLength(0) != _data.GetLength(1))
            { return null; }

            int n = _data.GetLength(0);
            Complex[,] a = new Complex[n, n];
            Array.Copy(_data, a, n * n);

            //出力行列
            Complex[,] inv_a = new Complex[n, n];
            for (int i = 0; i < n; i++)
            {
                inv_a[i, i] = 1.0;
            }
            //Parallel.For(0, n, new ParallelOptions { MaxDegreeOfParallelism = 8 }, k =>
            for (int k = 0; k < n; k++)
            {
                Complex buf1 = 1.0 / a[k, k];
                for (int j = n - 1; j >= 0; j--)
                {
                    inv_a[k, j] *= buf1;
                    a[k, j] *= buf1;
                }
                for (int i = 0; i < n; i++)
                {
                    if (i != k)
                    {
                        Complex buf2 = a[i, k];
                        for (int j = n - 1; j >= 0; j--)
                        {
                            inv_a[i, j] -= buf2 * inv_a[k, j];
                            a[i, j] -= buf2 * a[k, j];
                        }
                    }
                }
            }
            //);

            return inv_a;
        }

        public static double[,] Multiply(double[,] _data1, double[,] _data2)
        {
            if (_data1.GetLength(1) != _data2.GetLength(0))
            { return null; }

            int nx = _data1.GetLength(0);
            int ny = _data2.GetLength(1);
            int nc = _data1.GetLength(1);

            double[,] data = new double[nx, ny];

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    for (int k = 0; k < nc; k++)
                    {
                        data[i, j] += _data1[i, k] * _data2[k, j];
                    }
                }
            }

            return data;
        }

        public static Complex[,] Multiply(Complex[,] _data1, Complex[,] _data2)
        {
            if (_data1.GetLength(1) != _data2.GetLength(0))
            { return null; }

            int nx = _data1.GetLength(0);
            int ny = _data2.GetLength(1);
            int nc = _data1.GetLength(1);

            Complex[,] data = new Complex[nx, ny];

            for (int i = 0; i < nx; i++)
            {
                for (int j = 0; j < ny; j++)
                {
                    for (int k = 0; k < nc; k++)
                    {
                        data[i, j] += _data1[i, k] * _data2[k, j];
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_data">元データ</param>
        /// <param name="_theta">角度[rad]</param>
        /// <param name="_extxc">抜き出しX中心</param>
        /// <param name="_extyc">抜き出しY中心</param>
        /// <param name="_extx1">抜き出しX開始</param>
        /// <param name="_extx2">抜き出しX終了</param>
        /// <param name="_exty1">抜き出しY開始</param>
        /// <param name="_exty2">抜き出しY終了</param>
        /// <returns></returns>
        //public static double[,] Rotation(double[,] _data, double _theta, int _extxc, int _extyc, int _extx1, int _extx2, int _exty1, int _exty2)
        //{
        //    int nx0 = _data.GetLength(0);
        //    int ny0 = _data.GetLength(1);

        //    //回転用の座標系つくる
        //    int nx_ext = _extx2 - _extx1;
        //    int ny_ext = _exty2 - _exty1;
        //    double[,] outDataX = new double[nx_ext, ny_ext];
        //    double[,] outDataY = new double[nx_ext, ny_ext];

        //    for (int i = 0; i < nx_ext; i++)
        //    {
        //        for (int j = 0; j < ny_ext; j++)
        //        {
        //            outDataX[i, j] = _extx1 + i;
        //            outDataY[i, j] = _exty1 + j;
        //        }
        //    }
        //    //座標系を回転
        //    ClsNac.Rotation.rotXY(ref outDataX, ref outDataY, _theta, _extxc, _extyc);

        //    FileIO.FileIO.writeFile(System.Windows.Forms.Application.StartupPath+"\\outx.txt", outDataX);
        //    FileIO.FileIO.writeFile(System.Windows.Forms.Application.StartupPath + "\\outy.txt", outDataY);

        //    ClsNac.Spline spline = new Spline(_data, 0);

        //    return spline.Execute(outDataX, outDataY);
        //}

        public static double Max(double[,] _data)
        {
            double max = double.MinValue;
            for(int i=0;i<_data.GetLength(0);i++)
            {
                for (int j = 0; j < _data.GetLength(1); j++)
                {
                    if (max < _data[i, j]) max = _data[i, j];
                }
            }
            return max;
        }

        public static bool IncludeMinus(double[,] _data)
        {
            for (int i = 0; i < _data.GetLength(0); i++)
            {
                for (int j = 0; j < _data.GetLength(1); j++)
                {
                    if (_data[i, j] < 0) return true;
                }
            }
            return false;
        }

        public static double Sum(double[,] _data)
        {
            double sum = 0.0;
            for (int i = 0; i < _data.GetLength(0); i++)
            {
                for (int j = 0; j < _data.GetLength(1); j++)
                {
                    sum += _data[i, j];
                }
            }
            return sum;

        }
    }



}
