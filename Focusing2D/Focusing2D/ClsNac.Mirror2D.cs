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



}

