using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace csNCCalc
{
    public partial class Form1 : Form
    {
        OpenFileDialog ofd_spot = new OpenFileDialog();
        OpenFileDialog ofd_fig = new OpenFileDialog();
        public Form1()
        {
            InitializeComponent();
        }

        private void button_Calc_Click(object sender, EventArgs e)
        {
            double[,] spot,fig_pre;
            double[,] time, fig_aft;
            int nLoop = 100;
            double damp = 5000;
            ClsNac.FileIO.FileIO.readFile(ofd_spot.FileName, out spot);
            ClsNac.FileIO.FileIO.readFile(ofd_fig.FileName, out fig_pre);


            //for (int i = 0; i < spot.GetLength(0); i++)
            //{
            //    for (int j = 0; j < spot.GetLength(1); j++)
            //    {
            //        spot[i, j] -= 1;
            //        spot[i, j] = spot[i, j] > 0 ? 0 : spot[i, j];
            //    }
            //}
            fig_pre = ClsNac.ArrayManipulate.Add(fig_pre, 100);

            ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\spot.txt", spot);
            ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\fig_pre.txt", fig_pre);

            double RMS=0.0;
            double PV = 0.0;
            double RMSpre = double.MaxValue;
            System.Diagnostics.Debug.Print("Start");
            for (int i = 0; i < nLoop; i++)
            {
                Deconvolution(spot, fig_pre, damp, out fig_aft, out time, out RMS, out PV);

                //if (ClsNac.ArrayManipulate.IncludeMinus(fig_aft))
                //{ fig_pre = ClsNac.ArrayManipulate.Add(fig_pre, 100); }
                //else 
                if (RMSpre < RMS)
                    break;//damp *= 1.2;
                else
                {
                    Array.Copy(fig_aft, fig_pre, fig_aft.Length);
                    RMSpre = RMS;
                }

                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\fig_aft" + i.ToString("D3") + ".txt", fig_aft);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\time" + i.ToString("D3") + ".txt", time);

                System.Diagnostics.Debug.Print(PV.ToString() + "," + RMS.ToString() + "," + damp.ToString());

            }
            System.Diagnostics.Debug.Print("Finish");
        }

        private void button_ofd_spot_Click(object sender, EventArgs e)
        {
            if (ofd_spot.ShowDialog() == DialogResult.OK)
            {
                textBox_ofd_spot.Text = ofd_spot.FileName;
            }
        }

        private void button_ofd_volume_Click(object sender, EventArgs e)
        {
            if (ofd_fig.ShowDialog() == DialogResult.OK)
            {
                textBox_ofd_fig.Text = ofd_fig.FileName;
            }
        }

        void Deconvolution(double[,] _spot, double[,] _figure_pre, double _damp,
            out double[,] _figure_aft, out double[,] _time, out double _RMS,out double _PV)
        {     
            int nx_Spot = _spot.GetLength(0);
            int ny_Spot = _spot.GetLength(1);

            int nx_Figure = _figure_pre.GetLength(0);
            int ny_Figure = _figure_pre.GetLength(1);

            //滞在時間幅はfigよりspot幅分だけ狭くなる
            int nx_StayTime = nx_Figure - nx_Spot;
            int ny_StayTime = ny_Figure - ny_Spot;

            _figure_aft = new double[nx_Figure, ny_Figure];
            _time = new double[nx_StayTime, ny_StayTime];
            double spotvol = Math.Abs(ClsNac.ArrayManipulate.Sum(_spot));

            Array.Copy(_figure_pre, _figure_aft, _figure_pre.Length);

            for (int iy_StayTime = 0; iy_StayTime < ny_StayTime; iy_StayTime++)
            {
                for (int ix_StayTime = 0; ix_StayTime < nx_StayTime; ix_StayTime++)
                {
                    //計算用滞在時間
                    double tmpStayTime = 0.0;
                    for (int ix_Spot = 0; ix_Spot < nx_Spot; ix_Spot++)
                    {
                        for (int iy_Spot = 0; iy_Spot < ny_Spot; iy_Spot++)
                        {
                            //形状をスポットで割る
                            tmpStayTime += (_figure_pre[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] / _spot[ix_Spot, iy_Spot]);
                        }
                    }

                    //平均してダンプで割った数値を滞在時間にする
                    tmpStayTime /= (spotvol* nx_Spot*ny_Spot* _damp);
                    //System.Diagnostics.Debug.Print(tmpStayTime.ToString());
                    _time[ix_StayTime, iy_StayTime] = tmpStayTime;

                    ////決定した滞在時間分のスポット形状を引き算
                    //for (int ix_Spot = 0; ix_Spot < nx_Spot; ix_Spot++)
                    //{
                    //    for (int iy_Spot = 0; iy_Spot < ny_Spot; iy_Spot++)
                    //    {
                    //        _figure_aft[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] = _figure_pre[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] += (_time[ix_StayTime, iy_StayTime] * _spot[ix_Spot, iy_Spot]);
                    //    }
                    //}
                    //ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\fig_aft" + iy_StayTime.ToString("D3") + "_" + ix_StayTime.ToString("D3") + ".txt", _figure_pre);



                }
            }
            for (int iy_StayTime = 0; iy_StayTime < ny_StayTime; iy_StayTime++)
            {
                for (int ix_StayTime = 0; ix_StayTime < nx_StayTime; ix_StayTime++)
                {
                    //決定した滞在時間分のスポット形状を引き算
                    for (int ix_Spot = 0; ix_Spot < nx_Spot; ix_Spot++)
                    {
                        for (int iy_Spot = 0; iy_Spot < ny_Spot; iy_Spot++)
                        {
                            _figure_aft[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] += (_time[ix_StayTime, iy_StayTime] * _spot[ix_Spot, iy_Spot]);
                        }
                    }
                }
            }
            //RMS
            _RMS = RMS(_figure_aft);
            //PV
            _PV = PV(_figure_aft);
        }

        void DeconvolutionV(double[,] _spot, double[,] _figure_pre, double _damp,
            out double[,] _figure_aft, out double[,] _vel, out double _RMS)
        {
            int nx_Spot = _spot.GetLength(0);
            int ny_Spot = _spot.GetLength(1);

            int nx_Figure = _figure_pre.GetLength(0);
            int ny_Figure = _figure_pre.GetLength(1);

            //滞在時間幅はfigよりspot幅分だけ狭くなる
            int nx_Velocity = nx_Figure - nx_Spot;
            int ny_Velocity = ny_Figure - ny_Spot;

            _figure_aft = new double[nx_Figure, ny_Figure];
            _vel = new double[nx_Velocity, ny_Velocity];

            for (int iy_Velocity = 0; iy_Velocity < ny_Velocity; iy_Velocity++)
            {
                for (int ix_Velocity = 0; ix_Velocity < nx_Velocity; ix_Velocity++)
                {
                    //計算用滞在時間
                    double tmpVelocity = 0.0;
                    for (int ix_Spot = 0; ix_Spot < nx_Spot; ix_Spot++)
                    {
                        for (int iy_Spot = 0; iy_Spot < ny_Spot; iy_Spot++)
                        {
                            //形状をスポットで割る
                            tmpVelocity += (_figure_pre[ix_Velocity + ix_Spot, iy_Velocity + iy_Spot] / _spot[ix_Spot, iy_Spot]);
                        }
                    }

                    //平均してダンプで割った数値を滞在時間にする
                    tmpVelocity /= ((nx_Spot * ny_Spot) * _damp);
                    //System.Diagnostics.Debug.Print(tmpStayTime.ToString());
                    _vel[ix_Velocity, iy_Velocity] = tmpVelocity;

                }
            }
            for (int iy_StayTime = 0; iy_StayTime < ny_Velocity; iy_StayTime++)
            {
                for (int ix_StayTime = 0; ix_StayTime < nx_Velocity; ix_StayTime++)
                {
                    //決定した滞在時間分のスポット形状を引き算
                    for (int ix_Spot = 0; ix_Spot < nx_Spot; ix_Spot++)
                    {
                        for (int iy_Spot = 0; iy_Spot < ny_Spot; iy_Spot++)
                        {
                            _figure_aft[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] = _figure_pre[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] - (_vel[ix_StayTime, iy_StayTime] * _spot[ix_Spot, iy_Spot]);
                        }
                    }
                }
            }
            //RMS
            _RMS = RMS(_figure_pre);
            //PV

        }







        void Deconvolution2(double[,] _spot, double[,] _figure_pre, double _damp,
            out double[,] _figure_aft, out double[,] _time, out double _RMS)
        {
            //計算領域拡張ver

            int nx_Spot = _spot.GetLength(0);
            int ny_Spot = _spot.GetLength(1);

            int nx_figure_pre = _figure_pre.GetLength(0);
            int ny_figure_pre = _figure_pre.GetLength(1);

            int nx_figure_calc = nx_figure_pre + 2 * nx_Spot;
            int ny_figure_calc = ny_figure_pre + 2 * ny_Spot;

            //滞在時間幅はcalcよりspot幅分だけ狭くなる
            int nx_StayTime = nx_figure_calc - nx_Spot;
            int ny_StayTime = ny_figure_calc - ny_Spot;

            double[,] figure_calc_pre = new double[nx_figure_calc, ny_figure_calc];
            double[,] figure_calc_aft = new double[nx_figure_calc, ny_figure_calc];

            _figure_aft = new double[nx_figure_pre, ny_figure_pre];

            double[,] time_tmp = new double[nx_StayTime, ny_StayTime];

            double spotvol = ClsNac.ArrayManipulate.Sum(_spot);


            for (int i=0;i<nx_figure_calc;i++)
            {
                for (int j = 0; j < ny_figure_calc; j++)
                {
                    if (i < nx_Spot)
                    {
                        //左
                        if (j < ny_Spot)
                            figure_calc_pre[i, j] = _figure_pre[0, 0];//左上
                        else if (j < ny_Spot + ny_figure_pre)
                            figure_calc_pre[i, j] = _figure_pre[0, j-ny_Spot];//左中
                        else
                            figure_calc_pre[i, j] = _figure_pre[0, ny_figure_pre - 1];//左下
                    }
                    else if (i < nx_Spot + nx_figure_pre)
                    {
                        //中
                        if (j < ny_Spot)
                            figure_calc_pre[i, j] = _figure_pre[i-nx_Spot, 0];//中上
                        else if (j < ny_Spot + ny_figure_pre)
                            figure_calc_pre[i, j] = _figure_pre[i-nx_Spot, j-ny_Spot];//中中
                        else
                            figure_calc_pre[i, j] = _figure_pre[i-nx_Spot, ny_figure_pre - 1];//中下
                    }
                    else
                    {
                        //右
                        if (j < ny_Spot)
                            figure_calc_pre[i, j] = _figure_pre[nx_figure_pre - 1, 0];//右上                        
                        else if (j < ny_Spot + ny_figure_pre)
                            figure_calc_pre[i, j] = _figure_pre[nx_figure_pre - 1, j-ny_Spot];//右中
                        else
                            figure_calc_pre[i, j] = _figure_pre[nx_figure_pre - 1, ny_figure_pre - 1];//右下
                    }
                }
            }


            for (int ix_StayTime = 0; ix_StayTime < nx_StayTime; ix_StayTime++)
            {

                for (int iy_StayTime = 0; iy_StayTime < ny_StayTime; iy_StayTime++)
                {
                    //計算用滞在時間
                    double tmpStayTime = 0.0;
                    int count = 0;
                    for (int ix_Spot = 0; ix_Spot < nx_Spot; ix_Spot++)
                    {
                        for (int iy_Spot = 0; iy_Spot < ny_Spot; iy_Spot++)
                        {
                            //形状をスポットで割る
                            if (_spot[ix_Spot, iy_Spot] > 0)
                            {
                                tmpStayTime += (figure_calc_pre[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] / _spot[ix_Spot, iy_Spot]);
                                count++;
                            }
                        }
                    }

                    //平均してダンプで割った数値を滞在時間にする
                    tmpStayTime /= (spotvol * (nx_Spot * ny_Spot) * _damp);
                    //tmpStayTime /= (count * _damp);
                    time_tmp[ix_StayTime, iy_StayTime] = tmpStayTime;
                    //System.Diagnostics.Debug.Print(tmpStayTime.ToString());

                }
            }
            for (int ix_StayTime = 0; ix_StayTime < nx_StayTime; ix_StayTime++)
            {

                for (int iy_StayTime = 0; iy_StayTime < ny_StayTime; iy_StayTime++)
                {

                    //決定した滞在時間分のスポット形状を引き算
                    for (int ix_Spot = 0; ix_Spot < nx_Spot; ix_Spot++)
                    {
                        for (int iy_Spot = 0; iy_Spot < ny_Spot; iy_Spot++)
                        {
                            figure_calc_pre[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] -= (time_tmp[ix_StayTime, iy_StayTime] * _spot[ix_Spot, iy_Spot]);
                        }
                    }
                }
            }

            _figure_aft = ClsNac.ArrayManipulate.ExtractAND(figure_calc_pre, new Rectangle(nx_Spot, ny_Spot, nx_figure_pre, ny_figure_pre));
            _time = ClsNac.ArrayManipulate.ExtractAND(time_tmp, new Rectangle(nx_Spot, ny_Spot, nx_figure_pre, ny_figure_pre));
            //RMS
            _RMS = RMS(_figure_aft);
            //PV

        }





        double RMS(double[,] _data)
        {
            double sumsq = 0.0;
            for (int i = 0; i < _data.GetLength(0); i++)
            {
                for (int j = 0; j < _data.GetLength(1); j++)
                {
                    sumsq += Math.Pow(_data[i, j], 2.0);
                }
            }
            return Math.Sqrt(sumsq / _data.Length);
        }

        double PV(double[,]_data)
        {
            double max = double.MinValue;
            double min = double.MaxValue;
            for(int i=0;i<_data.GetLength(0);i++)
            {
                for(int j=0;j<_data.GetLength(1);j++)
                {
                    if (max < _data[i, j]) max = _data[i, j];
                    if (min > _data[i, j]) min = _data[i, j];
                }
            }
            return max - min;
        }
    }
}
