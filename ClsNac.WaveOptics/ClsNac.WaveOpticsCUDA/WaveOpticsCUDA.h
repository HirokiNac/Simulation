#pragma once
static const double PI = 3.1415926535897932384626433832795;

extern "C" {
	__declspec(dllexport) void PropFw1d(double _lambda,
		int _n1, double* _x1, double* _y1, double* _u1re, double* _u1im,
		int _n2, double* _x2, double* _y2, double* &_u2re, double* &_u2im);

	__declspec(dllexport) void PropFw1d_f(float _lambda,
		int _n1, float* _x1, float* _y1, float* _u1re, float* _u1im,
		int _n2, float* _x2, float* _y2, float* &_u2re, float* &_u2im);

}

