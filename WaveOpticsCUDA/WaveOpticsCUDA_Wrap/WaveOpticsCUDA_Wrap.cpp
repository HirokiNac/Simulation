// これは メイン DLL ファイルです。

#include "stdafx.h"
#include "WaveOpticsCUDA.h"
#include "WaveOpticsCUDA_Wrap.h"



void ClsNac::WaveOpticsCUDA_Wrap::Prop1D(const double _lambda, const int _dir,
	const int _n1, const array<double>^ _x1, const array<double>^ _y1, const array<double>^ _u1re, const array<double>^ _u1im,
	const int _n2, const array<double>^ _x2, const array<double>^ _y2, array<double>^ _u2re, array<double>^ _u2im)
{
	pin_ptr<double> ptr_x1 = &_x1[0];
	pin_ptr<double> ptr_y1 = &_y1[0];
	pin_ptr<double> ptr_u1re = &_u1re[0];
	pin_ptr<double> ptr_u1im = &_u1im[0];
	pin_ptr<double> ptr_x2 = &_x2[0];
	pin_ptr<double> ptr_y2 = &_y2[0];
	pin_ptr<double> ptr_u2re = &_u2re[0];
	pin_ptr<double> ptr_u2im = &_u2im[0];

	WaveOpticsCUDA::Prop1D(_lambda, _dir,
		_n1, ptr_x1, ptr_y1, ptr_u1re, ptr_u1im,
		_n2, ptr_x2, ptr_y2, ptr_u2re, ptr_u2im);
	
	ptr_x1 = nullptr;
	ptr_y1 = nullptr;
	ptr_u1re = nullptr;
	ptr_u1im = nullptr;
	ptr_x2 = nullptr;
	ptr_y2 = nullptr;
	ptr_u2re = nullptr;
	ptr_u2im = nullptr;
}

void ClsNac::WaveOpticsCUDA_Wrap::Prop2D(const double _lambda,const int _dir,
	const int _n1,const array<double>^ _x1,const array<double>^ _y1,const array<double>^ _z1,const array<double>^ _u1re,const array<double>^ _u1im,
	const int _n2,const array<double>^ _x2,const array<double>^ _y2,const array<double>^ _z2, array<double>^ _u2re, array<double>^ _u2im)
{
	pin_ptr<double> ptr_x1 = &_x1[0];
	pin_ptr<double> ptr_y1 = &_y1[0];
	pin_ptr<double> ptr_z1 = &_z1[0];
	pin_ptr<double> ptr_u1re = &_u1re[0];
	pin_ptr<double> ptr_u1im = &_u1im[0];
	pin_ptr<double> ptr_x2 = &_x2[0];
	pin_ptr<double> ptr_y2 = &_y2[0];
	pin_ptr<double> ptr_z2 = &_z2[0];
	pin_ptr<double> ptr_u2re = &_u2re[0];
	pin_ptr<double> ptr_u2im = &_u2im[0];

	WaveOpticsCUDA::Prop2D(_lambda, _dir,
		_n1, ptr_x1, ptr_y1, ptr_z1, ptr_u1re, ptr_u1im,
		_n2, ptr_x2, ptr_y2, ptr_z2, ptr_u2re, ptr_u2im);
	
	ptr_x1 = nullptr;
	ptr_y1 = nullptr;
	ptr_z1 = nullptr;
	ptr_u1re = nullptr;
	ptr_u1im = nullptr;
	ptr_x2 = nullptr;
	ptr_y2 = nullptr;
	ptr_z2 = nullptr;
	ptr_u2re = nullptr;
	ptr_u2im = nullptr;




}

