// WaveOpticsCppCUI.cpp : このファイルには 'main' 関数が含まれています。プログラム実行の開始と終了がそこで行われます。
//

#include <iostream>
#include <string>
#include <fstream>
#include <sstream>
#include <vector>
#include <iomanip>
//#include "WaveOpticsCpp.h"
#include "WaveOpticsCppCUI.h"
#include <time.h>
#include <mathimf.h>

using namespace std;

	vector<vector<double>>GetData(string _fileName)
	{
		ifstream ifs;
		ifs.open(_fileName, ios::in);

		string str_buf;

		double num;
		char comma;

		vector<vector<double>> data;

		while (getline(ifs, str_buf))
		{
			if (str_buf.size() == 0)break;
			vector<double>tmp;
			istringstream iss(str_buf);

			while (iss >> num)
			{
				tmp.push_back(num);
				iss >> comma;
			}
			data.push_back(tmp);
		}

		return data;
	}

	vector<double>GetData1d(string _fileName)
	{
		ifstream ifs;
		ifs.open(_fileName, ios::in);

		string str_buf;

		double num;
		char comma;

		vector<double> data;

		while (getline(ifs, str_buf))
		{
			if (str_buf.size() == 0)break;
			istringstream iss(str_buf);

			while (iss >> num)
			{
				data.push_back(num);
				iss >> comma;
			}
		}
		cout << "Load: \"" << _fileName << "\"\n";
		return data;
	}

	vector<double>GetData2d(string _fileName, int& _col, int& _row)
	{
		ifstream ifs;
		ifs.open(_fileName, ios::in);

		string str_buf;

		double num;
		char comma;

		vector<double> data;
		_col = 0;
		_row = 0;

		while (getline(ifs, str_buf))
		{
			if (str_buf.size() == 0)break;
			istringstream iss(str_buf);
			_col++;
			while (iss >> num)
			{
				data.push_back(num);
				iss >> comma;

				if (_col == 1)
					_row++;
			}

		}
		cout << "Load: \"" << _fileName << "\"\n";
		return data;
	}

	void SetData1d(string _fileName, double* _data, int _n)
	{
		ofstream ofs(_fileName);
		for (int i = 0; i < _n; i++)
		{
			ofs << setprecision(15) << _data[i] << std::endl;
		}
	}

	void SetData2d(string _fileName, double* _data, int _n, int _m)
	{
		ofstream ofs(_fileName);
		for (int i = 0; i < _n; i++)
		{
			for (int j = 0; j < _m; j++)
			{
				ofs << setprecision(15) << _data[i * _m + j] << ",";
			}
			ofs << std::endl;
		}
	}

	void Prop2D(const double _lambda, const int _dir,
		const int _n1, const double* _x1, const double* _y1, const double* _z1, const double* _u1re, const double* _u1im,
		const int _n2, const  double* _x2, const double* _y2, const double* _z2, double* __restrict _u2re, double* __restrict _u2im)
	{
		//表示用
		//5％ごとに表示
		int divider = _n2 / 1000;
		int count = 0;
		//
		printf("0                   100\n|--------------------|");

		double k = 2.0 * PI / _lambda;

#pragma ivdep loop count min(1024)
		for (int i = 0; i < _n2; i++)
		{
			double r, rx, ry, rz, rr;
			double tr, ti;
			double tur, tui;
			double ur = 0.0, ui = 0.0;

			for (int j = 0; j < _n1; j++)
			{
				rx = _x2[i] - _x1[j];
				ry = _y2[i] - _y1[j];
				rz = _z2[i] - _z1[j];
				r = sqrt(rx * rx + ry * ry + rz * rz);

				rr = 1.0 / sqrt(r);
				tr = cos(-k * r) * rr;
				ti = _dir * sin(-k * r) * rr;

				tur = _u1re[j];
				tui = _u1im[j];

				ur = ur + tur * tr - tui * ti;
				ui = ui + tur * ti + tui * tr;
			}

			_u2re[i] = ur;
			_u2im[i] = ui;


		}


		printf("\r|====================|");
	}

	int main()
	{
		const string dirPath = "D:\\Source\\Repos\\Simulation\\WaveOpticsCppCUI\\x64\\Debug\\";
		
		//座標読込
		printf("Load Wavefield1\r\n");
		int source_col = 0;
		int source_row = 0;
		vector<double>source_x;
		vector<double>source_y;
		vector<double>source_z;
		vector<double>source_re;
		vector<double>source_im;
#pragma parallel
		{
			source_x = GetData2d(dirPath + "mirror1_x.txt", source_col, source_row);
			source_y = GetData1d(dirPath + "mirror1_y.txt");
			source_z = GetData1d(dirPath + "mirror1_z.txt");
			source_re = GetData1d(dirPath + "mirror1_re.txt");
			source_im = GetData1d(dirPath + "mirror1_im.txt");
		}
		int source_n = source_col * source_row;

		//座標読込
		printf("Load Wavefield2\r\n");
		int mirror_col = 0;
		int mirror_row = 0;
		vector<double>mirror_x;
		vector<double>mirror_y;
		vector<double>mirror_z;
#pragma parallel
		{
			mirror_x = GetData2d(dirPath + "mirror2_x.txt", mirror_col, mirror_row);
			mirror_y = GetData1d(dirPath + "mirror2_y.txt");
			mirror_z = GetData1d(dirPath + "mirror2_z.txt");
		}
		int mirror_n = mirror_col * mirror_row;
		double* mirror_re = new double[mirror_n];
		double* mirror_im = new double[mirror_n];

		//clock_t start = clock();
		//Propagation
		Prop2D(1.25e-10, -1,
			source_n, &source_x[0], &source_y[0], &source_z[0], &source_re[0], &source_im[0],
			mirror_n, &mirror_x[0], &mirror_y[0], &mirror_z[0], mirror_re, mirror_im);
		//clock_t end = clock();
		//const double time = static_cast<double>(end - start) / CLOCKS_PER_SEC * 1000.0;
		//printf("time %lf[ms]\n", time);

		double* mirror_intens = new double[mirror_n];
#pragma loop count min(32)
		for (int i = 0; i < mirror_n; i++)
		{
			mirror_intens[i] = sqrt(mirror_re[i] * mirror_re[i] + mirror_im[i] * mirror_im[i]);
		}

#pragma parallel
		{
			SetData2d(dirPath + "mirror2_re.txt", &mirror_re[0], mirror_col, mirror_row);
			SetData2d(dirPath + "mirror2_im.txt", &mirror_im[0], mirror_col, mirror_row);
			SetData2d(dirPath + "mirror2_intens.txt", &mirror_intens[0], mirror_col, mirror_row);
		}
	}





// プログラムの実行: Ctrl + F5 または [デバッグ] > [デバッグなしで開始] メニュー
// プログラムのデバッグ: F5 または [デバッグ] > [デバッグの開始] メニュー

// 作業を開始するためのヒント: 
//    1. ソリューション エクスプローラー ウィンドウを使用してファイルを追加/管理します 
//   2. チーム エクスプローラー ウィンドウを使用してソース管理に接続します
//   3. 出力ウィンドウを使用して、ビルド出力とその他のメッセージを表示します
//   4. エラー一覧ウィンドウを使用してエラーを表示します
//   5. [プロジェクト] > [新しい項目の追加] と移動して新しいコード ファイルを作成するか、[プロジェクト] > [既存の項目の追加] と移動して既存のコード ファイルをプロジェクトに追加します
//   6. 後ほどこのプロジェクトを再び開く場合、[ファイル] > [開く] > [プロジェクト] と移動して .sln ファイルを選択します
