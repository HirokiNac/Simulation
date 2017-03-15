// cppWaveField.h

#pragma once

using namespace System;
using namespace System::Numerics;

namespace ClsNac{

	public ref class WF1D
	{
		// TODO: このクラスの、ユーザーのメソッドをここに追加してください。
	private:
		static const double PI = 3.1415926535897932384626433832795;
		

	public:

		static void fProp(const double lambda,
			array<double>^x1, array<double>^y1, array<Complex>^u1, 
			array<double>^x2, array<double>^y2, array<Complex>^u2);

		static int getProcessor();
	};
}
