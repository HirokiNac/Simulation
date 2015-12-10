// これは メイン DLL ファイルです。
#include "stdafx.h"
#include <complex>
#include "cppWaveField.h"

using namespace System::Numerics;


void cppWaveField::WF::fProp(double lambda, 
	array<double>^ x1, array<double>^ y1, array<Complex>^ u1, 
	array<double>^ x2, array<double>^ y2, array<Complex>^ u2)
{
	double k = 2.0*3.141592 / lambda;
	//u2 = gcnew array<Complex>(x2->Length);
	for (int i=0; i < u2->Length; i++)
	{
		for (int j = 0; j < u1->Length; j++)
		{
			double xy = Math::Sqrt(Math::Pow(x2[i] - x1[j], 2.0) + Math::Pow(y2[i] - y1[j], 2.0));
			u2[i] += u1[j] * Complex::Exp(-Complex::ImaginaryOne*k*xy) / xy;
		}
	}
}
