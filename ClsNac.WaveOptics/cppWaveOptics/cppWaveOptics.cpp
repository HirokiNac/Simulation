// これは メイン DLL ファイルです。

#include "stdafx.h"
#include <omp.h>
#include "cppWaveOptics.h"

void ClsNac::cppWaveOptics::Propagate1D(double _lambda, int _dir,
	array<double>^ _x1, array<double>^ _y1, array<Complex>^ _u1,
	array<double>^ _x2, array<double>^ _y2, array<Complex>^ _u2)
{
	double k = (double)_dir*2.0*PI / _lambda;

	int div1 = _u1->Length;
	int div2 = _u2->Length;
	if (_x1->Length != div1 || _y1->Length != div1
		|| _x2->Length != div2 || _y2->Length != div2)
		return;

#pragma omp parallel for schedule(static)
	for (int i = 0; i < div2; i++)
	{
		for (int j = 0; j < div1; j++)
		{
			double xy = Math::Sqrt(Math::Pow(_x2[i] - _x1[j], 2.0) + Math::Pow(_y2[i] - _y1[j], 2.0));
			_u2[i] += _u1[j] * Complex::Exp(Complex::ImaginaryOne*k*xy) / Math::Sqrt(xy);
		}
	}

}

void ClsNac::cppWaveOptics::Propagate2D(double _lambda, int _dir,
	array<double>^ _x1, array<double>^ _y1, array<double>^_z1, array<Complex>^ _u1,
	array<double>^ _x2, array<double>^ _y2, array<double>^_z2, array<Complex>^ _u2)
{
	double k = 2.0*PI / _lambda;

	int div1 = _u1->Length;
	int div2 = _u2->Length;
	if (_x1->Length != div1 || _y1->Length != div1 || _z1->Length != div1
		|| _x2->Length != div2 || _y2->Length != div2 || _z2->Length != div2)
		return;

#pragma omp parallel for schedule(static)
	for (int i_2 = 0; i_2 < div2; i_2++)
	{
		for (int i_1 = 0; i_1 < div1; i_1++)
		{
			double xyz = Math::Sqrt(
				Math::Pow(_x2[i_2] - _x1[i_1], 2.0) +
				Math::Pow(_y2[i_2] - _y1[i_1], 2.0) +
				Math::Pow(_z2[i_2] - _z1[i_1], 2.0));
			_u2[i_2] += _u1[i_1] * Complex::Exp(-Complex::ImaginaryOne*k*xyz) / xyz;
		}
	}

}

