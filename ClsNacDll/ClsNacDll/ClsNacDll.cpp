// これは メイン DLL ファイルです。

#include "stdafx.h"
#include <complex>
#include "ClsNacDll.h"

using namespace System::Numerics;
//using namespace cli;

void ClsNacDll::ampwf::fp(array<double>^ x, array<double>^ y,array<double>^ z)
{
	const int len = x->Length;
	for (int i = 0; i < len; i++)
		z[i] = x[i] + y[i];
}

void ClsNacDll::ampwf::fp2(array<Complex>^ a)
{
	const int len = a->Length;
	for (int i = 0; i < len; i++)
		a[i] *= 2.0;
}