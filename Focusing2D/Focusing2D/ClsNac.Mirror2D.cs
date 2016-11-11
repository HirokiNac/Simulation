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

namespace ClsNac.Mirror2D
{
    /*
     *  Z(upper)
     *  |/
     *--/--X(optical axis)
     * /|
     *Y(hall)
    */

    interface ICoord
    {
        double[] x { get; set; }
        double[] y { get; set; }
        double[] z { get; set; }

        Complex[] u { get; set; }
        double[] Intensity { get; set; }
        double[] Phase { get; set; }
        
        int divL { get; set; }
        int divW { get; set; }
    }

    public class Coord//:ICoord
    {
        public double[,] x2;
        public double[,] y2;
        public double[,] z2;

        public double[] x;
        public double[] y;
        public double[] z;

        public double xc { get { return x[divL / 2]; } }
        public double yc { get { return y[divL / 2]; } }
        public double zc { get { return z[divL / 2]; } }

        public int div { get { return divW * divL; } }
        public int divW { get;  set; }
        public int divL { get;  set; }

        public Complex[,] u2;
        public Complex[] u;
        public double[] real;
        public double[] imag;

        public double[,] Intensity2 { get; set; }
        public double[,] Phase2 { get; set; }

        public Coord(int _divW, int _divL)
        {
            Initialize(_divW, _divL);
        }

        public void Initialize(int _divW, int _divL)
        {
            divL = _divL;
            divW = _divW;

            x = new double[div];
            y = new double[div];
            z = new double[div];
            u = new Complex[div];
            real = new double[div];
            imag = new double[div];
        }

        public void Source()
        {

        }


        public void Focus()
        {

        }


        public void RotCoord(double t_x,double t_y,double t_z,double x0,double y0,double z0)
        {
            for (int i = 0; i < divW; i++)
            {
                for (int j = 0; j < divL; j++)
                {
                    double xx = x[i*divL+j] - x0;
                    double yy = y[i * divL + j] - y0;
                    double zz = z[i * divL + j] - z0;
                    x[i * divL + j] = xx * Math.Cos(t_x) * Math.Cos(t_y)
                        + yy * (Math.Sin(t_z) * Math.Cos(t_x) + Math.Cos(t_z) * Math.Sin(t_y) * Math.Sin(t_x))
                        + zz * (Math.Sin(t_z) * Math.Sin(t_x) - Math.Cos(t_z) * Math.Sin(t_y) * Math.Cos(t_x))
                        + x0;
                    y[i * divL + j] = -xx * Math.Sin(t_z) * Math.Cos(t_y)
                        + yy * (Math.Cos(t_z) * Math.Cos(t_x) - Math.Sin(t_z) * Math.Sin(t_y) * Math.Sin(t_x))
                        + zz * (Math.Cos(t_z) * Math.Sin(t_x) + Math.Sin(t_z) * Math.Sin(t_y) * Math.Cos(t_x))
                        + y0;
                    z[i * divL + j] = xx * Math.Sin(t_y)
                        - yy * Math.Cos(t_y) * Math.Sin(t_x)
                        + zz * Math.Cos(t_y) * Math.Cos(t_x)
                        + z0;

                }
            }
        }




    }

    public class CoordM : Coord//,ICoord
    {
        #region 変数
        double[] incAngle { get; set; }
        double[] curv { get; set; }

        #endregion
        public CoordM(int _divW, int _divL)
            : base(_divW, _divL)
        {
            incAngle = new double[div];
            curv = new double[divW];
        }
    }

    public class Mirror2D
    {
        public enum Dir { Horizontal, Vertical }
        public enum Pos { upper = 1, lower = -1, hall = -1, ring = 1 }

        public class Parameter
        {
            public double L1 { get; set; }
            public double L2 { get; set; }
            public double theta_i { get; set; }
            public double theta_s { get; set; }
            public double theta_f { get; set; }

            public Dir dir { get; set; }
            public Pos pos { get; set; }
            public double ML { get; set; }
            public double MW { get; set; }
            public int divL { get; set; }
            public int divW { get; set; }

            public double a { get; set; }
            public double b { get; set; }
            public double f { get; set; }
        }

        #region 変数

        double dtheta_rotX { get; set; }
        double dtheta_rotY { get; set; }
        double dtheta_rotZ { get; set; }

        public Mirror2D m0;
        public Parameter pm = new Parameter();
        public CoordM m;
        Coord s0;
        Coord f0;
        public Coord s;
        public Coord[] f;

        #endregion

        public Mirror2D(Parameter _pm)
        {
            Initialize(_pm);
        }

        public Mirror2D(Parameter _pm, Mirror2D _m0)
        {
            m0 = _m0;

            Initialize(_pm);
        }


