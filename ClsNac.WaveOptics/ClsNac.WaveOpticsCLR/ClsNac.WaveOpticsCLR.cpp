// これは メイン DLL ファイルです。

#include "stdafx.h"
#include <omp.h>
#include "ClsNac.WaveOpticsCLR.h"

using ClsNac::WaveOpticsCLR;

void ClsNac::WaveOpticsCLR::Prop1D(double _lambda, int _dir,
	array<double>^ _x1, array<double>^ _y1, array<Complex>^ _u1,
	array<double>^ _x2, array<double>^ _y2, array<Complex>^ _u2)
{
	double k = (double)_dir*2.0*PI / _lambda;

	int div1 = _u1->Length;
	int div2 = _u2->Length;

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



void ClsNac::WaveOpticsCLR::PropFw2D(double _lambda,
	array<double, 2>^ _x1, array<double, 2>^ _y1, array<double, 2>^_z1, array<Complex, 2>^ _u1,
	array<double, 2>^ _x2, array<double, 2>^ _y2, array<double, 2>^_z2, array<Complex, 2>^ _u2)
{
	double k = 2.0*PI / _lambda;

	int div1x = _u1->GetLength(0);
	int div1y = _u1->GetLength(1);
	int div2x = _u2->GetLength(0);
	int div2y = _u2->GetLength(1);

#pragma omp parallel for schedule(static)
	for (int i_2x = 0; i_2x < div2x; i_2x++)
	{
		for (int i_1x = 0; i_1x < div1x; i_1x++)
		{
			for (int i_2y = 0; i_2y < div2y; i_2y++)
			{
				for (int i_1y = 0; i_1y < div1y; i_1y++)
				{
					double xyz = Math::Sqrt(
						Math::Pow(_x2[i_2x, i_2y] - _x1[i_1x, i_1y], 2.0) + 
						Math::Pow(_y2[i_2x, i_2y] - _y1[i_1x, i_1y], 2.0) + 
						Math::Pow(_z2[i_2x, i_2y] - _z1[i_1x, i_1y], 2.0));
					_u2[i_2x, i_2y] += _u1[i_1x, i_1y] * Complex::Exp(-Complex::ImaginaryOne*k*xyz) / xyz;
				}
			}
		}
	}

}


void ClsNac::WaveOpticsCLR::Prop2D(double _lambda,
	array<double>^ _x1, array<double>^ _y1, array<double>^_z1, array<Complex>^ _u1,
	array<double>^ _x2, array<double>^ _y2, array<double>^_z2, array<Complex>^ _u2)
{
	double k = 2.0*PI / _lambda;

	int div1 = _u1->Length;
	int div2 = _u2->Length;

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

