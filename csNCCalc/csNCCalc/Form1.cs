#define PARALLEL

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
            int nLoop = 20;
            double damp = Convert.ToDouble(textBox_damp.Text);
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
            //spot = ClsNac.ArrayManipulate.Multiply(spot, 1e9);
            //fig_pre = ClsNac.ArrayManipulate.Multiply(fig_pre, 1e9);
            //fig_pre = ClsNac.ArrayManipulate.Add(fig_pre, 100);

            ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\spot.txt", spot);
            ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\fig_pre.txt", fig_pre);

            double RMS=0.0;
            double PV = 0.0;
            double RMSpre = double.MaxValue;
            StringBuilder sb = new StringBuilder();
            System.Diagnostics.Debug.Print("Start");
            sb.AppendLine("PV,RMS,damp");
            int windownx = spot.GetLength(0);
            int windowny = spot.GetLength(1);

            for (int i = 0; i < nLoop; i++)
            {
                double[,] fig_aft_tmp, time_tmp;
                double[,] fig_pre_mod = PreFigure(fig_pre, 2 * windownx, 2 * windowny);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\fig_pre_mod" + i.ToString("D3") + ".txt", fig_pre_mod);

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                Deconvolution(spot, fig_pre_mod, damp, out fig_aft_tmp, out time_tmp, out RMS, out PV);
                sw.Stop();
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\fig_aft_mod" + i.ToString("D3") + ".txt", fig_aft_tmp);
                fig_aft = ClsNac.ArrayManipulate.ExtractAND(fig_aft_tmp, new Rectangle(windownx, windowny, fig_pre.GetLength(0), fig_pre.GetLength(1)));

                System.Diagnostics.Debug.Print(PV.ToString() + "," + RMS.ToString() + "," + damp.ToString() + "," + sw.ElapsedMilliseconds.ToString());
                sb.AppendLine(PV.ToString() + "," + RMS.ToString() + "," + damp.ToString() + "," + sw.ElapsedMilliseconds.ToString());
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\fig_aft" + i.ToString("D3") + ".txt", fig_aft);
                ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\time" + i.ToString("D3") + ".txt", time_tmp);

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


            }
            System.IO.File.WriteAllText(Application.StartupPath + "\\stat.txt", sb.ToString());
            //System.Diagnostics.Debug.Print("Finish");
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
            int nx_StayTime = nx_Figure - nx_Spot+1;
            int ny_StayTime = ny_Figure - ny_Spot+1;

            _figure_aft = new double[nx_Figure, ny_Figure];
            double[,] _timeTmp = new double[nx_StayTime, ny_StayTime];

            double spotvol = Math.Abs(ClsNac.ArrayManipulate.Sum(_spot));

//#if PARALLEL
//            Parallel.For(0, ny_StayTime, iy_StayTime =>
//#else
//            for (int iy_StayTime = 0; iy_StayTime < ny_StayTime; iy_StayTime++)
//#endif
//            {
//                for (int ix_StayTime = 0; ix_StayTime < nx_StayTime; ix_StayTime++)
//                {
//                    //計算用滞在時間
//                    double tmpStayTime = 0.0;
//                    for (int ix_Spot = 0; ix_Spot < nx_Spot; ix_Spot++)
//                    {
//                        for (int iy_Spot = 0; iy_Spot < ny_Spot; iy_Spot++)
//                        {
//                            //形状をスポットで割る
//                            tmpStayTime += (_figure_pre[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] / _spot[ix_Spot, iy_Spot]);
//                        }
//                    }

//                    //平均してダンプで割った数値を滞在時間にする
//                    tmpStayTime /= (spotvol * nx_Spot * ny_Spot * _damp);
//                    //System.Diagnostics.Debug.Print(tmpStayTime.ToString());
//                    _timeTmp[ix_StayTime, iy_StayTime] = tmpStayTime;

