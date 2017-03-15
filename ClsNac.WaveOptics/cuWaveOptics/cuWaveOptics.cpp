// これは メイン DLL ファイルです。

#include "stdafx.h"

#include "cuWaveOptics.h"
#include <cuda_runtime.h>

__global__ void PropFw1D(const double _k,
	const int _n1, const double* _x1, const double* _y1, const double* _u1re, const double* _u1im,
	const int _n2, const double* _x2, const double* _y2, double* _u2re, double* _u2im);

void ClsNac::cuWaveOptics::Propagate1D(	int _dir,
	array<double>^ _x1, array<double>^ _y1, array<Complex>^ _u1, 
	array<double>^ _x2, array<double>^ _y2, array<Complex>^ _u2)
{
	dim3 gridDim;
	dim3 blockDim;
	size_t offset = 0;
	const double* x1;
	cudaSetupArgument(x1, offset);
	offset += sizeof(x1);

	void** args;
	
	cudaLaunchKernel((const void*)&PropFw1D, gridDim, blockDim, args);
}

void ClsNac::cuWaveOptics::Propagate2D(double _lambda, int _dir, array<double>^ _x1, array<double>^ _y1, array<double>^ _z1, array<Complex>^ _u1, array<double>^ _x2, array<double>^ _y2, array<double>^ _z2, array<Complex>^ _u2)
{
	throw gcnew System::NotImplementedException();
}
