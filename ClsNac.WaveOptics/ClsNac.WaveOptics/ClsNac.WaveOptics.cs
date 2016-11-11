using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ClsNac.WaveOptics
{
    public class WaveField1D
    {
        /// <summary>
        /// 分割数
        /// </summary>
        public int div { get; private set; }
        public double[] x;
        public double[] y;

        /// <summary>
        /// 波長
        /// </summary>
        public double lambda { get; set; }
        public Complex[] u;
        public double[] re;
        public double[] im;

        public WaveField1D(int _div)
        {
            div = _div;

            x = new double[div];
            y = new double[div];
            u = new Complex[div];
            re = new double[div];
            im = new double[div];
        }

        public double[] Intensity()
        {
            double[] _intens = new double[div];
            for (int i = 0; i < div; i++)
            {
                _intens[i] = Math.Pow(u[i].Magnitude, 2.0);
            }
            return _intens;
        }

        public double[] IntensityN()
        {
            double[] _intens = Intensity();
            double[] _intensN = new double[div];
            double max = double.MinValue;
            for (int i = 0; i < div; i++)
            {
                max = Math.Max(max, _intens[i]);
            }
            for (int i = 0; i < div; i++)
            {
                _intensN[i] = _intens[i] / max;
            }
            return _intensN;
        }

        public double[] Phase()
        {
            double[] _phase = new double[div];
            for (int i = 0; i < div; i++)
            {
                _phase[i] = u[i].Phase;
            }
            return _phase;
        }

    }

    public class FresnelKirchhoff1D
    {
        int n_wfs;
        public WaveField1D[] wfs;
        public enum MODE { CPU, CUDA, AF }
        public enum DIRECTION : int { FORWARD = -1, BACKWARD = 1 }

        public FresnelKirchhoff1D(ref WaveField1D[] _wfs)
        {
            wfs = _wfs;
            n_wfs = wfs.Length;
        }

        public static void Propagation(WaveField1D _wf1, ref WaveField1D _wf2, MODE _MODE = MODE.CPU, DIRECTION _DIRECTION = DIRECTION.FORWARD)
        {
            switch(_MODE)
            {
                case MODE.CPU:
                    ClsNac.WaveOpticsCLR.Prop1D(
                        _wf1.lambda, (int)_DIRECTION,
                        _wf1.x, _wf1.y, _wf1.u,
                        _wf2.x, _wf2.y, _wf2.u);
                    break;
                case MODE.CUDA:
                    PropFw1dCuda(_wf1, ref _wf2);
                    break;
            }
        }

        public void Execute(int _start = 0, MODE _MODE = MODE.CPU,DIRECTION _DIRECTION=DIRECTION.FORWARD)
        {
            for (int n = _start; n < n_wfs - 1; n++)
            {
                Propagation(wfs[n], ref wfs[n + 1], _MODE, _DIRECTION);
            }

        }


        #region PropFw1DAf
        public static void PropFw1DAf(WaveField1D wfs1,ref WaveField1D wfs2)
        {
            IntPtr ptr_u2re = IntPtr.Zero;
            IntPtr ptr_u2im = IntPtr.Zero;

            PropFw1DAf(wfs1.lambda, 
                wfs1.div, wfs1.x, wfs1.y, wfs1.re, wfs1.im,
                wfs2.div, wfs2.x, wfs2.y, ref ptr_u2re, ref ptr_u2im);

            Marshal.Copy(ptr_u2re, wfs2.re, 0, wfs2.div);
            Marshal.Copy(ptr_u2im, wfs2.im, 0, wfs2.div);

            for (int i = 0; i < wfs2.div;i++)
            {
                wfs2.u[i] = new Complex(wfs2.re[i], wfs2.im[i]);
            }


            Marshal.FreeHGlobal(ptr_u2re);
            Marshal.FreeHGlobal(ptr_u2im);
        }

        public static void PropFw1DAf(double _lambda,
            int _n1,double[] _x1,double[] _y1,double[] _u1re,double[] _u1im,
            int _n2,double[] _x2,double[] _y2,ref double[] _u2re,ref double[] _u2im)
        {
            IntPtr ptr_u2re = IntPtr.Zero;
            IntPtr ptr_u2im = IntPtr.Zero;

            PropFw1DAf(_lambda,
                _n1, _x1, _y1, _u1re, _u1im,
                _n2, _x2, _y2, ref ptr_u2re, ref ptr_u2im);

            Marshal.Copy(ptr_u2re, _u2re, 0, _n2);
            Marshal.Copy(ptr_u2im, _u2im, 0, _n2);

            Marshal.FreeHGlobal(ptr_u2re);
            Marshal.FreeHGlobal(ptr_u2im); 
        }

        [DllImport("ClsNac.WaveOpticsAF.dll",EntryPoint ="PropFw1D", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PropFw1DAf(
            double _lambda,
            int _n1, [Out]double[] _x1, [Out]double[] _y1, [Out]double[] _u1re, [Out]double[] _u1im,
            int _n2, [Out]double[] _x2, [Out]double[] _y2, ref IntPtr _u2re, ref IntPtr _u2im);
        #endregion


        #region PropFw1dCuda
        public static void PropFw1dCuda(WaveField1D wfs1, ref WaveField1D wfs2)
        {
            IntPtr ptr_u2re =new IntPtr();
            ptr_u2re = Marshal.AllocHGlobal(wfs2.div * sizeof(double));
            IntPtr ptr_u2im = new IntPtr();
            ptr_u2im = Marshal.AllocHGlobal(wfs2.div * sizeof(double));

            PropFw1dCuda(wfs1.lambda,
                wfs1.div, wfs1.x, wfs1.y, wfs1.re, wfs1.im,
                wfs2.div, wfs2.x, wfs2.y, ref ptr_u2re, ref ptr_u2im);

            Marshal.Copy(ptr_u2re, wfs2.re, 0, wfs2.div);
            Marshal.Copy(ptr_u2im, wfs2.im, 0, wfs2.div);
            Marshal.FreeHGlobal(ptr_u2re);
            Marshal.FreeHGlobal(ptr_u2im);

            for (int i = 0; i < wfs2.div; i++)
            {
                wfs2.u[i] = new Complex(wfs2.re[i], wfs2.im[i]);
            }


        }

        public static void PropFw1dCuda(double _lambda,
            int _n1, double[] _x1, double[] _y1, double[] _u1re, double[] _u1im,
            int _n2, double[] _x2, double[] _y2, ref double[] _u2re, ref double[] _u2im)
        {
            IntPtr ptr_u2re = new IntPtr();
            ptr_u2re = Marshal.AllocHGlobal(_n2 * sizeof(double));
            IntPtr ptr_u2im = new IntPtr();
            ptr_u2im = Marshal.AllocHGlobal(_n2 * sizeof(double));

            PropFw1dCuda(_lambda,
                _n1, _x1, _y1, _u1re, _u1im,
                _n2, _x2, _y2, ref ptr_u2re, ref ptr_u2im);

            Marshal.Copy(ptr_u2re, _u2re, 0, _n2);
            Marshal.Copy(ptr_u2im, _u2im, 0, _n2);

            Marshal.FreeHGlobal(ptr_u2re);
            Marshal.FreeHGlobal(ptr_u2im);
        }

        [DllImport("ClsNac.WaveOpticsCUDA.dll", EntryPoint = "PropFw1d", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PropFw1dCuda(
            double _lambda,
            int _n1, [Out]double[] _x1, [Out]double[] _y1, [Out]double[] _u1re, [Out]double[] _u1im,
            int _n2, [Out]double[] _x2, [Out]double[] _y2, ref IntPtr _u2re, ref IntPtr _u2im);

        [DllImport("ClsNac.WaveOpticsCUDA.dll", EntryPoint = "PropFw1d_f", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PropFw1dCuda_f(
            double _lambda,
            int _n1, [Out]float[] _x1, [Out]float[] _y1, [Out]float[] _u1re, [Out]float[] _u1im,
            int _n2, [Out]float[] _x2, [Out]float[] _y2, ref IntPtr _u2re, ref IntPtr _u2im);


        #endregion
    }

    public class WaveField2D
    {
        public int div1 { get; private set; }
        public int div2 { get; private set; }

        /// <summary>
        /// 分割数
        /// </summary>
        public int div { get { return div1 * div2; } }
        public double[] x;
        public double[] y;
        public double[] z;

        /// <summary>
        /// 波長
        /// </summary>
        public double lambda { get; set; }
        public Complex[] u;
        public double[] re;
        public double[] im;

        public WaveField2D(int _div1,int _div2)
        {
            div1 = _div1;
            div2 = _div2;

            x = new double[div];
            y = new double[div];
            z = new double[div];
            u = new Complex[div];
            re = new double[div];
            im = new double[div];
        }

        public void CpyComplexToReIm()
        {
            for(int i=0;i<div;i++)
            {
                re[i] = u[i].Real;
                im[i] = u[i].Imaginary;
            }
        }

        public void CpyReImToComplex()
        {
            for(int i=0;i<div;i++)
            {
                u[i] = new Complex(re[i], im[i]);
            }
        }

        public double[,] Intensity()
        {
            double[,] _intens = new double[div1, div2];
            for (int i = 0; i < div1; i++)
            {
                for (int j = 0; j < div2; j++)
                {
                    _intens[i, j] = Math.Pow(u[i * div2 + j].Magnitude, 2.0);
                }
            }
            return _intens;
        }

        public double[,] IntensityN()
        {
            double[,] _intens = Intensity();
            double[,] _intensN = new double[div1, div2];
            double max = double.MinValue;
            for (int i = 0; i < div1; i++)
            {
                for (int j = 0; j < div2; j++)
                {
                    max = Math.Max(max, _intens[i, j]);
                }
            }
            for (int i = 0; i < div1; i++)
            {
                for (int j = 0; j < div2; j++)
                {
                    _intensN[i, j] = _intens[i, j] / max;
                }
            }
            return _intensN;
        }

        public double[] Phase()
        {
            double[] _phase = new double[div];
            for (int i = 0; i < div; i++)
            {
                _phase[i] = u[i].Phase;
            }
            return _phase;
        }

    }

    public class WaveField2Df
    {
        public int div1 { get; private set; }
        public int div2 { get; private set; }

        /// <summary>
        /// 分割数
        /// </summary>
        public int div { get { return div1 * div2; } }
        public float[] x;
        public float[] y;
        public float[] z;

        /// <summary>
        /// 波長
        /// </summary>
        public float lambda { get; set; }
        public Complex[] u;
        public float[] re;
        public float[] im;

        public WaveField2Df(int _div1, int _div2)
        {
            div1 = _div1;
            div2 = _div2;

            x = new float[div];
            y = new float[div];
            z = new float[div];
            u = new Complex[div];
            re = new float[div];
            im = new float[div];
        }

        public void CpyComplexToReIm()
        {
            for (int i = 0; i < div; i++)
            {
                re[i] = (float)u[i].Real;
                im[i] = (float)u[i].Imaginary;
            }
        }

        public void CpyReImToComplex()
        {
            for (int i = 0; i < div; i++)
            {
                u[i] = new Complex(re[i], im[i]);
            }
        }

        public double[,] Intensity()
        {
            double[,] _intens = new double[div1, div2];
            for (int i = 0; i < div1; i++)
            {
                for (int j = 0; j < div2; j++)
                {
                    _intens[i, j] = Math.Pow(u[i * div2 + j].Magnitude, 2.0);
                }
            }
            return _intens;
        }

        public double[,] IntensityN()
        {
            double[,] _intens = Intensity();
            double[,] _intensN = new double[div1, div2];
            double max = double.MinValue;
            for (int i = 0; i < div1; i++)
            {
                for (int j = 0; j < div2; j++)
                {
                    max = Math.Max(max, _intens[i, j]);
                }
            }
            for (int i = 0; i < div1; i++)
            {
                for (int j = 0; j < div2; j++)
                {
                    _intensN[i, j] = _intens[i, j] / max;
                }
            }
            return _intensN;
        }

        public double[] Phase()
        {
            double[] _phase = new double[div];
            for (int i = 0; i < div; i++)
            {
                _phase[i] = u[i].Phase;
            }
            return _phase;
        }

    }

    public class FresnelKirchhoff2D
    {
        int n_wfs;
        public WaveField2D[] wfs;
        public enum MODE { CPU, CUDA, AF }
        public enum DIRECTION : int { FORWARD = -1, BACKWARD = 1 }

        public FresnelKirchhoff2D(ref WaveField2D[] _wfs)
        {
            wfs = _wfs;
            n_wfs = wfs.Length;
        }

        public static void Propagation(WaveField2D _wf1, ref WaveField2D _wf2, MODE _MODE = MODE.CPU, DIRECTION _DIRECTION = DIRECTION.FORWARD)
        {
            switch (_MODE)
            {
                case MODE.CPU:
                    ClsNac.WaveOpticsCLR.Prop1D(
                        _wf1.lambda, (int)_DIRECTION,
                        _wf1.x, _wf1.y, _wf1.u,
                        _wf2.x, _wf2.y, _wf2.u);
                    break;
                case MODE.CUDA:
                    PropFw2dCuda(_wf1, ref _wf2);
                    break;
            }
        }

        public void Execute(int _start = 0, MODE _MODE = MODE.CPU, DIRECTION _DIRECTION = DIRECTION.FORWARD)
        {
            for (int n = _start; n < n_wfs - 1; n++)
            {
                Propagation(wfs[n], ref wfs[n + 1], _MODE, _DIRECTION);
            }

        }


        #region PropFw2dCuda

        public static void PropFw2dCLR(WaveField2D wfs1, ref WaveField2D wfs2)
        {
            ClsNac.WaveOpticsCLR.Prop2D(
                wfs1.lambda,
                wfs1.x, wfs1.y, wfs1.z, wfs1.u,
                wfs2.x, wfs2.y, wfs2.z, wfs2.u);


        }
 
        public static void PropFw2dCuda(WaveField2D wfs1, ref WaveField2D wfs2)
        {
            IntPtr ptr_u2re = new IntPtr();
            ptr_u2re = Marshal.AllocHGlobal(wfs2.div * sizeof(double));
            IntPtr ptr_u2im = new IntPtr();
            ptr_u2im = Marshal.AllocHGlobal(wfs2.div * sizeof(double));

            PropFw2dCuda(wfs1.lambda,
                wfs1.div, wfs1.x, wfs1.y, wfs1.z, wfs1.re, wfs1.im,
                wfs2.div, wfs2.x, wfs2.y, wfs2.z, ref ptr_u2re, ref ptr_u2im);

            Marshal.Copy(ptr_u2re, wfs2.re, 0, wfs2.div);
            Marshal.Copy(ptr_u2im, wfs2.im, 0, wfs2.div);
            Marshal.FreeHGlobal(ptr_u2re);
            Marshal.FreeHGlobal(ptr_u2im);

            for (int i = 0; i < wfs2.div; i++)
            {
                wfs2.u[i] = new Complex(wfs2.re[i], wfs2.im[i]);
            }


        }

        public static void PropFw2dCuda2(WaveField2D wfs1, ref WaveField2D wfs2)
        {
            IntPtr ptr_u2re = new IntPtr();
            ptr_u2re = Marshal.AllocHGlobal(wfs2.div * sizeof(double));
            IntPtr ptr_u2im = new IntPtr();
            ptr_u2im = Marshal.AllocHGlobal(wfs2.div * sizeof(double));

            PropFw2dCuda2(wfs1.lambda,
                wfs1.div1, wfs1.div2, wfs1.x, wfs1.y, wfs1.z, wfs1.re, wfs1.im,
                wfs2.div1, wfs2.div2, wfs2.x, wfs2.y, wfs2.z, ref ptr_u2re, ref ptr_u2im);

            Marshal.Copy(ptr_u2re, wfs2.re, 0, wfs2.div);
            Marshal.Copy(ptr_u2im, wfs2.im, 0, wfs2.div);
            Marshal.FreeHGlobal(ptr_u2re);
            Marshal.FreeHGlobal(ptr_u2im);

            for (int i = 0; i < wfs2.div; i++)
            {
                wfs2.u[i] = new Complex(wfs2.re[i], wfs2.im[i]);
            }


        }

        public static void PropFw2dCuda(double _lambda,
            int _n1, double[] _x1, double[] _y1,double[] _z1, double[] _u1re, double[] _u1im,
            int _n2, double[] _x2, double[] _y2,double[] _z2, ref double[] _u2re, ref double[] _u2im)
        {
            IntPtr ptr_u2re = new IntPtr();
            ptr_u2re = Marshal.AllocHGlobal(_n2 * sizeof(double));
            IntPtr ptr_u2im = new IntPtr();
            ptr_u2im = Marshal.AllocHGlobal(_n2 * sizeof(double));

            PropFw2dCuda(_lambda,
                _n1, _x1, _y1, _z1, _u1re, _u1im,
                _n2, _x2, _y2, _z2, ref ptr_u2re, ref ptr_u2im);

            Marshal.Copy(ptr_u2re, _u2re, 0, _n2);
            Marshal.Copy(ptr_u2im, _u2im, 0, _n2);

            Marshal.FreeHGlobal(ptr_u2re);
            Marshal.FreeHGlobal(ptr_u2im);
        }

        [DllImport("ClsNac.WaveOpticsCUDA.dll", EntryPoint = "PropFw2d", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PropFw2dCuda(
            double _lambda,
            int _n1, [Out]double[] _x1, [Out]double[] _y1, [Out]double[] _z1, [Out]double[] _u1re, [Out]double[] _u1im,
             int _n2, [Out]double[] _x2, [Out]double[] _y2, [Out]double[] _z2, ref IntPtr _u2re, ref IntPtr _u2im);

        [DllImport("ClsNac.WaveOpticsCUDA.dll", EntryPoint = "PropFw2d2", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PropFw2dCuda2(
            double _lambda,
            int _m1, int _n1, [Out]double[] _x1, [Out]double[] _y1, [Out]double[] _z1, [Out]double[] _u1re, [Out]double[] _u1im,
            int _m2, int _n2, [Out]double[] _x2, [Out]double[] _y2, [Out]double[] _z2, ref IntPtr _u2re, ref IntPtr _u2im);

        [DllImport("ClsNac.WaveOpticsCUDA.dll", EntryPoint = "PropFw1d_f", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PropFw1dCuda_f(
            double _lambda,
            int _n1, [Out]float[] _x1, [Out]float[] _y1, [Out]float[] _u1re, [Out]float[] _u1im,
            int _n2, [Out]float[] _x2, [Out]float[] _y2, ref IntPtr _u2re, ref IntPtr _u2im);


        #endregion
    }

    public class FresnelKirchhoff2Df
    {
        int n_wfs;
        public WaveField2Df[] wfs;
        public enum MODE { CPU, CUDA, AF }
        public enum DIRECTION : int { FORWARD = -1, BACKWARD = 1 }

        public FresnelKirchhoff2Df(ref WaveField2Df[] _wfs)
        {
            wfs = _wfs;
            n_wfs = wfs.Length;
        }

        public static void Propagation(WaveField2D _wf1, ref WaveField2D _wf2, MODE _MODE = MODE.CUDA, DIRECTION _DIRECTION = DIRECTION.FORWARD)
        {
            float[] x1, y1, z1, re1, im1;
            float[] x2, y2, z2, re2, im2;
            float lambda = (float)_wf1.lambda;
            int n1 = _wf1.div;
            int n2 = _wf2.div;

            double2float(_wf1.x, out x1);
            double2float(_wf1.y, out y1);
            double2float(_wf1.z, out z1);
            double2float(_wf1.re, out re1);
            double2float(_wf1.im, out im1);
            double2float(_wf2.x, out x2);
            double2float(_wf2.y, out y2);
            double2float(_wf2.z, out z2);
            double2float(_wf2.re, out re2);
            double2float(_wf2.im, out im2);
            

            switch (_MODE)
            {
                case MODE.CPU:
                    ClsNac.WaveOpticsCLR.Prop2Df(
                        lambda,
                        x1, y1, z1, re1, im1,
                        x2, y2, z2, re2, im2);
                    break;
                case MODE.CUDA:
                    PropFw2dCudaf(lambda,
                        n1, x1, y1, z1, re1, im1,
                        n2, x2, y2, z2, ref re2, ref im2);
                    break;
            }

            float2double(re2, out _wf2.re);
            float2double(im2, out _wf2.im);
        }

        static void float2double(float[] data_f,out double[] data_d)
        {
            int n = data_f.Length;
            data_d = Array.ConvertAll(data_f, elem => (double)elem);
        }

        static void double2float(double[] data_d, out float[] data_f)
        {
            int n = data_d.Length;
            data_f = Array.ConvertAll(data_d, elem => (float)elem);
        }

        #region PropFw2dCuda

        //public static void PropFw2dCLR(WaveField2Df wfs1, ref WaveField2Df wfs2)
        //{
        //    ClsNac.WaveOpticsCLR.Prop2D(
        //        wfs1.lambda,
        //        wfs1.x, wfs1.y, wfs1.z, wfs1.re, wfs2.im,
        //        wfs2.x, wfs2.y, wfs2.z, wfs2.re, wfs2.im);
        //}

        public static void PropFw2dCudaf(WaveField2Df wfs1, ref WaveField2Df wfs2)
        {
            IntPtr ptr_u2re = new IntPtr();
            ptr_u2re = Marshal.AllocHGlobal(wfs2.div * sizeof(float));
            IntPtr ptr_u2im = new IntPtr();
            ptr_u2im = Marshal.AllocHGlobal(wfs2.div * sizeof(float));

            PropFw2dCuda_f(wfs1.lambda,
                wfs1.div, wfs1.x, wfs1.y, wfs1.z, wfs1.re, wfs1.im,
                wfs2.div, wfs2.x, wfs2.y, wfs2.z, ref ptr_u2re, ref ptr_u2im);

            Marshal.Copy(ptr_u2re, wfs2.re, 0, wfs2.div);
            Marshal.Copy(ptr_u2im, wfs2.im, 0, wfs2.div);
            Marshal.FreeHGlobal(ptr_u2re);
            Marshal.FreeHGlobal(ptr_u2im);

            for (int i = 0; i < wfs2.div; i++)
            {
                wfs2.u[i] = new Complex(wfs2.re[i], wfs2.im[i]);
            }


        }
 
        public static void PropFw2dCudaf(float _lambda,
            int _n1, float[] _x1, float[] _y1, float[] _z1, float[] _u1re, float[] _u1im,
            int _n2, float[] _x2, float[] _y2, float[] _z2, ref float[] _u2re, ref float[] _u2im)
        {
            IntPtr ptr_u2re = new IntPtr();
            ptr_u2re = Marshal.AllocHGlobal(_n2 * sizeof(float));
            IntPtr ptr_u2im = new IntPtr();
            ptr_u2im = Marshal.AllocHGlobal(_n2 * sizeof(float));

            PropFw2dCuda_f(_lambda,
                _n1, _x1, _y1, _z1, _u1re, _u1im,
                _n2, _x2, _y2, _z2, ref ptr_u2re, ref ptr_u2im);

            Marshal.Copy(ptr_u2re, _u2re, 0, _n2);
            Marshal.Copy(ptr_u2im, _u2im, 0, _n2);

            Marshal.FreeHGlobal(ptr_u2re);
            Marshal.FreeHGlobal(ptr_u2im);
        }

        [DllImport("ClsNac.WaveOpticsCUDA.dll", EntryPoint = "PropFw2d_f", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PropFw2dCuda_f(
            float _lambda,
            int _n1, [Out]float[] _x1, [Out]float[] _y1, [Out]float[] _z1, [Out]float[] _u1re, [Out]float[] _u1im,
             int _n2, [Out]float[] _x2, [Out]float[] _y2, [Out]float[] _z2, ref IntPtr _u2re, ref IntPtr _u2im);

        [DllImport("ClsNac.WaveOpticsCUDA.dll", EntryPoint = "PropFw2d", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PropFw2dCuda(
            double _lambda,
            int _n1, [Out]double[] _x1, [Out]double[] _y1, [Out]double[] _z1, [Out]double[] _u1re, [Out]double[] _u1im,
             int _n2, [Out]double[] _x2, [Out]double[] _y2, [Out]double[] _z2, ref IntPtr _u2re, ref IntPtr _u2im);

        [DllImport("ClsNac.WaveOpticsCUDA.dll", EntryPoint = "PropFw2d2", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PropFw2dCuda2(
            double _lambda,
            int _m1, int _n1, [Out]double[] _x1, [Out]double[] _y1, [Out]double[] _z1, [Out]double[] _u1re, [Out]double[] _u1im,
            int _m2, int _n2, [Out]double[] _x2, [Out]double[] _y2, [Out]double[] _z2, ref IntPtr _u2re, ref IntPtr _u2im);

        [DllImport("ClsNac.WaveOpticsCUDA.dll", EntryPoint = "PropFw1d_f", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PropFw1dCuda_f(
            double _lambda,
            int _n1, [Out]float[] _x1, [Out]float[] _y1, [Out]float[] _u1re, [Out]float[] _u1im,
            int _n2, [Out]float[] _x2, [Out]float[] _y2, ref IntPtr _u2re, ref IntPtr _u2im);


        #endregion
    }




    
}
