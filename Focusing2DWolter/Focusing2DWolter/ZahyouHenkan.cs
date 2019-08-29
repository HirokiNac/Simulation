using System;
using System.Collections.Generic;
using System.Text;

//2016/02/08更新
namespace ZahyouHenkan
{
    public static class ZahyouHenkan
    {
        /// <summary>
        /// 平面補正時のステータス
        /// </summary>
        public enum PlaneCorrectionStatus
        {
            ZeroMin,
            ZeroAverage,
        }

        /// <summary>
        /// データ補間ルーチン（単位は全て一緒に、左上原点）
        /// </summary>
        /// <param name="rotationAngle">データの回転角</param>
        /// <param name="dtInput">読み込みデータのデータ間隔</param>
        /// <param name="x0_Genten">切り出し原点左上Ｘ座標</param>
        /// <param name="y0_Genten">切り出し原点左上Ｙ座標</param>
        /// <param name="dataInput">入力データ配列</param>
        /// <param name="dtOutput">出力データのデータ間隔</param>
        /// <param name="dx_Kiridashi">出力データの切り出し幅ＤＸ</param>
        /// <param name="dy_Kiridashi">出力データの切り出し幅ＤＹ</param>
        /// <param name="dataOutput">出力データ配列</param>
        public static void CoordinateTransform(double rotationAngle, double dtInput, double x0_Genten, double y0_Genten, double[,] dataInput,
                                               double dtOutput, double dx_Kiridashi, double dy_Kiridashi, ref double[,] dataOutput)
        {
            try
            {
                double dbleMinimum = 1.0e128;

                double p, q, Zac, Zbd;
                int ax, ay, bx, by, cx, cy, dx, dy;

                int intYInput, intXInput, intYOutput, intXOutput;

                //入力データ配列数と出力データ配列数の決定
                intXInput = dataInput.GetLength(0);
                intYInput = dataInput.GetLength(1);

                //出力データ配列のデータ数
                intXOutput = dataOutput.GetLength(0);
                intYOutput = dataOutput.GetLength(1);

                //補間＆回転データの計算
                for (int i = 0; i < intXOutput; i++)
                {
                    for (int j = 0; j < intYOutput; j++)
                    {
                        //(i,j)の前加工面でのGPI座標への変換(p,q) :: DataInput[i,j]はxy座標で(i,j)に相当することに注意
                        p = (Math.Cos(rotationAngle) * (double)(i) + Math.Sin(rotationAngle) * (double)(j)) * dtOutput / dtInput + x0_Genten;
                        q = (-Math.Sin(rotationAngle) * (double)(i) + Math.Cos(rotationAngle) * (double)(j)) * dtOutput / dtInput + y0_Genten;

                        //回転、補間後のデータが元データより外の場合は値を０にする
                        if ((q > intYInput -1) | (q < 0) | (p > intXInput -1) | (p < 0))
                        {
                            dataOutput[i, j] = 0.0;
                        }
                        else
                        {
                            ax = (int)p;
                            ay = (int)q;
                            bx = ax;
                            by = ay + 1;
                            cx = ax + 1;
                            cy = ay;
                            dx = ax + 1;
                            dy = ay + 1;

                            Zac = (dataInput[ax, ay] * (cx - p) + dataInput[cx, cy] * (p - ax)) / (cx - ax);
                            Zbd = (dataInput[bx, by] * (dx - p) + dataInput[dx, dy] * (p - bx)) / (dx - bx);
                            dataOutput[i, j] = Zac * (by - q) + Zbd * (q - ay);

                            //最小値の更新
                            if (dbleMinimum > dataOutput[i, j])
                            {
                                dbleMinimum = dataOutput[i, j];
                            }
                        }
                    }
                }

                ////最小値を０に
                //for (int i = 0; i < intXOutput; i++)
                //{
                //    for (int j = 0; j < intYOutput; j++)
                //    {
                //        dataOutput[i, j] -= dbleMinimum;
                //    }
                //}

                return;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace, "エラー発生");
            }
        }