//                    //決定した滞在時間分のスポット形状を引き算
//                    //for (int ix_Spot = 0; ix_Spot < nx_Spot; ix_Spot++)
//                    //{
//                    //    for (int iy_Spot = 0; iy_Spot < ny_Spot; iy_Spot++)
//                    //    {
//                    //        _figure_aft[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] = _figure_pre[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] += (_time[ix_StayTime, iy_StayTime] * _spot[ix_Spot, iy_Spot]);
//                    //    }
//                    //}
//                    //ClsNac.FileIO.FileIO.writeFile(Application.StartupPath + "\\fig_aft" + iy_StayTime.ToString("D3") + "_" + ix_StayTime.ToString("D3") + ".txt", _figure_pre);
//                }
//            }
//#if PARALLEL
//            );
//#endif
            double[,] figure_aft = new double[nx_Figure, ny_Figure];
            Array.Copy(_figure_pre, figure_aft, _figure_pre.Length);


            int nx_Para = nx_StayTime / nx_Spot + 1;
            int ny_Para = ny_StayTime / ny_Spot + 1;

            for (int ix_Move = 0; ix_Move < nx_Spot; ix_Move++)
            {
                for (int iy_Move = 0; iy_Move < ny_Spot; iy_Move++)
                {
#if PARALLEL
                    Parallel.For(0, nx_Para, ix_Para =>
                    //ここで並列化
#else
                    for (int ix_Para = 0; ix_Para < nx_Para; ix_Para++)
#endif
                    {
                        //範囲超える場合break
                        int ix = ix_Move + ix_Para * nx_Spot;
                        
                        if (ix < nx_StayTime)
                        {
                            for (int iy_Para = 0; iy_Para < ny_Para; iy_Para++)
                            {
                                //範囲超える場合break
                                int iy = iy_Move + iy_Para * ny_Spot;
                                if (iy < ny_StayTime)
                                {

                                    //計算用滞在時間
                                    double timeTmp = 0.0;
                                    for (int ix_Spot = 0; ix_Spot < nx_Spot; ix_Spot++)
                                    {
                                        for (int iy_Spot = 0; iy_Spot < ny_Spot; iy_Spot++)
                                        {
                                            //形状をスポットで割る
                                            timeTmp += (_figure_pre[ix + ix_Spot, iy + iy_Spot] / _spot[ix_Spot, iy_Spot]);
                                        }
                                    }

                                    //平均してダンプで割った数値を滞在時間にする
                                    timeTmp /= (spotvol * nx_Spot * ny_Spot * _damp);
                                    //System.Diagnostics.Debug.Print(tmpStayTime.ToString());
                                    _timeTmp[ix, iy] = timeTmp;



                                    //決定した滞在時間分のスポット形状を引き算
                                    for (int ix_Spot = 0; ix_Spot < nx_Spot; ix_Spot++)
                                    {
                                        for (int iy_Spot = 0; iy_Spot < ny_Spot; iy_Spot++)
                                        {
                                            figure_aft[ix + ix_Spot, iy + iy_Spot]
                                            += (_timeTmp[ix, iy] * _spot[ix_Spot, iy_Spot]);
                                        }
                                    }
                                }
                            }
                        }
                    }
#if PARALLEL
                    );
#endif
                }
            }
            //for (int iy_StayTime = 0; iy_StayTime < ny_StayTime; iy_StayTime++)
            //{
            //    for (int ix_StayTime = 0; ix_StayTime < nx_StayTime; ix_StayTime++)
            //    {
            //        //決定した滞在時間分のスポット形状を引き算
            //        for (int ix_Spot = 0; ix_Spot < nx_Spot; ix_Spot++)
            //        {
            //            for (int iy_Spot = 0; iy_Spot < ny_Spot; iy_Spot++)
            //            {
            //                figure_aft[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] = _figure_pre[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] + (_timeTmp[ix_StayTime, iy_StayTime] * _spot[ix_Spot, iy_Spot]);
            //            }
            //        }
            //    }
            //}
            _time = _timeTmp;
            _figure_aft = figure_aft;
            //RMS,PV
            (_RMS, _PV) = RMSPV(_figure_aft);

        }

        /// <summary>
        /// 初期速度の決定
        /// </summary>
        /// <param name="_spot"></param>
        /// <param name="_figure_pre"></param>
        /// <param name="_vel"></param>
        /// <param name="_saikou"></param>
        void DeconvolutionVelocityPre(double[,] _spot, double[,] _figure_pre, out double[,] _vel, double _saikou)
        {
            int nx_Spot = _spot.GetLength(0);
            int ny_Spot = _spot.GetLength(1);

            int nx_Figure = _figure_pre.GetLength(0);
            int ny_Figure = _figure_pre.GetLength(1);

            //滞在時間幅はfigよりspot幅分だけ狭くなる
            int nx_StayTime = nx_Figure - nx_Spot + 1;
            int ny_StayTime = ny_Figure - ny_Spot + 1;

            _vel = new double[nx_StayTime, ny_StayTime];

            for (int ix_StayTime = 0; ix_StayTime < nx_StayTime; ix_StayTime++)
            {
                for (int iy_StayTime = 0; iy_StayTime < ny_StayTime; iy_StayTime++)
                {
                    _vel[ix_StayTime, iy_StayTime] = _saikou;
                }
            }
        }


        void DeconvolutionVelocity(double[,] _spot, double[,] _figure_pre, double _damp,
            out double[,] _figure_aft, ref double[,] _vel, out double _RMS, out double _PV)
        {
            int nx_Spot = _spot.GetLength(0);
            int ny_Spot = _spot.GetLength(1);

            int nx_Figure = _figure_pre.GetLength(0);
            int ny_Figure = _figure_pre.GetLength(1);

            //滞在時間幅はfigよりspot幅分だけ狭くなる
            int nx_StayTime = nx_Figure - nx_Spot + 1;
            int ny_StayTime = ny_Figure - ny_Spot + 1;

            _figure_aft = new double[nx_Figure, ny_Figure];
            double[,] _velTmp = new double[nx_StayTime, ny_StayTime];

            double spotvol = Math.Abs(ClsNac.ArrayManipulate.Sum(_spot));

            double[,] figure_aft = new double[nx_Figure, ny_Figure];
            Array.Copy(_figure_pre, figure_aft, _figure_pre.Length);
            double[,] volTmp = new double[nx_Figure, ny_Figure];

            int nx_Para = nx_StayTime / nx_Spot + 1;
            int ny_Para = ny_StayTime / ny_Spot + 1;

            //Volume
            for (int ix_Move = 0; ix_Move < nx_Spot; ix_Move++)
            {
                for (int iy_Move = 0; iy_Move < ny_Spot; iy_Move++)
                {
#if PARALLEL
                    //
                    Parallel.For(0, nx_Para, ix_Para =>
                    //ここで並列化
#else
                    for (int ix_Para = 0; ix_Para < nx_Para; ix_Para++)
#endif
                    {
                        //範囲超える場合break
                        int ix = ix_Move + ix_Para * nx_Spot;
                        if (ix < nx_StayTime)
                        {
                            for (int iy_Para = 0; iy_Para < ny_Para; iy_Para++)
                            {
                                //範囲超える場合break
                                int iy = iy_Move + iy_Para * ny_Spot;
                                if (iy < ny_StayTime)
                                {
                                    //Spot Loop
                                    for (int ix_Spot = 0; ix_Spot < nx_Spot; ix_Spot++)
                                    {
                                        for (int iy_Spot = 0; iy_Spot < ny_Spot; iy_Spot++)
                                        {
                                            //スポット/速度から加工量計算
                                            volTmp[ix + ix_Spot, iy + iy_Spot] += _spot[ix_Spot, iy_Spot] / _velTmp[ix, iy];
                                        }
                                    }
                                }
                            }
                        }
                    }
#if PARALLEL
                    );
#endif
                }
            }

            //Pre形状に足し算してAft形状だす
            //PARALLELできる
            for (int ix_Figure = 0; ix_Figure < nx_Figure; ix_Figure++)
            {
                for (int iy_Figure = 0; iy_Figure < ny_StayTime; iy_Figure++)
                {
                    figure_aft[ix_Figure, iy_Figure] += volTmp[ix_Figure, iy_Figure];
                }
            }




            //for (int iy_StayTime = 0; iy_StayTime < ny_StayTime; iy_StayTime++)
            //{
            //    for (int ix_StayTime = 0; ix_StayTime < nx_StayTime; ix_StayTime++)
            //    {
            //        //決定した滞在時間分のスポット形状を引き算
            //        for (int ix_Spot = 0; ix_Spot < nx_Spot; ix_Spot++)
            //        {
            //            for (int iy_Spot = 0; iy_Spot < ny_Spot; iy_Spot++)
            //            {
            //                figure_aft[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] = _figure_pre[ix_StayTime + ix_Spot, iy_StayTime + iy_Spot] + (_timeTmp[ix_StayTime, iy_StayTime] * _spot[ix_Spot, iy_Spot]);
            //            }
            //        }
            //    }
            //}
            _vel = _velTmp;
            _figure_aft = figure_aft;
            //RMS,PV
            (_RMS, _PV) = RMSPV(_figure_aft);

        }




        double[,] PreFigure(double[,] _fig, int _windownx, int _windowny)
        {
            int fignx = _fig.GetLength(0);
            int figny = _fig.GetLength(1);
            int all_nx = fignx + _windownx;
            int all_ny = figny + _windowny;
            int windownx_2 = _windownx / 2;
            int windowny_2 = _windowny / 2;
            double[,] dbleFigMod = new double[all_nx, all_ny];


            for (int i = 0; i < fignx; i++)
            {
                for (int j = 0; j < figny; j++)
                {
                    dbleFigMod[windownx_2 + i, windowny_2 + j] = _fig[i, j];
                }
            }

            //X方向Window
            for (int i = 0; i < windownx_2; i++)
            {
                for (int j = 0; j < figny; j++)
                {
                    dbleFigMod[i , j  + windowny_2] = Window(_fig[0, j], (double)i / _windownx);
                    dbleFigMod[i + windownx_2 + fignx, j  + windowny_2] = Window(_fig[fignx - 1, j], 0.5 - (double)i / _windownx);
                }
            }

            for (int j = 0; j < windowny_2; j++)
            {
                for (int i = 0; i < fignx; i++)
                {
                    dbleFigMod[i  + windownx_2, j ] = Window(_fig[i, 0], (double)j / _windowny);
                    dbleFigMod[i  + windownx_2, j  + windowny_2 + figny] = Window(_fig[i, figny - 1], 0.5 - (double)j / _windowny);
                }
            }

            for (int i = 0; i < windownx_2; i++)
            {
                for (int j = 0; j < windowny_2; j++)
                {
                    dbleFigMod[i, j]
                        = dbleFigMod[i, windowny_2]
                        * dbleFigMod[windownx_2, j]
                        / dbleFigMod[windownx_2, windowny_2];
                    dbleFigMod[i + windownx_2 + fignx, j]
                        = dbleFigMod[i + windownx_2 + fignx - 1, windowny_2]
                        * dbleFigMod[windownx_2 + fignx - 1, j]
                        / dbleFigMod[windownx_2 + fignx - 1, windowny_2];
                    dbleFigMod[i, j + windowny_2 + figny]
                        = dbleFigMod[windownx_2, j + windowny_2 + figny - 1]
                        * dbleFigMod[i, windowny_2 + figny - 1]
                        / dbleFigMod[windownx_2, windowny_2 + figny - 1];
                    dbleFigMod[i + windownx_2 + fignx, j + windowny_2 + figny]
                        = dbleFigMod[windownx_2 + fignx - 1, j + windowny_2 + figny]
                        * dbleFigMod[i + windownx_2 + fignx - 1, windowny_2 + figny - 1]
                        / dbleFigMod[windownx_2 + fignx - 1, windowny_2 + figny - 1];
                }
            }

            return dbleFigMod;
        }

        double Window(double value, double pos)
        {
            //return CircleWindow(value, pos);
            return HannWindow(value, pos);
        }

        /// <summary>
        /// ハン窓
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        double HannWindow(double value, double pos)
        {
            return value * (0.5 - 0.5 * Math.Cos(2 * Math.PI * pos));
        }

        public static Tuple<double,double> RMSPV(double[,] _data)
        {
            double sumsq = 0.0;
            double max = double.MinValue;
            double min = double.MaxValue;

            for (int i = 0; i < _data.GetLength(0); i++)
            {
                for (int j = 0; j < _data.GetLength(1); j++)
                {
                    sumsq += Math.Pow(_data[i, j], 2.0);
                    if (max < _data[i, j]) max = _data[i, j];
                    if (min > _data[i, j]) min = _data[i, j];
                }
            }
            double RMS = Math.Sqrt(sumsq / _data.Length);
            double PV = max - min;

            return new Tuple<double, double>(RMS, PV);
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


        void daikakoukon(out double[,] _dai, int _ndaix, int _ndaiy, double[,] _zsp, int _nspx, int _nspy, double _eemdt, double _okudt, out double _daivol)
        {
            _dai = new double[_ndaix, _ndaiy];
            _daivol = 0.0;
            int nd = (int)(_okudt / _eemdt);
            
            for (int i = 0; i < nd; i++)
            {
                for (int j = 0; j < nd; j++)
                {
                    for (int k = 0; k < _nspx; k++)
                    {
                        for (int m = 0; m < _nspy; m++)
                        {
                            _dai[i + k, j + m] += _zsp[k, m];
                            _daivol += _zsp[k, m];
                        }
                    }
                }
            }
        }
    }
}
