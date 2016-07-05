#include "WaveOpticsAF.h"
#include<arrayfire.h>
#include<complex>
using namespace af;

void PropFw1D(double _lambda, 
	int _n1, double* _x1, double* _y1, double* _u1re, double* _u1im, 
	int _n2, double* _x2, double* _y2, double* &_u2re, double* &_u2im)
{
	array af_x1 = array(_n1, _x1);
	array af_y1 = array(_n1, _y1);
	array af_u1 = complex(array(_n1, _u1re), array(_n1, _u1im));
	array af_x2 = array(_n2, _x2);
	array af_y2 = array(_n2, _y2);
	array af_u2 = array(_n2, dtype::c64);

	double k = -2.0*PI / _lambda;
	gfor(index i, _n2)
	{
		array xy = sqrt(pow2(af_x2(i) - af_x1) + pow2(af_y2(i) - af_y1));
		//for (int j = 0; j < _n1; j++)
		//{
		af_u2(i) = sum<double>(af_u1*exp(af_cdouble(0.0, 1.0)*k*xy) / xy);
		//}
	}
	_u2re = real(af_u2).host<double>();
	_u2im = imag(af_u2).host<double>();
	
}