        //ハイパスフィルター(区間平均を引く)
        //引数：区間平均の前後ピクセル数+-X,補正データ
        public static void LineHighPassFilter(int kukanAveragePix, ref double[,] Data)
        {
            double a, b;
            double[,] hei = new double[Data.GetLength(0), Data.GetLength(1)];
            double[] hin = new double[Data.GetLength(1)];

            for (int i = 0; i < Data.GetLength(0); i++)
            {
                for (int j = 0; j < Data.GetLength(1); j++)
                {
                    a = 0.0;
                    b = 0.0;
                    for (int k = j - kukanAveragePix; k < j + kukanAveragePix; k++)
                    {
                        if (k >= 0 && k < Data.GetLength(1))
                        {
                            a++;
                            b += Data[i, j];
                        }
                    }
                    hei[i, j] = b / a;
                }
            }

            for (int j = 0; j < Data.GetLength(1); j++)
            {
                hin[j] = 0.0;
                for (int i = 0; i < Data.GetLength(0); i++)
                {
                    hin[j] += hei[i, j] / Data.GetLength(0);
                }
            }

            for (int i = 0; i < Data.GetLength(0); i++)
            {
                for (int j = 0; j < Data.GetLength(1); j++)
                {
                    Data[i, j] -= hin[j];
                }
            }
        }

        public static void LeastSquare(double[,] Data,out double a, out double b, out double c)
        {
            int intX, intY;
            double d11, d12, d13, d21, d22, d23, d31, d32, d33, det;
            double sa, sb, sc, sd, se, sf, sg, sh, si;
            sa = 0.0;
            sb = 0.0;
            sc = 0.0;
            sd = 0.0;
            se = 0.0;
            sf = 0.0;
            sg = 0.0;
            sh = 0.0;
            si = 0.0;

            intX = Data.GetLength(0);
            intY = Data.GetLength(1);

            for (int i = 0; i < intX; i++)
            {
                for (int j = 0; j < intY; j++)
                {
                    if (Data[i, j] != 0.0)
                    {

                        sa += Convert.ToDouble(i * i);
                        sb += Convert.ToDouble(i * j);
                        sc += Convert.ToDouble(i);
                        sd += Convert.ToDouble(j * j);
                        se += Convert.ToDouble(j);
                        sf += 1.0;
                        sg += Convert.ToDouble(i) * Data[i, j];
                        sh += Convert.ToDouble(j) * Data[i, j];
                        si += Data[i, j];
                    }
                }
            }

            //逆行列の計算
            d11 = sd * sf - se * se;
            d12 = se * sc - sb * sf;
            d13 = sb * se - sc * sd;
            d21 = sc * se - sb * sf;
            d22 = sa * sf - sc * sc;
            d23 = sb * sc - sa * se;
            d31 = sb * se - sc * sd;
            d32 = sb * sc - sa * se;
            d33 = sa * sd - sb * sb;

            det = sa * d11 + sb * d12 + sc * d13;

            a = (d11 * sg + d12 * sh + d13 * si) / det;
            b = (d21 * sg + d22 * sh + d23 * si) / det;
            c = (d31 * sg + d32 * sh + d33 * si) / det;

        }

