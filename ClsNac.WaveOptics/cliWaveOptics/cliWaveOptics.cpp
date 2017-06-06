// これは メイン DLL ファイルです。

#include "stdafx.h"
#include "cliWaveOptics.h"

void ClsNac::cliWaveOptics::Propagate1D(const int _dir,
	array<double>^ _x1, array<double>^ _y1, array<Complex>^ _u1,
	array<double>^ _x2, array<double>^ _y2, array<Complex>^ %_u2)
{
	int m = _x1->Length;
	int n = _x2->Length;
	if (_y1->Length != m || _u1->Length != m
		|| _y2->Length != n || _u2->Length != n)
		return;

#pragma omp parallel for num_threads(Core) schedule(static)
	for (int i = 0; i < n; i++)
	{
		Complex u = 0.0;
		for (int j = 0; j < m; j++)
		{
			double xy = Math::Sqrt(Math::Pow(_x2[i] - _x1[j], 2.0) + Math::Pow(_y2[i] - _y1[j], 2.0));
			u += _u1[j] * Complex::Exp(_dir*Complex::ImaginaryOne*k*xy) / Math::Sqrt(xy);
		}
		_u2[i] = u;
	}

}

void ClsNac::cliWaveOptics::Propagate1D(int _dir, 
	array<double>^ _x1, array<double>^ _y1, 
	array<double>^ _u1r, array<double>^ _u1i, 
	array<double>^ _x2, array<double>^ _y2, 
	array<double>^ %_u2r, array<double>^ %_u2i)
{
	int m = _x1->Length;
	int n = _x2->Length;
	if (_y1->Length != m || _u1r->Length != m || _u1i->Length != m
		|| _y2->Length != n || _u2r->Length != n || _u2i->Length || n)
		return;

#pragma omp parallel for num_threads(Core) schedule(static)
	for (int i = 0; i < n; i++)
	{
		double ur = 0.0;
		double ui = 0.0;

		for (int j = 0; j < m; j++)
		{
			double rx = _x2[i] - _x1[j];
			double ry = _y2[i] - _y1[j];

			double r = Math::Sqrt(rx*rx + ry*ry);
			double rr = 1.0 / r;

			double tr = Math::Cos(_dir*k*r)*rr;
			double ti = Math::Sin(_dir*k*r)*rr;

			ur += _u1r[j] * tr - _u1i[j] * ti;
			ui += _u1r[j] * ti + _u1i[j] * tr;
		}
		_u2r[i] = ur;
		_u2i[i] = ui;
	}

}

void ClsNac::cliWaveOptics::Propagate2D(const int _dir,
	array<double>^ _x1, array<double>^ _y1, array<double>^_z1, array<Complex>^ _u1,
	array<double>^ _x2, array<double>^ _y2, array<double>^_z2, array<Complex>^ %_u2)
{
	int m = _x1->Length;
	int n = _x2->Length;
	if (_y1->Length != m || _z1->Length != m || _u1->Length != m
		|| _y2->Length != n || _z2->Length != n || _u2->Length != n)
		return;

#pragma omp parallel for num_threads(Core) schedule(static)
	for (int j = 0; j < n; j++)
	{
		for (int i = 0; i < m; i++)
		{
			double xyz = Math::Sqrt(
				Math::Pow(_x2[j] - _x1[i], 2.0) +
				Math::Pow(_y2[j] - _y1[i], 2.0) +
				Math::Pow(_z2[j] - _z1[i], 2.0));
			_u2[j] += _u1[i] * Complex::Exp(_dir*Complex::ImaginaryOne*k*xyz) / xyz;
		}
	}

}

void ClsNac::cliWaveOptics::Propagate2D(int _dir,
	array<double, 2>^_x1, array<double, 2>^_y1, array<double, 2>^_z1, array<Complex, 2>^_u1,
	array<double, 2>^_x2, array<double, 2>^_y2, array<double, 2>^_z2, array<Complex, 2>^ %_u2)
{
	int m1 = _x1->GetLength(0);
	int m2 = _x1->GetLength(1);
	int m = m1*m2;
	int n1 = _x2->GetLength(0);
	int n2 = _x2->GetLength(1);
	int n = n1*n2;

	if (_y1->GetLength(0) != m1 || _y1->GetLength(1) != m2
		|| _z1->GetLength(0) != m1 || _z1->GetLength(1) != m2
		|| _u1->GetLength(0) != m1 || _u1->GetLength(1) != m2
		|| _y2->GetLength(0) != n1 || _y2->GetLength(1) != n2
		|| _z2->GetLength(0) != n1 || _z2->GetLength(1) != n2
		|| _u2->GetLength(0) != n1 || _u2->GetLength(1) != n2)
		return;

#pragma omp parallel for num_threads(Core) schedule(static)
	for (int j = 0; j < n; j++)
	{
		int j1 = j / n1;
		int j2 = j%n1;
		Complex u2 = 0.0;
		for (int i = 0; i < m; i++)
		{
			int i1 = i / m1;
			int i2 = i%m1;

			double xyz = Math::Sqrt(
				Math::Pow(_x2[j1, j2] - _x1[i1, i2], 2.0) +
				Math::Pow(_y2[j1, j2] - _y1[i1, i2], 2.0) +
				Math::Pow(_z2[j1, j2] - _z1[i1, i2], 2.0));
			u2 += _u1[i1, i2] * Complex::Exp(_dir*Complex::ImaginaryOne*k*xyz) / xyz;
		}
		_u2[j1, j2] = u2;
	}
}

