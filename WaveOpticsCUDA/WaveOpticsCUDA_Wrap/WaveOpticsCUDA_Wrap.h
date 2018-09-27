// WaveOpticsCUDA_Wrap.h

#pragma once

using namespace System;

namespace ClsNac {

	public ref class WaveOpticsCUDA_Wrap
	{
		// TODO: このクラスの、ユーザーのメソッドをここに追加してください。

	public:
		static void Prop1D(const double _lambda, const int _dir,
			const int _n1, const  array<double>^ _x1, const array<double>^ _y1, const array<double>^ _u1re, const array<double>^_u1im,
			const int _n2, const array<double>^ _x2, const array<double>^ _y2, array<double>^ _u2re, array<double>^_u2im);



		static void Prop2D(const double _lambda, const int _dir,
			const int _n1, const array<double>^ _x1, const array<double>^ _y1, const array<double>^ _z1, const array<double>^ _u1re, const array<double>^ _u1im,
			const int _n2, const array<double>^ _x2, const array<double>^ _y2, const array<double>^ _z2, array<double>^ _u2re, array<double>^ _u2im);

		

	};
}
