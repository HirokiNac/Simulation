// cppWaveOptics.h

#pragma once
#include <omp.h>

using System::Numerics::Complex;
using System::Math;

namespace ClsNac {

	public ref class cppWaveOptics
	{
	private:
		static const double PI = 3.1415926535897932384626433832795;
		int Core = 0;
		double lambda;
		double k;

	public:
		/// <summary>
		/// 
		/// </summary>
		/// <param name="_lambda"></param>
		/// <param name="_Core">0:AllProcs</param>
		cppWaveOptics(double _lambda,int _Core)
		{
			if (_Core == 0 || _Core > omp_get_num_procs())
				Core = omp_get_num_procs();
			else
				Core = _Core;

			lambda = _lambda;
			k = 2.0*PI / _lambda;
		}
		~cppWaveOptics()
		{
			this->!cppWaveOptics();
		}

		!cppWaveOptics()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_dir">FW:-1 BW:1</param>
		/// <param name="_x1"></param>
		/// <param name="_y1"></param>
		/// <param name="_u1"></param>
		/// <param name="_x2"></param>
		/// <param name="_y2"></param>
		/// <param name="_u2"></param>
		void Propagate1D(int _dir,
			 array<double>^ _x1, array<double>^ _y1, array<Complex>^ _u1,
			 array<double>^ _x2, array<double>^ _y2, array<Complex>^ _u2);

		void Propagate1D(int _dir,
			array<double>^ _x1, array<double>^ _y1, array<double>^ _u1r, array<double>^ _u1i,
			array<double>^ _x2, array<double>^ _y2, array<double>^ _u2r, array<double>^_u2i);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_dir">FW:-1 BW:1</param>
		/// <param name="_x1"></param>
		/// <param name="_y1"></param>
		/// <param name="_z1"></param>
		/// <param name="_u1"></param>
		/// <param name="_x2"></param>
		/// <param name="_y2"></param>
		/// <param name="_z2"></param>
		/// <param name="_u2"></param>
		void Propagate2D( int _dir,
			 array<double>^_x1,  array<double>^_y1,  array<double>^_z1,  array<Complex>^_u1,
			 array<double>^_x2,  array<double>^_y2,  array<double>^_z2, array<Complex>^_u2);

	};
}