        public static void LeastSquare(double[,] Data, out double a, out double b)
        {
            //CoverLossData2(Data, ref Data);

            int intX, intY;
            double d11, d12, d13, d21, d22, d23, d31, d32, d33, det;
            double c, sa, sb, sc, sd, se, sf, sg, sh, si;
            sa = 0.0;
            sb = 0.0;
            sc = 0.0;
            sd = 0.0;
            se = 0.0;
            sf = 0.0;
            sg = 0.0;
            sh = 0.0;
            si = 0.0;

            intX = Data.GetLength(0);
            intY = Data.GetLength(1);

            for (int i = 0; i < intX; i++)
            {
                for (int j = 0; j < intY; j++)
                {
                    if (Data[i, j] != 0.0)
                    {
                        sa += Convert.ToDouble(i * i);
                        sb += Convert.ToDouble(i * j);
                        sc += Convert.ToDouble(i);
                        sd += Convert.ToDouble(j * j);
                        se += Convert.ToDouble(j);
                        sf += 1.0;
                        sg += Convert.ToDouble(i) * Data[i, j];
                        sh += Convert.ToDouble(j) * Data[i, j];
                        si += Data[i, j];
                }
            }
            }

            //逆行列の計算
            d11 = sd * sf - se * se;
            d12 = se * sc - sb * sf;
            d13 = sb * se - sc * sd;
            d21 = sc * se - sb * sf;
            d22 = sa * sf - sc * sc;
            d23 = sb * sc - sa * se;
            d31 = sb * se - sc * sd;
            d32 = sb * sc - sa * se;
            d33 = sa * sd - sb * sb;

            det = sa * d11 + sb * d12 + sc * d13;

            a = (d11 * sg + d12 * sh + d13 * si) / det;
            b = (d21 * sg + d22 * sh + d23 * si) / det;
            c = (d31 * sg + d32 * sh + d33 * si) / det;

        }


        //平面補正
        //引数：平面補正を行う二次元データ
        public static void PlaneCorrection(PlaneCorrectionStatus status, ref double[,] Data)
        {
            int intX = Data.GetLength(0), intY = Data.GetLength(1);
            double minimum = 1.0e128;
            double a, b, c;

            LeastSquare(Data, out a, out b, out c);

            //平面補正
            for (int i = 0; i < intX; i++)
            {
                for (int j = 0; j < intY; j++)
                {
                    Data[i, j] -= (a * i + b * j + c);
                    if (minimum > Data[i, j])
                    {
                        minimum = Data[i, j];
                    }
                }
            }

            //最小値を零にする場合
            if (status == PlaneCorrectionStatus.ZeroMin)
            {
                //最小値を０に
                for (int i = 0; i < intX; i++)
                {
                    for (int j = 0; j < intY; j++)
                    {
                        Data[i, j] -= minimum;
                    }
                }
            }
            //平均値を零にする場合
            else if (status == PlaneCorrectionStatus.ZeroAverage)
            {
                double dbleAve = 0;
                for (int i = 0; i < intX; i++)
                {
                    for (int j = 0; j < intY; j++)
                    {
                        dbleAve += Data[i, j];
                    }
                }
                dbleAve = dbleAve / (Data.GetLength(0) * Data.GetLength(1));
                //最小値を０に
                for (int i = 0; i < intX; i++)
                {
                    for (int j = 0; j < intY; j++)
                    {
                        Data[i, j] -= dbleAve;
                    }
                }
            }

            return;
        }

