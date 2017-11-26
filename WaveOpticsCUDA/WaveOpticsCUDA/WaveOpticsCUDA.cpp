#include "WaveOpticsCUDA.h"

extern "C" void
Prop1DCuda(const double _k,const int _dir,
	const int _n1,const double* _x1,const double* _y1,const double* _u1re,const double* _u1im,
	const int _n2,const double* _x2,const double* _y2, double* _u2re, double* _u2im);

extern "C" void
Prop2DCuda(double _k,int _dir,
	int _n1, double* _x1, double* _y1, double* _z1, double* _u1re, double* _u1im,
	int _n2, double* _x2, double* _y2, double* _z2, double* _u2re, double* _u2im);


void WaveOpticsCUDA::Prop1D(const double _lambda,const int _dir,
	const int _n1,const double * _x1,const double * _y1,const double * _u1re,const double * _u1im, 
	const int _n2,const double * _x2,const double * _y2, double * _u2re, double * _u2im)
{
	double k = 2.0*PI / _lambda;
	Prop1DCuda(k,_dir, _n1, _x1, _y1, _u1re, _u1im, _n2, _x2, _y2, _u2re, _u2im);
}

void WaveOpticsCUDA::Prop2D(double _lambda, int _dir, 
	int _n1, double * _x1, double * _y1, double * _z1, double * _u1re, double * _u1im, 
	int _n2, double * _x2, double * _y2, double * _z2, double * _u2re, double * _u2im)
{
	double k = 2.0*PI / _lambda;

	Prop2DCuda(k, _dir, _n1, _x1, _y1, _z1, _u1re, _u1im, _n2, _x2, _y2, _z2, _u2re, _u2im);
}
