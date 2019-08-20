using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Math;

namespace ClsNac 
{
    static class Rotation
    {
        #region 回転ルーチン

        /// <summary>
        /// 座標の回転1
        /// </summary>
        /// <param name="x">回転したい座標x</param>
        /// <param name="y">回転したい座標y</param>
        /// <param name="theta">回転角度</param>
        /// <param name="x0">回転中心x座標</param>
        /// <param name="y0">回転中心y座標</param>
        public static void rotXY(ref double x, ref double y, double theta, double x0, double y0)
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
        public static void rotXY(ref double[] x, ref double[] y, double theta, double x0, double y0)
        {
            for (int i = 0; i < x.Length; i++)
                rotXY(ref x[i], ref y[i], theta, x0, y0);
        }

        /// <summary>
        /// 座標の回転3
        /// </summary>
        /// <param name="x">回転したい座標[x,y]</param>
        /// <param name="theta">回転角度</param>
        /// <param name="x0">回転中心x座標</param>
        /// <param name="y0">回転中心y座標</param>
        public static void rotXY(ref double[,] x, ref double[,] y, double theta, double x0, double y0)
        {
            for (int i = 0; i < x.GetLength(0); i++)
                for (int j = 0; j < x.GetLength(1); j++)
                    rotXY(ref x[i, j], ref y[i, j], theta, x0, y0);

        }

        /// <summary>
        /// a+bx+cy
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        public static void rotXYZ(ref double[,] x, ref double[,] y, ref double[,] z, double a, double b, double c)
        {
            int n1 = x.GetLength(0);
            int n2 = y.GetLength(1);
            for(int i=0;i<n1;i++)
            {
                for(int j=0;j<n2;j++)
                {
                    rotXYZ(ref x[i, j], ref y[i, j], ref z[i, j], a, b, c);
                }
            }
        }

        public static void rotXYZ(ref double x, ref double y, ref double z, double a, double b, double c)
        {
            double x_buf = x;
            double y_buf = y;
            double z_buf = z;
            x = x_buf * Cos(b) - z_buf * Sin(b);
            y = y_buf * Cos(c) - (x_buf * Sin(b) + z_buf * Cos(b)) * Sin(c);
            z = y_buf * Sin(c) + (x_buf * Sin(b) + z_buf * Cos(b)) * Cos(c) + a;
        }
        
        #endregion


    }
}
