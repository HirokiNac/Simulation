#pragma once
#include<string>

#include <iostream>
#include <string>
#include <fstream>
#include <sstream>
#include <vector>
using namespace std;
static const double PI = 3.1415926535897932384626433832795;

class WaveOpticsCppCUI
{
	vector<vector<double>>GetData(string _fileName);
	vector<double>GetData1d(string _fileName);
	vector<double>GetData2d(string _fileName, int& _col, int& _row);
	void SetData1d(string _fileName, double* _data, int _n);
	void SetData2d(string _fileName, double* _data, int _n, int _m);
	void Prop2D(const double _lambda, const int _dir,
		const int _n1, const double* _x1, const double* _y1, const double* _z1, const double* _u1re, const double* _u1im,
		const int _n2, const  double* _x2, const double* _y2, const double* _z2, double* _u2re, double* _u2im);
};
