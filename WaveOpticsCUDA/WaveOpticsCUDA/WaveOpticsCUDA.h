#pragma once
static const double PI= 3.1415926535897932384626433832795;
extern "C" {
	class WaveOpticsCUDA
	{
		public:
			__declspec(dllexport) static void __cdecl Prop1D(const double _lambda,const int _dir,
				const int _n1, const double* _x1,const double* _y1,const double* _u1re,const double* _u1im,
				const int _n2, const double* _x2,const double* _y2, double* _u2re, double* _u2im);

			__declspec(dllexport) static void __cdecl Prop2D(double _lambda, int _dir,
				int _n1, double* _x1, double* _y1, double* _z1, double* _u1re, double* _u1im,
				int _n2, double* _x2, double* _y2, double* _z2, double* _u2re, double* _u2im);

	};
}