// ClsNac.fftw.h

#pragma once
#include<complex.h>
#include<fftw3.h>

using namespace System;
using System::Numerics::Complex;

namespace ClsNac {

	public ref class fftw
	{
		// TODO: このクラスの、ユーザーのメソッドをここに追加してください。

	public:

		/// <summary>
		/// 1DFFT
		/// </summary>
		/// <param name="_in"></param>
		/// <param name="_out"></param>
		/// <param name="_dir">FFT:-1,iFFT:1</param>
		static void Execute1D(array<Complex>^ _in, array<Complex>^_out, int _dir);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_in"></param>
		/// <param name="_out"></param>
		static void Forward1D(array<double>^ _in, array<Complex>^_out);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_in"></param>
		/// <param name="_out"></param>
		static void Backward1D(array<Complex>^ _in, array<double>^_out);
		
		static void Execute2D(array<Complex,2>^ _in, array<Complex,2>^_out, int _dir, bool _shift);
		static void Forward2D(array<double,2>^ _in, array<Complex,2>^_out,bool _shift);
		static void Backward2D(array<Complex,2>^ _in, array<double,2>^_out, bool _shift);
		
		
	};
}
