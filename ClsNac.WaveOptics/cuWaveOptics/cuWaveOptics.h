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
		double lambda;
		double k;
		float k_f;

	public:

		cuWaveOptics(double _lambda)
		{
			lambda = _lambda;
			k = 2.0*PI / _lambda;
			k_f = (float)k;
		}

		~cuWaveOptics()
		{

		}
		!cuWaveOptics()
		{

		}


		void getQuery();

		void setDevice(array<int>^ _dev)
		{

		}

		void getDevice();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_dir"></param>
		/// <param name="_x1"></param>
		/// <param name="_y1"></param>
		/// <param name="_u1r"></param>
		/// <param name="_u1i"></param>
		/// <param name="_x2"></param>
		/// <param name="_y2"></param>
		/// <param name="_u2r"></param>
		/// <param name="_u2i"></param>
		void Propagate1D(int _dir,
			int _n1, array<double>^ _x1, array<double>^ _y1, array<double>^ _u1r, array<double>^ _u1i,
			int _n2, array<double>^ _x2, array<double>^ _y2, array<double>^ _u2r, array<double>^ _u2i);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_dir"></param>
		/// <param name="_x1"></param>
		/// <param name="_y1"></param>
		/// <param name="_u1r"></param>
		/// <param name="_u1i"></param>
		/// <param name="_x2"></param>
		/// <param name="_y2"></param>
		/// <param name="_u2r"></param>
		/// <param name="_u2i"></param>
		void Propagate1D(int _dir,
			int _n1, array<float>^ _x1, array<float>^ _y1, array<float>^ _u1r, array<float>^ _u1i,
			int _n2, array<float>^ _x2, array<float>^ _y2, array<float>^ _u2r, array<float>^ _u2i);

	};


}