void ClsNac::cliWaveOptics::Propagate2D(const int _dir,
	array<double>^ _x1, array<double>^ _y1, array<double>^ _z1,
	array<double>^ _u1r, array<double>^ _u1i,
	array<double>^ _x2, array<double>^ _y2, array<double>^ _z2,
	array<double>^ %_u2r,  array<double>^ %_u2i)
{
	int m = _x1->Length;
	int n = _x2->Length;
	if (_y1->Length != m || _z1->Length != m || _u1r->Length != m || _u1i->Length != m
		|| _y2->Length != n || _z2->Length != n || _u2r->Length != n || _u2i->Length != n)
		return;

#pragma omp parallel for num_threads(Core) schedule(static)
	for (int i = 0; i < n; i++)
	{
		double ur = 0.0;
		double ui = 0.0;

		for (int j = 0; j < m; j++)
		{
			double rx = _x2[i] - _x1[j];
			double ry = _y2[i] - _y1[j];
			double rz = _z2[i] - _z1[j];

			double r = Math::Sqrt(rx*rx + ry*ry + rz*rz);
			double rr = 1.0 / r;

			double tr = Math::Cos(_dir*k*r)*rr;
			double ti = Math::Sin(_dir*k*r)*rr;

			ur += _u1r[j] * tr - _u1i[j] * ti;
			ui += _u1r[j] * ti + _u1i[j] * tr;
		}
		_u2r[i] = ur;
		_u2i[i] = ui;
	}

}

void ClsNac::cliWaveOptics::Propagate2D(int _dir,
	array<double, 2>^_x1, array<double, 2>^_y1, array<double, 2>^_z1,
	array<double, 2>^_u1r, array<double, 2>^_u1i,
	array<double, 2>^_x2, array<double, 2>^_y2, array<double, 2>^_z2,
	array<double, 2>^ %_u2r, array<double, 2>^ %_u2i)
{
	int m1 = _x1->GetLength(0);
	int m2 = _x1->GetLength(1);
	int m = m1*m2;
	int n1 = _x2->GetLength(0);
	int n2 = _x2->GetLength(1);
	int n = n1*n2;

	if (_y1->GetLength(0) != m1 || _y1->GetLength(1) != m2
		|| _z1->GetLength(0) != m1 || _z1->GetLength(1) != m2
		|| _u1r->GetLength(0) != m1 || _u1r->GetLength(1) != m2
		|| _u1i->GetLength(0) != m1 || _u1i->GetLength(1) != m2
		|| _y2->GetLength(0) != n1 || _y2->GetLength(1) != n2
		|| _z2->GetLength(0) != n1 || _z2->GetLength(1) != n2
		|| _u2r->GetLength(0) != n1 || _u2r->GetLength(1) != n2
		|| _u2i->GetLength(0) != n1 || _u2i->GetLength(1) != n2)
		return;

#pragma omp parallel for num_threads(Core) schedule(static)
	for (int j = 0; j < n; j++)
	{
		int j1 = j / n1;
		int j2 = j%n1;
		double ur = 0.0;
		double ui = 0.0;

		for (int i = 0; i < m; i++)
		{
			int i1 = i / m1;
			int i2 = i%m1;

			double rx = _x2[j1, j2] - _x1[i1, i2];
			double ry = _y2[j1, j2] - _y1[i1, i2];
			double rz = _z2[j1, j2] - _z1[i1, i2];

			double r = Math::Sqrt(rx*rx + ry*ry + rz*rz);
			double rr = 1.0 / r;

			double tr = Math::Cos(_dir*k*r)*rr;
			double ti = Math::Sin(_dir*k*r)*rr;

			ur += (_u1r[i1, i2] * tr - _u1i[i1, i2] * ti);
			ui += (_u1r[i1, i2] * ti + _u1i[i1, i2] * tr);
		}
		_u2r[j1, j2] = ur;
		_u2i[j1, j2] = ui;
	}




}
