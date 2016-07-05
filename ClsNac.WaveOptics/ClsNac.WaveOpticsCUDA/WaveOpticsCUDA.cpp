#include "WaveOpticsCUDA.h"
#include <stdlib.h>
#include <iostream>

extern "C" void PropFw1dCuda(double _k,
	int _n1, double* _x1, double* _y1, double* _u1re, double* _u1im,
	int _n2, double* _x2, double* _y2, double* &_u2re, double* &_u2im);

extern "C" void PropFw1dCuda_f(float _k,
	int _n1, float* _x1, float* _y1, float* _u1re, float* _u1im,
	int _n2, float* _x2, float* _y2, float* &_u2re, float* &_u2im);

void PropFw1d(double _lambda,
	int _n1, double* _x1, double* _y1, double* _u1re, double* _u1im,
	int _n2, double* _x2, double* _y2, double* &_u2re, double* &_u2im)
{
	double k = 2.0*PI / _lambda;

	PropFw1dCuda(k, _n1, _x1, _y1, _u1re, _u1im, _n2, _x2, _y2, _u2re, _u2im);
}

void PropFw1d_f(double _lambda,
	int _n1, float* _x1, float* _y1, float* _u1re, float* _u1im,
	int _n2, float* _x2, float* _y2, float* &_u2re, float* &_u2im)
{
	double k = 2.0*PI / _lambda;

	PropFw1dCuda_f(k, _n1, _x1, _y1, _u1re, _u1im, _n2, _x2, _y2, _u2re, _u2im);
}