        public void Initialize(Parameter _pm)
        {
            pm = _pm;

            s0 = new Coord(1, 1);
            m = new CoordM(pm.divW, pm.divL);
            f0 = new Coord(1, 1);

            Ellipse();

            //changeAngle(m0);
        }

        void Ellipse()
        {
            pm.a = (pm.L1 + pm.L2) / 2.0;
            pm.theta_s = (double)pm.pos * Math.Abs(Math.Atan(pm.L2 * Math.Sin(2.0 * pm.theta_i) / (pm.L1 + pm.L2 * Math.Cos(2.0 * pm.theta_i))));
            pm.theta_f = Math.Abs((double)pm.pos * pm.theta_s - 2.0 * pm.theta_i);

            pm.f = (pm.L1 * Math.Cos(pm.theta_s) + pm.L2 * Math.Cos(pm.theta_f)) / 2.0;
            pm.b = Math.Sqrt(pm.a * pm.a - pm.f * pm.f);

            s0.x[0] = -pm.f;
            s0.y[0] = 0.0;
            s0.z[0] = 0.0;

            f0.x[0] = pm.f;
            f0.y[0] = 0.0;
            f0.z[0] = 0.0;

            //x,y座標決定(座標系基準)
            double m_xc = pm.L1 * Math.Cos(pm.theta_s) - pm.f;
            double m_yc = 0.0;

            double dx = pm.ML / (pm.divL - 1.0);
            double dy = pm.MW / (pm.divW - 1.0);

            for (int i = 0; i < pm.divW; i++)
            {
                for (int j = 0; j < pm.divL; j++)
                {
                    m.x[i * pm.divL + j] = m_xc - pm.ML / 2.0 + dx * j;
                    m.y[i * pm.divL + j] = m_yc - pm.MW / 2.0 + dy * i;
                    m.z[i * pm.divL + j] = ell_z(m.x[i * pm.divL + j], m.y[i * pm.divL + j]);
                }
            }
            if (pm.dir == Dir.Horizontal)
                changeVH();

            changeAngle(m0);
        }

        double ell_z(double x, double y)
        {
            return (double)pm.pos * pm.b * Math.Sqrt(1 - Math.Pow(x / pm.a, 2.0));
        }

        public void Source(int _divY, int _divZ, double dy = 0.0, double dz = 0.0)
        {
            //double s_xc = s.xc;
            //double s_yc = s.yc;
            //double s_zc = s.zc;
            s = new Coord(_divY, _divZ);

            for (int i = 0; i < _divY; i++)
            {
                for (int j = 0; j < _divZ; j++)
                {
                    s.x[i * _divZ + j] = s0.xc;
                    s.y[i * _divZ + j] = s0.yc + (-_divY / 2.0 + i) * dy;
                    s.z[i * _divZ + j] = s0.zc + (-_divZ / 2.0 + j) * dz;
                    s.real[i * _divZ + j] = 1.0;
                    s.imag[i * _divZ + j] = 0.0;
                }
            }
            //s.Rot(Coord.RotAxis.rotY, pm.theta_s, s_xc, s_yc, s_zc);
        }

        public void Focus(int divX,int divY, int divZ, double dx,double dy, double dz)
        {
            f = new Coord[1];
            f[0] = new Coord(divY, divZ);

            for (int i = 0; i < divY; i++)
            {
                for (int j = 0; j < divZ; j++)
                {
                    f[0].x[i * divZ + j] = f0.xc;
                    f[0].y[i * divZ + j] = f0.yc + (-divY / 2 + i) * dy;
                    f[0].z[i * divZ + j] = f0.zc + (-divZ / 2 + j) * dz;
                }
            }

            double t_x = 0.0;
            double t_y = Math.Atan((f0.zc - m.zc) / (f0.xc - m.xc));
            double t_z = Math.Atan((f0.yc - m.yc) / (f0.xc - m.xc));
            f[0].RotCoord(t_x, t_y, t_z, f[0].xc, f[0].yc, f[0].zc);
        }

        void changeVH()
        {
            double[] y = m.y;
            double[] z = m.z;
            m.y = z;
            m.z = y;
        }

