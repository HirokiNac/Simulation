// cuWaveOptics.h

#pragma once

using namespace System;
using System::Numerics::Complex;
using System::Math;

namespace ClsNac{

	public ref class cuWaveOptics 
	{
		// TODO: このクラスの、ユーザーのメソッドをここに追加してください。
	private:
		static const double PI = 3.1415926535897932384626433832795;
	public:

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_lambda"></param>
		/// <param name="_dir"></param>
		/// <param name="_x1"></param>
		/// <param name="_y1"></param>
		/// <param name="_u1"></param>
		/// <param name="_x2"></param>
		/// <param name="_y2"></param>
		/// <param name="_u2"></param>
		static void Propagate1D(double _lambda, int _dir,
			array<double>^ _x1, array<double>^ _y1, array<Complex>^ _u1,
			array<double>^ _x2, array<double>^ _y2, array<Complex>^ _u2);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_lambda"></param>
		/// <param name="_x1"></param>
		/// <param name="_y1"></param>
		/// <param name="_z1"></param>
		/// <param name="_u1"></param>
		/// <param name="_x2"></param>
		/// <param name="_y2"></param>
		/// <param name="_z2"></param>
		/// <param name="_u2"></param>
		static void Propagate2D(double _lambda, int _dir,
			array<double>^_x1, array<double>^_y1, array<double>^_z1, array<Complex>^_u1,
			array<double>^_x2, array<double>^_y2, array<double>^_z2, array<Complex>^_u2);

	};
}
