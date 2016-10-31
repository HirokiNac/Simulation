// ClsNac.WaveOpticsCLR.h

#pragma once

using System::Numerics::Complex;
using  System::Math;

namespace ClsNac {

	public ref class WaveOpticsCLR
	{
		// TODO: このクラスの、ユーザーのメソッドをここに追加してください。
	private:
		static const double PI = 3.1415926535897932384626433832795;
	public:
		static void Prop1D(double _lambda,int _dir,
			array<double>^ _x1, array<double>^ _y1, array<Complex>^ _u1,
			array<double>^ _x2, array<double>^ _y2, array<Complex>^ _u2);



		static void PropFw2D(double _lambda,
			array<double, 2>^_x1, array<double, 2>^_y1, array<double, 2>^_z1, array<Complex, 2>^_u1,
			array<double, 2>^_x2, array<double, 2>^_y2, array<double, 2>^_z2, array<Complex, 2>^_u2);

		static void Prop2D(double _lambda,
			array<double>^ _x1, array<double>^ _y1, array<double>^ _z1, array<Complex>^ _u1,
			array<double>^ _x2, array<double>^ _y2, array<double>^ _z2, array<Complex>^ _u2);
	};
}
