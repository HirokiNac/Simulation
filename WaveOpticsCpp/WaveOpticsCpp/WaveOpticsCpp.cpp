// WaveOpticsCpp.cpp : DLL アプリケーション用にエクスポートされる関数を定義します。
//
#define SIMD

#include "stdafx.h"
#include "WaveOpticsCpp.h"
#include <math.h>

void WaveOpticsCpp::Prop1D(const double _lambda, const int _dir,
	const int _n1, const double* _x1, const double* _y1, const double* _u1re, const double* _u1im,
	const int _n2, const double* _x2, const double* _y2, double* _u2re, double* _u2im)
{
	double k = 2.0*PI / _lambda;

	double r, rx, ry, rr;
	double tr, ti;
	double tur, tui;
	double ur = 0.0, ui = 0.0;

	for (int i = 0; i < _n2; i++)
	{
		for (int j = 0; j < _n1; j++)
		{
			rx = _x2[i] - _x1[j];
			ry = _y2[i] - _y1[j];
			r = sqrt(rx*rx + ry*ry);

			rr = 1.0 / sqrt(r);
			tr = cos(-k*r) * rr;
			ti = _dir* sin(-k*r) * rr;

			tur = _u1re[j];
			tui = _u1im[j];

			ur = ur + tur*tr - tui*ti;
			ui = ui + tur*ti + tui*tr;
		}
		_u2re[i] = ur;
		_u2im[i] = ui;
	}

}

void WaveOpticsCpp::Prop2D(const double _lambda, const int _dir,
	const int _n1, const double* _x1, const double* _y1, const double* _z1, const double* _u1re, const double* _u1im,
	const int _n2, const  double* _x2, const double* _y2, const double* _z2, double* _u2re, double* _u2im)
{
	double k = 2.0*PI / _lambda;


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
			r = sqrt(rx*rx + ry*ry + rz*rz);

			rr = 1.0 / sqrt(r);
			tr = cos(-k*r) * rr;
			ti = _dir* sin(-k*r) * rr;

			tur = _u1re[j];
			tui = _u1im[j];

			ur = ur + tur*tr - tui*ti;
			ui = ui + tur*ti + tui*tr;
		}
		_u2re[i] = ur;
		_u2im[i] = ui;
	}

}

void WaveOpticsCpp::Prop2D(const double _lambda, const int _dir, 
	const int _n1, const double * _x1, const double * _y1, const double * _z1, const double * _u1re, const double * _u1im,const double * _ds1, 
	const int _n2, const double * _x2, const double * _y2, const double * _z2, double * _u2re, double * _u2im)
{
	double k = 2.0 * PI / _lambda;



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
			r = sqrt(rx*rx + ry * ry + rz * rz);

			rr = 1.0 / sqrt(r);
			tr = cos(-k * r) * rr;
			ti = _dir * sin(-k * r) * rr;

			tur = _u1re[j];
			tui = _u1im[j];

			ur = ur + _ds1[j] * (tur * tr - tui * ti);
			ui = ui + _ds1[j] * (tur * ti + tui * tr);
		}
		_u2re[i] = ur;
		_u2im[i] = ui;
	}


}

void WaveOpticsCpp::CalcSpace(const int _nCol, const int _nRow,
	const double* _x1, const double* _y1, const double* _z1, double* _ds)
{
	for (int i = 1; i < _nCol-1; i++)
	{
		for (int j = 1; j < _nRow - 1; j++)
		{
			double dx = _x1[i * _nRow + j + 1] - _x1[i * _nRow + j - 1];
			double dzx = _z1[i * _nRow + j + 1] - _z1[i * _nRow + j - 1];

			double dy = _y1[(i + 1) * _nRow + j] - _y1[(i - 1) * _nRow + j];
			double dzy = _z1[(i + 1) * _nRow + j] - _z1[(i - 1) * _nRow + j];

			_ds[i] = sqrt(pow(dx, 2.0) + pow(dzx, 2.0)) * sqrt(pow(dy, 2.0) + pow(dzy, 2.0));
		}
	}

}