        //平面補正
        //引数：平面補正を行う二次元データ
        //ゼロ部分を省いた平面補正
        //2012/09/22更新
        public static void PlaneCorrection(PlaneCorrectionStatus status,int[,] zero, ref double[,] Data)
        {
            int intX, intY;
            double minimum = 1.0e128;
            double a, b, c, d11, d12, d13, d21, d22, d23, d31, d32, d33, det;
            double sa, sb, sc, sd, se, sf, sg, sh, si;
            sa = 0.0;
            sb = 0.0;
            sc = 0.0;
            sd = 0.0;
            se = 0.0;
            sf = 0.0;
            sg = 0.0;
            sh = 0.0;
            si = 0.0;

            intX = Data.GetLength(0);
            intY = Data.GetLength(1);

            for (int i = 0; i < intX; i++)
            {
                for (int j = 0; j < intY; j++)
                {
                    if (zero[i, j] == 0)
                    {
                        sa += Convert.ToDouble(i * i);
                        sb += Convert.ToDouble(i * j);
                        sc += Convert.ToDouble(i);
                        sd += Convert.ToDouble(j * j);
                        se += Convert.ToDouble(j);
                        sf += 1.0;
                        sg += Convert.ToDouble(i) * Data[i, j];
                        sh += Convert.ToDouble(j) * Data[i, j];
                        si += Data[i, j];
                    }
                }
            }

            //逆行列の計算
            d11 = sd * sf - se * se;
            d12 = se * sc - sb * sf;
            d13 = sb * se - sc * sd;
            d21 = sc * se - sb * sf;
            d22 = sa * sf - sc * sc;
            d23 = sb * sc - sa * se;
            d31 = sb * se - sc * sd;
            d32 = sb * sc - sa * se;
            d33 = sa * sd - sb * sb;

            det = sa * d11 + sb * d12 + sc * d13;

            a = (d11 * sg + d12 * sh + d13 * si) / det;
            b = (d21 * sg + d22 * sh + d23 * si) / det;
            c = (d31 * sg + d32 * sh + d33 * si) / det;

            //平面補正
            for (int i = 0; i < intX; i++)
            {
                for (int j = 0; j < intY; j++)
                {
                    Data[i, j] -= (a * i + b * j + c);
                    if (minimum > Data[i, j])
                    {
                        minimum = Data[i, j];
                    }
                }
            }

            //最小値を零にする場合
            if (status == PlaneCorrectionStatus.ZeroMin)
            {
                //最小値を０に
                for (int i = 0; i < intX; i++)
                {
                    for (int j = 0; j < intY; j++)
                    {
                        Data[i, j] -= minimum;
                    }
                }
            }
            //平均値を零にする場合
            else if (status == PlaneCorrectionStatus.ZeroAverage)
            {
                double dbleAve = 0;
                int count = 0;
                for (int i = 0; i < intX; i++)
                    for (int j = 0; j < intY; j++)
                        if (zero[i, j] == 0)
                        {
                            dbleAve += Data[i, j];
                            count++;
                        }
                dbleAve = dbleAve / count;
                //最小値を０に
                for (int i = 0; i < intX; i++)
                    for (int j = 0; j < intY; j++)
                        if (zero[i, j] == 0)
                            Data[i, j] -= dbleAve;
                        else
                            Data[i, j] = 0.0;


            }

            return;
        }

        public static void PlaneCorrection(PlaneCorrectionStatus status, int[,] zero,double[,] subData, ref double[,] Data)
        {
            int intX, intY;
            double minimum = 1.0e128;
            double a, b, c, d11, d12, d13, d21, d22, d23, d31, d32, d33, det;
            double sa, sb, sc, sd, se, sf, sg, sh, si;
            sa = 0.0;
            sb = 0.0;
            sc = 0.0;
            sd = 0.0;
            se = 0.0;
            sf = 0.0;
            sg = 0.0;
            sh = 0.0;
            si = 0.0;

            intX = Data.GetLength(0);
            intY = Data.GetLength(1);

            for (int i = 0; i < intX; i++)
            {
                for (int j = 0; j < intY; j++)
                {
                    if (zero[i, j] == 0)
                    {
                        sa += Convert.ToDouble(i * i);
                        sb += Convert.ToDouble(i * j);
                        sc += Convert.ToDouble(i);
                        sd += Convert.ToDouble(j * j);
                        se += Convert.ToDouble(j);
                        sf += 1.0;
                        sg += Convert.ToDouble(i) * Data[i, j];
                        sh += Convert.ToDouble(j) * Data[i, j];
                        si += Data[i, j];
                    }
                }
            }

            //逆行列の計算
            d11 = sd * sf - se * se;
            d12 = se * sc - sb * sf;
            d13 = sb * se - sc * sd;
            d21 = sc * se - sb * sf;
            d22 = sa * sf - sc * sc;
            d23 = sb * sc - sa * se;
            d31 = sb * se - sc * sd;
            d32 = sb * sc - sa * se;
            d33 = sa * sd - sb * sb;

            det = sa * d11 + sb * d12 + sc * d13;

            a = (d11 * sg + d12 * sh + d13 * si) / det;
            b = (d21 * sg + d22 * sh + d23 * si) / det;
            c = (d31 * sg + d32 * sh + d33 * si) / det;

            //平面補正
            for (int i = 0; i < intX; i++)
            {
                for (int j = 0; j < intY; j++)
                {
                    Data[i, j] -= (a * i + b * j + c);
                    if (minimum > Data[i, j])
                    {
                        minimum = Data[i, j];
                    }
                }
            }

            //最小値を零にする場合
            if (status == PlaneCorrectionStatus.ZeroMin)
            {
                //最小値を０に
                for (int i = 0; i < intX; i++)
                {
                    for (int j = 0; j < intY; j++)
                    {
                        Data[i, j] -= minimum;
                    }
                }
            }
            //平均値を零にする場合
            else if (status == PlaneCorrectionStatus.ZeroAverage)
            {
                double dbleAve = 0;
                int count = 0;
                for (int i = 0; i < intX; i++)
                    for (int j = 0; j < intY; j++)
                        if (zero[i, j] == 0)
                        {
                            dbleAve += Data[i, j];
                            count++;
                        }
                dbleAve = dbleAve / count;
                //最小値を０に
                for (int i = 0; i < intX; i++)
                    for (int j = 0; j < intY; j++)
                        if (zero[i, j] == 0)
                            Data[i, j] -= dbleAve;
                        else
                            Data[i, j] = 0.0;


            }

            return;
        }