        void changeAngle(Mirror2D m0)
        {
            if (m0 == null) return;


            if (this.pm.dir == Dir.Vertical)
            {
                if (m0.pm.dir == Dir.Vertical)
                {

                }
                else
                {

                    //前ミラーの入射光線に合わせる
                    this.m.RotCoord(0.0, (double)pm.pos * pm.theta_s, (double)m0.pm.pos * m0.pm.theta_s, s0.xc, s0.yc, s0.zc);
                    //前ミラーの出射光線に合わせる
                    this.m.RotCoord(0.0, 0.0, 2.0 * (double)m0.pm.pos * m0.pm.theta_i, m0.m.xc, m0.m.yc, m0.m.zc);
                    //前ミラーの入射光線に合わせる
                    this.f0.RotCoord(0.0, (double)pm.pos * pm.theta_s, (double)m0.pm.pos * m0.pm.theta_s, s0.xc, s0.yc, s0.zc);
                    //前ミラーの出射光線に合わせる
                    this.f0.RotCoord(0.0, 0.0, 2.0 * (double)m0.pm.pos * m0.pm.theta_i, m0.m.xc, m0.m.yc, m0.m.zc);
                }
            }
            else
            {
                if (m0.pm.dir == Dir.Vertical)
                { }
                else
                { }
            }

        }

