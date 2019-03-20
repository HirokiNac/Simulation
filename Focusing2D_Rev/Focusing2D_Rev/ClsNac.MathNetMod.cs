using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace ClsNac.MathNetMod
{
    class Spline2D
    {
        int nx;
        int ny;
        double[,] z;
        double[] x;
        double[] y;

        public Spline2D(double[,] _z)
        {
            nx = _z.GetLength(0);
            ny = _z.GetLength(1);
            x = new double[nx];
            y = new double[ny];

            for(int i=0;i< nx;i++)
            {
                x[i] = i;
            }
            for (int j = 0; j < ny; j++)
            {
                y[j] = j;
            }
            z = _z;

        }

        public Spline2D(double[,] _z,double[] _x,double[] _y)
        {
            //要素数が一致するか確認
            if (_z.GetLength(0) != _x.Length) { }
            if (_z.GetLength(1) != _y.Length) { }

            nx = _z.GetLength(0);
            ny = _z.GetLength(1);
            x = _x;
            y = _y;
            z = _z;
        }

        

        public double[,] Interpolation(double[] x_mod,double[] y_mod)
        {
            //if (x_mod.GetLength(0) == y_mod.GetLength(0))
            //{

            //}
            //else if (x_mod.GetLength(1) == y_mod.GetLength(1))
            //{

            //}

            int nx_mod = x_mod.Length;
            int ny_mod = y_mod.Length;
            

            //z[変換後x,変換前y]を作る
            double[][] z_subMod = new double[nx_mod][];
            for (int ii = 0; ii < nx_mod; ii++)
                z_subMod[ii] = new double[ny];


            Parallel.For(0, ny, j =>
            {
                //x方向の一次元に変換
                double[] z1 = new double[nx];
                for (int i = 0; i < nx; i++)
                    z1[i] = z[i, j];
                //x方向に補完
                for (int ii = 0; ii < nx_mod; ii++)
                    z_subMod[ii][j] = Interpolate.Linear(x, z1).Interpolate(x_mod[ii]);
#if CUBIC
                    z_subMod[ii][j] = Interpolate.CubicSpline(x, z1).Interpolate(x_mod[ii]);
#endif
            });

            //[変換後x,変換後y]
            double[,] z_mod = new double[nx_mod, ny_mod];
            Parallel.For(0, ny_mod, jj =>
            {
                for (int ii = 0; ii < nx_mod; ii++)
                    z_mod[ii, jj] = Interpolate.Linear(y, z_subMod[ii]).Interpolate(y_mod[jj]);
#if CUBIC
                    z_mod[ii, jj] = Interpolate.CubicSpline(y, z_subMod[ii]).Interpolate(y_mod[jj]);
#endif
            });

            return z_mod;
        }

        //public double[,] Interpolation(int x_mag,int y_mag)
        //{
        //    if (0 <= x_mag || 0 <= y_mag)
        //    { }

        //    double[] x_mod = new double[x_mag * nx + 1];
        //    double[] y_mod = new double[y_mag * ny + 1];

        //    for (int i = 0; i < x_mag * nx + 1; i++)
        //        x_mod[i] = (double)i / x_mag;
        //    for (int j = 0; j < y_mag * ny + 1; j++)
        //        y_mod[j] = (double)j / y_mag;

        //    return Interpolation(x_mod, y_mod);
        //}

        public double[,] Interpolation(int nx2,int ny2)
        {
            double[] x_mod = new double[nx2];
            double[] y_mod = new double[ny2];
            for (int i = 0; i < nx2; i++)
                x_mod[i] = (double)i * nx /( nx2-1.0);

            for (int i = 0; i < ny2; i++)
                y_mod[i] = (double)i * ny / (ny2-1.0);

            return Interpolation(x_mod, y_mod);
        }
    }
}