        //平面補正
        //引数：平面補正を行う一次元データ
        public static void PlaneCorrection(PlaneCorrectionStatus status, ref double[] Data)
        {
            double[,] subDouble = new double[2, Data.Length];

            for (int i = 0; i < Data.Length; i++)
            {
                subDouble[0, i] = Data[i];
                subDouble[1, i] = Data[i];
            }

            PlaneCorrection(status, ref subDouble);

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = subDouble[0, i];
            }

            return;
        }

        //平面補正
        //引数：平面補正を行う一次元データ
        public static void PlaneCorrection(PlaneCorrectionStatus status,int[] zero, ref double[] Data)
        {
            double[,] subDouble = new double[2, Data.Length];
            int[,] subZero = new int[2, Data.Length];

            for (int i = 0; i < Data.Length; i++)
            {
                subDouble[0, i] = Data[i];
                subDouble[1, i] = Data[i];
                subZero[0, i] = zero[i];
                subZero[1, i] = zero[i];
            }

            PlaneCorrection(status,subZero, ref subDouble);

            for (int i = 0; i < Data.Length; i++)
            {
                Data[i] = subDouble[0, i];
            }

            return;
        }

        /// <summary>
        /// 平面補正領域指定バージョン
        /// </summary>
        /// <param name="x0">平面補正領域左上Ｘ</param>
        /// <param name="y0">平面補正領域左上Ｙ</param>
        /// <param name="dx">平面補正領域Ｘ幅</param>
        /// <param name="dy">平面補正領域Ｙ幅</param>
        /// <param name="status">零点を最小値にするか平均値にするか</param>
        /// <param name="Data">平面補正データ配列</param>
        public static void PlaneCorrection(double x0, double y0, double dx, double dy, PlaneCorrectionStatus status, ref double[,] Data)
        {
            int intX, intY;
            double minimum = 1.0e128;
            double a, b, c, d11, d12, d13, d21, d22, d23, d31, d32, d33, det;
            double sa, sb, sc, sd, se, sf, sg, sh, si;
            sa = 0.0;
            sb = 0.0;
            sc = 0.0;
            sd = 0.0;
            se = 0.0;
            sf = 0.0;
            sg = 0.0;
            sh = 0.0;
            si = 0.0;

            intX = Data.GetLength(0);
            intY = Data.GetLength(1);

            for (int i = 0; i < intX; i++)
            {
                for (int j = 0; j < intY; j++)
                {
                    //領域内の場合だけ計算
                    if ((x0 <= i) && (i < x0 + dx) && (y0 <= j) && (j < y0 + dy))
                    {
                        sa += Convert.ToDouble(i * i);
                        sb += Convert.ToDouble(i * j);
                        sc += Convert.ToDouble(i);
                        sd += Convert.ToDouble(j * j);
                        se += Convert.ToDouble(j);
                        sf += 1.0;
                        sg += Convert.ToDouble(i) * Data[i, j];
                        sh += Convert.ToDouble(j) * Data[i, j];
                        si += Data[i, j];
                    }
                }
            }

            //逆行列の計算
            d11 = sd * sf - se * se;
            d12 = se * sc - sb * sf;
            d13 = sb * se - sc * sd;
            d21 = sc * se - sb * sf;
            d22 = sa * sf - sc * sc;
            d23 = sb * sc - sa * se;
            d31 = sb * se - sc * sd;
            d32 = sb * sc - sa * se;
            d33 = sa * sd - sb * sb;

            det = sa * d11 + sb * d12 + sc * d13;

            a = (d11 * sg + d12 * sh + d13 * si) / det;
            b = (d21 * sg + d22 * sh + d23 * si) / det;
            c = (d31 * sg + d32 * sh + d33 * si) / det;

            //平面補正
            for (int i = 0; i < intX; i++)
            {
                for (int j = 0; j < intY; j++)
                {
                    Data[i, j] -= (a * i + b * j + c);
                    if (minimum > Data[i, j])
                    {
                        minimum = Data[i, j];
                    }
                }
            }

            //最小値を零にする場合
            if (status == PlaneCorrectionStatus.ZeroMin)
            {
                //最小値を０に
                for (int i = 0; i < intX; i++)
                {
                    for (int j = 0; j < intY; j++)
                    {
                        Data[i, j] -= minimum;
                    }
                }
            }
            //平均値を零にする場合
            else if (status == PlaneCorrectionStatus.ZeroAverage)
            {
                double dbleAve = 0;
                for (int i = 0; i < intX; i++)
                {
                    for (int j = 0; j < intY; j++)
                    {
                        dbleAve += Data[i, j];
                    }
                }
                dbleAve = dbleAve / (Data.GetLength(0) * Data.GetLength(1));
                //最小値を０に
                for (int i = 0; i < intX; i++)
                {
                    for (int j = 0; j < intY; j++)
                    {
                        Data[i, j] -= dbleAve;
                    }
                }
            }

            return;
        }

        //0データの穴埋め補間
        public static void CoverLossData(double[,] inputData, ref double[,] outputData)
        {
            int intX = inputData.GetLength(0);
            int intY = inputData.GetLength(1);
            int intSub = 1;

            //穴埋め実行フラグ
            //bool boolFlag = false;

            for (int i = 0; i < intX; i++)
            {
                for (int j = 0; j < intY; j++)
                {
                    //データが0の場合穴埋めを行う
                    if (Math.Abs(inputData[i, j]) < 1.0e-15)
                    {
                        //中心軸より左側の場合の処理
                        if (i < intX / 2)
                        {
                            //サブ変数の初期化
                            intSub = 1;
                            //boolFlag = false;
                            for (int n = i + 1; n < intX; n++)
                            {
                                if (Math.Abs(inputData[i + intSub, j]) > 1.0e-15)
                                {
                                    //元データに一番近い右端のデータで穴埋めを行う
                                    outputData[i, j] = inputData[i + intSub, j];
                                    //穴埋めを行ったらループを抜け出す。
                                    //boolFlag = true;
                                    break;
                                }
                                else
                                {
                                    intSub++;
                                }

                            }
                        }
                        //中心軸より右側の場合の処理
                        else
                        {
                            //サブ変数の初期化
                            intSub = -1;
                            for (int n = 1; n < i; n++)
                            {
                                if (Math.Abs(inputData[i + intSub, j]) > 1.0e-15)
                                {
                                    //元データに一番近い右端のデータで穴埋めを行う
                                    outputData[i, j] = inputData[i + intSub, j];
                                    //穴埋めを行ったらループを抜け出す。
                                    //boolFlag = true;
                                    break;
                                }
                                else
                                {
                                    intSub--;
                                }
                            }
                        }
                    }
                    //データが0でない場合はそのまま代入
                    else
                    {
                        outputData[i, j] = inputData[i, j];
                    }
                }
            }
        }
        
        /// <summary>
        /// GPIデータの穴埋め補間：上下端データの処理追加バージョン
        /// </summary>
        /// <param name="inputData"></param>
        /// <param name="outputData"></param>
        public static void CoverLossData2(double[,] inputData, ref double[,] outputData)
        {
            int intX = inputData.GetLength(0);
            int intY = inputData.GetLength(1);
            int intSub = 1;

            for (int i = 0; i < intX; i++)
            {
                for (int j = 0; j < intY; j++)
                {
                    //データが0の場合穴埋めを行う
                    if (Math.Abs(inputData[i, j]) < 1.0e-15)
                    {
                        //中心軸より左側の場合の処理
                        if (i < intX / 2)
                        {
                            //サブ変数の初期化
                            intSub = 1;
                            for (int n = i + 1; n < intX; n++)
                            {
                                if (Math.Abs(inputData[i + intSub, j]) > 1.0e-15)
                                {
                                    //元データに一番近い右端のデータで穴埋めを行う
                                    outputData[i, j] = inputData[i + intSub, j];
                                    //穴埋めを行ったらループを抜け出す。
                                    break;
                                }
                                else
                                {
                                    intSub++;
                                }

                            }
                        }
                        //中心軸より右側の場合の処理
                        else
                        {
                            //サブ変数の初期化
                            intSub = -1;
                            for (int n = 1; n < i; n++)
                            {
                                if (Math.Abs(inputData[i + intSub, j]) > 1.0e-15)
                                {
                                    //元データに一番近い右端のデータで穴埋めを行う
                                    outputData[i, j] = inputData[i + intSub, j];
                                    //穴埋めを行ったらループを抜け出す。
                                    break;
                                }
                                else
                                {
                                    intSub--;
                                }
                            }
                        }
                    }
                    //データが0でない場合はそのまま代入
                    else
                    {
                        outputData[i, j] = inputData[i, j];
                    }
                }
            }

            //縦方向の穴埋め
            for (int i = 0; i < intX; i++)
            {
                for (int j = 0; j < intY; j++)
                {
                    if (Math.Abs(outputData[i, j]) < 1.0e-15)
                    {
                        CoverLossDataUpDown(i, j, ref outputData);
                    }
                }
            }

        }

        //縦横方向に欠損データを埋め合わせ
        private static void CoverLossDataUpDown(int intColX, int intRowY, ref double[,] dbleData)
        {
            int intX = dbleData.GetLength(0);
            int intY = dbleData.GetLength(1);
            int intSub = 1;

            if (intRowY < intY / 2)
            {
                intSub = 1;
                for (int n = intRowY + 1; n < intY; n++)
                {
                    if (Math.Abs(dbleData[intColX, intRowY + intSub]) > 1.0e-15)
                    {
                        //元データに一番近い右端のデータで穴埋めを行う
                        dbleData[intColX, intRowY] = dbleData[intColX, intRowY + intSub];
                        //穴埋めを行ったらループを抜け出す。
                        break;
                    }
                    else
                    {
                        intSub++;
                    }
                }
            }
            else
            {
                intSub = -1;
                for (int n = intRowY - 1; n >= 0; n--)
                {
                    if (Math.Abs(dbleData[intColX, intRowY + intSub]) > 1.0e-15)
                    {
                        //元データに一番近い右端のデータで穴埋めを行う
                        dbleData[intColX, intRowY] = dbleData[intColX, intRowY + intSub];
                        //穴埋めを行ったらループを抜け出す。
                        break;
                    }
                    else
                    {
                        intSub--;
                    }
                }
            }
        }


    }
}