        public void rotXYZ(double x0, double y0, double theta)
        {
            double xc = m.xc;
            double yc = m.yc;
            double zc = m.zc;

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

        //class WaveField
        //{
        //    const double h = 6.62607e-34;
        //    const double e = 1.602e-19;
        //    const double c = 2.99792458e8;
        //    public double lambda { get; set; }
        //    public double Energy
        //    {
        //        get { return h * c / (e * lambda); }
        //        set { lambda = h * c / (e * value); }
        //    }

        //    //波動場
        //    public Complex[,] u2 { get { return coord(u); } set { u = coord(value); } }
        //    //波動場X座標二次元
        //    public double[,] x2 { get { return coord(x); } set { x = coord(value); } }
        //    //波動場Y座標二次元
        //    public double[,] y2 { get { return coord(y); } set { y = coord(value); } }
        //    //波動場Z座標二次元
        //    public double[,] z2 {get { return coord(z); } set { z = coord(value); } }
        //    //強度二次元
        //    public double[,] I2 { get { return coord(I); } }
        //    //位相二次元
        //    public double[,] P2 { get { return coord(P); } }

        //    public double[] x;
        //    public double[] y;
        //    public double[] z;
        //    public Complex[] u;
        //    public double[] I;
        //    public double[] P;

        //    public int div;
        //    public int div1 { get; private set; }
        //    public int div2 { get; private set; }

        //    #region progress
        //    IProgress<ProgressInfo> progress;
        //    int totalNum;
        //    int doneNum;
        //    int repNum;
        //    void report(int _value, string _message)
        //    {
        //        if (progress != null)
        //            progress.Report(new ProgressInfo(_value, _message + string.Format("計算中 {0}/{1}", _value, 100)));
        //    }
        //    #endregion

        //    Type[,] coord<Type>(Type[] _type)
        //    {
        //        Type[,] type = new Type[div1, div2];
        //        for(int i=0;i<div1;i++)
        //        {
        //            for(int j=0;j<div2;j++)
        //            {
        //                type[i, j] = _type[i * div2 + j];
        //            }
        //        }
        //        return type;
        //    }

        //    Type[] coord<Type>(Type[,] _type)
        //    {
        //        Type[] type = new Type[div];
        //        for(int i=0;i<div1;i++)
        //        {
        //            for(int j=0;j<div2;j++)
        //            {
        //                type[i * div2 + j] = _type[i, j];
        //            }
        //        }
        //        return type;
        //    }


        //    //コンストラクタ
        //    public WaveField(int _divW, int _divL, double _lambda)
        //    {
        //        //各値の初期化
        //        div1 = _divW;
        //        div2 = _divL;
        //        div = div1 * div2;

        //        lambda = _lambda;
        //    }

        //    public WaveField(ref Coord _coord, double _lambda)
        //    {
        //        lambda = _lambda;
        //        div1 = _coord.divW; 
        //        div2 = _coord.divL;
        //        div = div1 * div2;
                
        //        x2 = _coord.x;
        //        y2 = _coord.y;
        //        z2 = _coord.z;
        //        u2 = _coord.u;
        //    }

        //    public WaveField(ref CoordM _coord, double _lambda)
        //    {
        //        lambda = _lambda;
        //        div1 = _coord.divW;
        //        div2 = _coord.divL;
        //        div = div1 * div2;

        //        x2 = _coord.x;
        //        y2 = _coord.y;
        //        z2 = _coord.z;
        //        u2 = _coord.u;
        //    }


        //    //順方向伝播(引数：伝播元波動場)
        //    public void ForwardPropagation(WaveField u_back,IProgress<ProgressInfo> _progress=null)
        //    {
        //        this.progress = _progress;
        //        this.totalNum = div;
        //        this.doneNum = 0;
        //        this.repNum = this.totalNum / 100;

        //        double k;
        //        Complex ii = new Complex(0.0, 1.0);     //虚数単位

        //        //波数の計算
        //        k = 2.0 * Math.PI / this.lambda;

        //        //伝播計算
        //        //for (int i = 0; i < this.div; i++)
        //        //ParallelOptions po = new ParallelOptions();
        //        //po.MaxDegreeOfParallelism = Environment.ProcessorCount;
        //        Parallel.For(0, this.div, i =>
        //        {
        //            this.u[i] = new Complex(0.0, 0.0);

        //            for (int m = 0; m < u_back.div; m++)
        //            {
        //                this.u[i] += u_back.u[m] * Complex.Exp(-ii * k * Math.Sqrt(Math.Pow(this.x[i] - u_back.x[m], 2.0) + Math.Pow(this.y[i] - u_back.y[m], 2.0) + Math.Pow(this.z[i] - u_back.z[m], 2.0)))
        //                                / Math.Sqrt(Math.Pow(this.x[i] - u_back.x[m], 2.0) + Math.Pow(this.y[i] - u_back.y[m], 2.0) + Math.Pow(this.z[i] - u_back.z[m], 2.0));
        //            }

        //            Interlocked.Increment(ref this.doneNum);
        //            if (this.doneNum % this.repNum == 0)
        //                report(100 * (this.doneNum) / this.totalNum, "");
        //        });

        //        //強度計算
        //        this.I = new double[this.div];
        //        this.P = new double[this.div];
        //        for (int i = 0; i < this.div; i++)
        //        {
        //            this.I[i] = Math.Pow(this.u[i].Magnitude, 2.0);
        //            this.P[i] = this.u[i].Phase;
        //        }
        //    }


        //    ////逆方向伝播（引数；伝播元波動場）
        //    //public void BackwardPropagation(WaveField u_back)
        //    //{
        //    //    double k;
        //    //    double[] ds = new double[u_back.x.Length];
        //    //    Complex ii = new Complex(0.0, 1.0);     //虚数単位

        //    //    //波数の計算
        //    //    k = 2.0 * Math.PI / this.lambda;

        //    //    //伝播元の微小長さの計算
        //    //    if (u_back.x.Length != 1)
        //    //    {
        //    //        for (int i = 1; i < u_back.x.Length; i++)
        //    //        {
        //    //            ds[i] = Math.Sqrt(Math.Pow(u_back.x[i] - u_back.x[i - 1], 2.0) + Math.Pow(u_back.y[i] - u_back.y[i - 1], 2.0));
        //    //        }
        //    //        ds[0] = ds[1];
        //    //    }
        //    //    else
        //    //    {
        //    //        ds[0] = 1;
        //    //    }

        //    //    //伝播計算
        //    //    for (int i = 0; i < this.x.Length; i++)
        //    //    {
        //    //        this.u[i] = new Complex(0.0, 0.0);
        //    //        for (int j = 0; j < u_back.x.Length; j++)
        //    //        {
        //    //            this.u[i] += u_back.u[j] * Complex.Exp(ii * k * Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)))
        //    //                            / Math.Sqrt(Math.Pow(this.x[i] - u_back.x[j], 2.0) + Math.Pow(this.y[i] - u_back.y[j], 2.0)) * ds[j];
        //    //        }
        //    //    }
        //    //}

        //    ////FWHM計算
        //    //public static double FWHM(WaveField waveField)
        //    //{
        //    //    double max = 0.0;
        //    //    double iMax = 0;
        //    //    for (int i = 0; i < waveField.PosX.Length; i++)
        //    //        if (waveField.Intensity[i] > max)
        //    //        {
        //    //            max = waveField.Intensity[i];
        //    //            iMax = i;
        //    //        }

        //    //    double dblePos1 = 0.0;
        //    //    double dblePos2 = 0.0;
        //    //    for (int i = 1; i < iMax; i++)
        //    //        if (max / 2.0 < waveField.Intensity[i])
        //    //        {
        //    //            dblePos1 = (waveField.PosX[i] - waveField.PosX[i - 1]) / (waveField.Intensity[i] - waveField.Intensity[i - 1]) * (max / 2.0 - waveField.Intensity[i]) + waveField.PosX[i];
        //    //            break;
        //    //        }
        //    //    for (int i = waveField.PosX.Length - 2; i > iMax; i--)
        //    //        if (max / 2.0 < waveField.Intensity[i])
        //    //        {
        //    //            dblePos2 = (waveField.PosX[i] - waveField.PosX[i + 1]) / (waveField.Intensity[i] - waveField.Intensity[i + 1]) * (max / 2.0 - waveField.Intensity[i]) + waveField.PosX[i];
        //    //            break;
        //    //        }

        //    //    return dblePos2 - dblePos1;
        //    //}

        //}

    }



}
