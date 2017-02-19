// ClsNac.fftw.h

#pragma once

using namespace System;
using System::Numerics::Complex;

namespace ClsNac {

	public ref class fftw
	{
		// TODO: このクラスの、ユーザーのメソッドをここに追加してください。

	public:
		static void Execute1D(array<Complex>^ _in, array<Complex>^_out, int _sign);
		static void Execute1D(array<double>^ _in, array<Complex>^_out, int _sign);
		static void Execute1D(array<Complex>^ _in, array<double>^_out, int _sign);
		
		static void Execute2D(array<Complex>^ _in, array<Complex>^_out, int _sign, bool _shift);
		static void Execute2D(array<double>^ _in, array<Complex>^_out, int _sign, bool _shift);
		static void Execute2D(array<Complex>^ _in, array<double>^_out, int _sign, bool _shift);
		
		
	};
}
