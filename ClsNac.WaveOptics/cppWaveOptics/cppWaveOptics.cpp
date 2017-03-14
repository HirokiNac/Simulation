// これは メイン DLL ファイルです。

#include "stdafx.h"
#include "cppWaveOptics.h"

void ClsNac::cppWaveOptics::Propagate1D(int _dir,
	array<double>^ _x1, array<double>^ _y1, array<Complex>^ _u1,
	array<double>^ _x2, array<double>^ _y2, array<Complex>^ _u2)
{
	int div1 = _u1->Length;
	int div2 = _u2->Length;
	if (_x1->Length != div1 || _y1->Length != div1
		|| _x2->Length != div2 || _y2->Length != div2)
		return;

#pragma omp parallel for private(i) num_threads(Core) schedule(static)
	for (int i = 0; i < div2; i++)
	{
		Complex u = 0.0;
		for (int j = 0; j < div1; j++)
		{
			double xy = Math::Sqrt(Math::Pow(_x2[i] - _x1[j], 2.0) + Math::Pow(_y2[i] - _y1[j], 2.0));
			u += _u1[j] * Complex::Exp(_dir*Complex::ImaginaryOne*k*xy) / Math::Sqrt(xy);
		}
		_u2[i] = u;
	}

}

void ClsNac::cppWaveOptics::Propagate1D(int _dir, array<double>^ _x1, array<double>^ _y1, array<double>^ _u1r, array<double>^ _u1i, array<double>^ _x2, array<double>^ _y2, array<double>^ _u2r, array<double>^ _u2i)
{
	int div1 = _x1->Length;
	int div2 = _x2->Length;
	if (_y1->Length != div1 || _u1r->Length != div1 || _u1i->Length != div1 || _y2->Length != div2)
		return;

#pragma omp parallel for private(i) num_threads(Core) schedule(static)
	for (int i = 0; i < div2; i++)
	{
		double ur = 0.0;
		double ui = 0.0;

		for (int j = 0; j < div1; j++)
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

void ClsNac::cppWaveOptics::Propagate2D( int _dir,
	array<double>^ _x1, array<double>^ _y1, array<double>^_z1, array<Complex>^ _u1,
	array<double>^ _x2, array<double>^ _y2, array<double>^_z2, array<Complex>^ _u2)
{
	int div1 = _u1->Length;
	int div2 = _u2->Length;
	if (_x1->Length != div1 || _y1->Length != div1 || _z1->Length != div1
		|| _x2->Length != div2 || _y2->Length != div2 || _z2->Length != div2)
		return;

#pragma omp parallel for private(i) num_threads(Core) schedule(static)
	for (int j = 0; j < div2; j++)
	{
		for (int i = 0; i < div1; i++)
		{
			double xyz = Math::Sqrt(
				Math::Pow(_x2[j] - _x1[i], 2.0) +
				Math::Pow(_y2[j] - _y1[i], 2.0) +
				Math::Pow(_z2[j] - _z1[i], 2.0));
			_u2[j] += _u1[i] * Complex::Exp(_dir*Complex::ImaginaryOne*k*xyz) / xyz;
		}
	}

}

