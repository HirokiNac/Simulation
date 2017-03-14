// これは メイン DLL ファイルです。

#include "stdafx.h"

#include "cuWaveOptics.h"

void ClsNac::cuWaveOptics::Propagate1D(double _lambda, int _dir, array<double>^ _x1, array<double>^ _y1, array<Complex>^ _u1, array<double>^ _x2, array<double>^ _y2, array<Complex>^ _u2)
{
}

void ClsNac::cuWaveOptics::Propagate2D(double _lambda, int _dir, array<double>^ _x1, array<double>^ _y1, array<double>^ _z1, array<Complex>^ _u1, array<double>^ _x2, array<double>^ _y2, array<double>^ _z2, array<Complex>^ _u2)
{
	throw gcnew System::NotImplementedException();
}
