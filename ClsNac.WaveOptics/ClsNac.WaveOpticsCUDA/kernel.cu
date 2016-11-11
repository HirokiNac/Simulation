
#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#include <math.h>
#include <stdio.h>
#include <stdlib.h>


const int threads = 16;
const int numBlock = 16;
const dim3 threadsPerBlock = dim3(512, 512);

dim3 calcBlock(dim3 thread, int x, int y)
{
	return dim3(
		static_cast<int>(ceil(1.0*x / thread.x)),
		static_cast<int>(ceil(1.0*y / thread.y)));
}


__global__ void PropFw1D(const double _k,
	const int _n1, const double* _x1, const double* _y1, const double* _u1re, const double* _u1im,
	const int _n2, const double* _x2, const double* _y2, double* _u2re, double* _u2im)
{
	const unsigned int col = blockIdx.x * blockDim.x + threadIdx.x;

	double r, rx, ry, rr;
	double tr, ti;
	double tur, tui;
	double ur = 0.0, ui = 0.0;
	if (col < _n2)
	{
		for (int j = 0; j < _n1; j++)
		{
			rx = _x2[col] - _x1[j];
			ry = _y2[col] - _y1[j];
			r = sqrt(rx*rx + ry*ry);

			rr = 1.0 / sqrt(r);
			tr = cos(-_k*r) * rr;
			ti = sin(-_k*r) * rr;

			tur = _u1re[j];
			tui = _u1im[j];

			ur = ur + tur*tr - tui*ti;
			ui = ui + tur*ti + tui*tr;

		}
		_u2re[col] = _u2re[col] + ur;
		_u2im[col] = _u2im[col] + ui;

	}

}

__global__ void PropFw1D_f(const float _k,
	const int _n1, const float* _x1, const float* _y1, const float* _u1re, const float* _u1im,
	const int _n2, const float* _x2, const  float* _y2, float* _u2re, float* _u2im)
{
	const unsigned int col = blockIdx.x + blockDim.x* threadIdx.x;

	float r, rx, ry;
	float tr, ti;
	float tur, tui;
	float ur, ui;
	for (int j = 0; j < _n1; j++)
	{
		rx = _x2[col] - _x1[j];
		ry = _y2[col] - _y1[j];
		r = sqrt(rx*rx + ry*ry);

		tr = cos(-_k*r) / r;
		ti = sin(-_k*r) / r;

		tur = _u1re[j];
		tui = _u1im[j];

		tur = ur + tur*tr - tui*ti;
		tui = ui + tur*ti + tui*tr;

	}
	_u2re[col] = _u2re[col] + ur;
	_u2im[col] = _u2im[col] + ui;

}

__global__ void PropFw2D(const double _k,
	const int _n1, const double* _x1, const double* _y1, const  double* _z1, const  double* _u1re, const  double* _u1im,
	const int _n2, const double* _x2, const double* _y2, const double* _z2, double* _u2re, double* _u2im)
{
	const unsigned int col = blockIdx.x * blockDim.x + threadIdx.x;
	//
	if (_n2 < col)return;

	//
	double r, rx, ry, rz, rr;
	double tr, ti;
	double tur, tui;
	double ur = 0.0, ui = 0.0;
	double x1, y1, z1;

	for (int j = 0; j < _n1; j++)
	{
		x1 = _x1[j];
		y1 = _y1[j];
		z1 = _z1[j];
		
		rx = _x2[col] - x1;
		ry = _y2[col] - y1;
		rz = _z2[col] - z1;
		r = sqrt(rx*rx + ry*ry + rz*rz);

		rr = 1.0 / r;
		tr = cos(-_k*r) * rr;
		ti = sin(-_k*r) * rr;
		
		tur = _u1re[j];
		tui = _u1im[j];

		ur = ur + tur*tr - tui*ti;
		ui = ui + tur*ti + tui*tr;

	}
	_u2re[col] = _u2re[col] + ur;
	_u2im[col] = _u2im[col] + ui;
	__syncthreads();
}

__global__ void PropFw2D2(const double _k,
	const int _m1,const int _n1, const double* _x1, const double* _y1, const  double* _z1, const  double* _u1re, const  double* _u1im,
	const int _m2,const int _n2, const double* _x2, const double* _y2, const double* _z2, double* _u2re, double* _u2im)
{
	const unsigned int col = blockIdx.x * blockDim.x + threadIdx.x;
	const unsigned int row = blockIdx.y*blockDim.y + threadIdx.y;
	//
	if (_m2 < col || _n2 < row)return;
	const unsigned int colrow = col*_n2 + row;
	//
	double r, rx, ry, rz, rr;
	double tr, ti;
	double tur, tui;
	double ur = 0.0, ui = 0.0;
	double x1, y1, z1;

	for (int j = 0; j < _n1; j++)
	{
		x1 = _x1[j];
		y1 = _y1[j];
		z1 = _z1[j];

		rx = _x2[colrow] - x1;
		ry = _y2[colrow] - y1;
		rz = _z2[colrow] - z1;
		r = sqrt(rx*rx + ry*ry + rz*rz);

		rr = 1.0 / r;
		tr = cos(-_k*r) * rr;
		ti = sin(-_k*r) * rr;

		tur = _u1re[j];
		tui = _u1im[j];

		ur = ur + tur*tr - tui*ti;
		ui = ui + tur*ti + tui*tr;

	}
	_u2re[colrow] = _u2re[colrow] + ur;
	_u2im[colrow] = _u2im[colrow] + ui;
	__syncthreads();
}

__global__ void PropFw2D_f(const float _k,
	const int _n1, const float* _x1, const float* _y1, const  float* _z1, const  float* _u1re, const  float* _u1im,
	const int _n2, const float* _x2, const float* _y2, const float* _z2, float* _u2re, float* _u2im)
{
	const unsigned int col = blockIdx.x * blockDim.x + threadIdx.x;
	//
	if (_n2 < col)return;

	//
	float r, rx, ry, rz, rr;
	float tr, ti;
	float tur, tui;
	float ur = 0.0, ui = 0.0;
	float x1, y1, z1;

	for (int j = 0; j < _n1; j++)
	{
		x1 = _x1[j];
		y1 = _y1[j];
		z1 = _z1[j];

		rx = _x2[col] - x1;
		ry = _y2[col] - y1;
		rz = _z2[col] - z1;
		r = sqrt(rx*rx + ry*ry + rz*rz);

		rr = 1.0 / r;
		tr = cos(-_k*r) * rr;
		ti = sin(-_k*r) * rr;

		tur = _u1re[j];
		tui = _u1im[j];

		ur = ur + tur*tr - tui*ti;
		ui = ui + tur*ti + tui*tr;

	}
	_u2re[col] = _u2re[col] + ur;
	_u2im[col] = _u2im[col] + ui;
	__syncthreads();
}

extern "C" void
PropFw1dCuda(double _k,
int _n1, double* _x1, double* _y1, double* _u1re, double* _u1im,
int _n2, double* _x2, double* _y2, double* &_u2re, double* &_u2im)
{
	cudaSetDevice(0);

	size_t memsize1 = _n1*sizeof(double);
	size_t memsize2 = _n2*sizeof(double);

	//1
	double *dx1 = 0;
	cudaMalloc((void**)&dx1, memsize1);
	cudaMemcpy(dx1, _x1, memsize1, cudaMemcpyHostToDevice);

	double *dy1 = 0;
	cudaMalloc((void**)&dy1, memsize1);
	cudaMemcpy(dy1, _y1, memsize1, cudaMemcpyHostToDevice);

	double *du1re = 0;
	cudaMalloc((void**)&du1re, memsize1);
	cudaMemcpy(du1re, _u1re, memsize1, cudaMemcpyHostToDevice);

	double *du1im = 0;
	cudaMalloc((void**)&du1im, memsize1);
	cudaMemcpy(du1im, _u1im, memsize1, cudaMemcpyHostToDevice);

	//2
	double *dx2 = 0;
	cudaMalloc((void**)&dx2, memsize2);
	cudaMemcpy(dx2, _x2, memsize2, cudaMemcpyHostToDevice);

	double *dy2 = 0;
	cudaMalloc((void**)&dy2, memsize2);
	cudaMemcpy(dy2, _y2, memsize2, cudaMemcpyHostToDevice);

	double *du2re = 0;
	cudaMalloc((void**)&du2re, memsize2);
	//cudaMemcpy(du2re, _u2re, memsize2, cudaMemcpyHostToDevice);

	double *du2im = 0;
	cudaMalloc((void**)&du2im, memsize2);
	//cudaMemcpy(du2im, _u2im, memsize2, cudaMemcpyHostToDevice);

	PropFw1D << <numBlock, threadsPerBlock >> >(_k, _n1, dx1, dy1, du1re, du1im, _n2, dx2, dy2, du2re, du2im);

	double* u2re_out = 0;
	cudaMallocHost((void**)&u2re_out, memsize2);
	cudaMemcpy(u2re_out, du2re, memsize2, cudaMemcpyDeviceToHost);
	double* u2im_out = 0;
	cudaMallocHost((void**)&u2im_out, memsize2);
	cudaMemcpy(u2im_out, du2im, memsize2, cudaMemcpyDeviceToHost);


	for (int i = 0; i < _n2; i++)
	{
		_u2re[i] = u2re_out[i];
		_u2im[i] = u2im_out[i];
	}


	//memfree
	cudaFree(dx1);
	cudaFree(dy1);
	cudaFree(du1re);
	cudaFree(du1im);

	cudaFree(dx2);
	cudaFree(dy2);
	cudaFree(du2re);
	cudaFree(du2im);
	cudaFree(u2re_out);
	cudaFree(u2im_out);
}

extern "C" void
PropFw1dCuda_f(float _k,
int _n1, float* _x1,float* _y1, float* _u1re, float* _u1im,
int _n2, float* _x2, float* _y2, float* &_u2re, float* &_u2im)
{
	cudaSetDevice(0);

	size_t memsize1 = _n1*sizeof(float);
	size_t memsize2 = _n2*sizeof(float);

	//1
	float *dx1 = 0;
	cudaMalloc((void**)&dx1, memsize1);
	cudaMemcpy(dx1, _x1, memsize1, cudaMemcpyHostToDevice);

	float *dy1 = 0;
	cudaMalloc((void**)&dy1, memsize1);
	cudaMemcpy(dy1, _y1, memsize1, cudaMemcpyHostToDevice);

	float *du1re = 0;
	cudaMalloc((void**)&du1re, memsize1);
	cudaMemcpy(du1re, _u1re, memsize1, cudaMemcpyHostToDevice);

	float *du1im = 0;
	cudaMalloc((void**)&du1im, memsize1);
	cudaMemcpy(du1im, _u1im, memsize1, cudaMemcpyHostToDevice);

	//2
	float *dx2 = 0;
	cudaMalloc((void**)&dx2, memsize2);
	cudaMemcpy(dx2, _x2, memsize2, cudaMemcpyHostToDevice);

	float *dy2 = 0;
	cudaMalloc((void**)&dy2, memsize2);
	cudaMemcpy(dy2, _y2, memsize2, cudaMemcpyHostToDevice);

	float *du2re = 0;
	cudaMalloc((void**)&du1re, memsize1);
	cudaMemcpy(du2re, _u2re, memsize1, cudaMemcpyHostToDevice);

	float *du2im = 0;
	cudaMalloc((void**)&du1im, memsize1);
	cudaMemcpy(du2im, _u2im, memsize1, cudaMemcpyHostToDevice);

	PropFw1D_f << <_n2 / 512, 512 >> >(_k, _n1, dx1, dy1, du1re, du1im, _n2, dx2, dy2, du2re, du2im);

	//out
	float* u2re_out = (float*)malloc(memsize2);
	//cudaMallocHost((void**)&u2re_out, memsize2);
	cudaMemcpy(u2re_out, du2re, memsize2, cudaMemcpyDeviceToHost);

	float* u2im_out = (float*)malloc(memsize2);
	//cudaMallocHost((void**)&u2im_out, memsize2);
	cudaMemcpy(u2im_out, du2im, memsize2, cudaMemcpyDeviceToHost);

	for (int i = 0; i < _n2; i++)
	{
		_u2re[i] = u2re_out[i];
		_u2im[i] = u2im_out[i];
	}

	//memfree
	//1
	cudaFree(dx1);
	cudaFree(dy1);
	cudaFree(du1re);
	cudaFree(du1im);
	//2
	cudaFree(dx2);
	cudaFree(dy2);
	cudaFree(du2re);
	cudaFree(du2im);
	//out
	free(u2re_out);
	free(u2im_out);
	//cudaFree(u2re_out);
	//cudaFree(u2im_out);
}

extern "C" void
PropFw2dCuda(double _k,
int _n1, double* _x1, double* _y1, double* _z1, double* _u1re, double* _u1im,
int _n2, double* _x2, double* _y2, double* _z2, double* &_u2re, double* &_u2im)
{
	cudaSetDevice(1);

	size_t memsize1 = _n1*sizeof(double);
	size_t memsize2 = _n2*sizeof(double);

	//1
	double *dx1 = 0;
	cudaMalloc((void**)&dx1, memsize1);
	cudaMemcpy(dx1, _x1, memsize1, cudaMemcpyHostToDevice);

	double *dy1 = 0;
	cudaMalloc((void**)&dy1, memsize1);
	cudaMemcpy(dy1, _y1, memsize1, cudaMemcpyHostToDevice);

	double *dz1 = 0;
	cudaMalloc((void**)&dz1, memsize1);
	cudaMemcpy(dz1, _z1, memsize1, cudaMemcpyHostToDevice);

	double *du1re = 0;
	cudaMalloc((void**)&du1re, memsize1);
	cudaMemcpy(du1re, _u1re, memsize1, cudaMemcpyHostToDevice);

	double *du1im = 0;
	cudaMalloc((void**)&du1im, memsize1);
	cudaMemcpy(du1im, _u1im, memsize1, cudaMemcpyHostToDevice);


	//2
	double *dx2 = 0;
	cudaMalloc((void**)&dx2, memsize2);
	cudaMemcpy(dx2, _x2, memsize2, cudaMemcpyHostToDevice);

	double *dy2 = 0;
	cudaMalloc((void**)&dy2, memsize2);
	cudaMemcpy(dy2, _y2, memsize2, cudaMemcpyHostToDevice);

	double *dz2 = 0;
	cudaMalloc((void**)&dz2, memsize2);
	cudaMemcpy(dz2, _z2, memsize2, cudaMemcpyHostToDevice);

	double *du2re = 0;
	cudaMalloc((void**)&du2re, memsize2);
	//cudaMemcpy(du2re, _u2re, memsize2, cudaMemcpyHostToDevice);

	double *du2im = 0;
	cudaMalloc((void**)&du2im, memsize2);
	//cudaMemcpy(du2im, _u2im, memsize2, cudaMemcpyHostToDevice);
	dim3 b = calcBlock(threadsPerBlock, _n2, 1);
	PropFw2D << <_n2/512,512 /*calcBlock(threadsPerBlock,_n2,1), threadsPerBlock*/ >> >(_k, _n1, dx1, dy1, dz1, du1re, du1im, _n2, dx2, dy2, dz2, du2re, du2im);

	cudaDeviceSynchronize();


	double* u2re_out =  (double*)malloc(memsize2);
	//cudaMalloc((void**)&u2re_out, memsize2);
	cudaMemcpy(u2re_out, du2re, memsize2, cudaMemcpyDeviceToHost);
	double* u2im_out =   (double*)malloc(memsize2);
	//cudaMalloc((void**)&u2im_out, memsize2);
	cudaMemcpy(u2im_out, du2im, memsize2, cudaMemcpyDeviceToHost);


	for (int i = 0; i < _n2; i++)
	{
		_u2re[i] = u2re_out[i];
		_u2im[i] = u2im_out[i];
	}

	//memfree
	cudaFree(dx1);
	cudaFree(dy1);
	cudaFree(dz1);
	cudaFree(du1re);
	cudaFree(du1im);

	cudaFree(dx2);
	cudaFree(dy2);
	cudaFree(dz2);
	cudaFree(du2re);
	cudaFree(du2im);

	free(u2re_out);
	free(u2im_out);
	//cudaFreeHost(u2re_out);
	//cudaFreeHost(u2im_out);

	cudaDeviceReset();
}

extern "C" void
PropFw2dCuda2(double _k,
int _m1,int _n1, double* _x1, double* _y1, double* _z1, double* _u1re, double* _u1im,
int _m2,int _n2, double* _x2, double* _y2, double* _z2, double* &_u2re, double* &_u2im)
{
	cudaSetDevice(1);

	size_t memsize1 = _m1*_n1*sizeof(double);
	size_t memsize2 = _m2*_n2*sizeof(double);

	//1
	double *dx1 = 0;
	cudaMalloc((void**)&dx1, memsize1);
	cudaMemcpy(dx1, _x1, memsize1, cudaMemcpyHostToDevice);

	double *dy1 = 0;
	cudaMalloc((void**)&dy1, memsize1);
	cudaMemcpy(dy1, _y1, memsize1, cudaMemcpyHostToDevice);

	double *dz1 = 0;
	cudaMalloc((void**)&dz1, memsize1);
	cudaMemcpy(dz1, _z1, memsize1, cudaMemcpyHostToDevice);

	double *du1re = 0;
	cudaMalloc((void**)&du1re, memsize1);
	cudaMemcpy(du1re, _u1re, memsize1, cudaMemcpyHostToDevice);

	double *du1im = 0;
	cudaMalloc((void**)&du1im, memsize1);
	cudaMemcpy(du1im, _u1im, memsize1, cudaMemcpyHostToDevice);


	//2
	double *dx2 = 0;
	cudaMalloc((void**)&dx2, memsize2);
	cudaMemcpy(dx2, _x2, memsize2, cudaMemcpyHostToDevice);

	double *dy2 = 0;
	cudaMalloc((void**)&dy2, memsize2);
	cudaMemcpy(dy2, _y2, memsize2, cudaMemcpyHostToDevice);

	double *dz2 = 0;
	cudaMalloc((void**)&dz2, memsize2);
	cudaMemcpy(dz2, _z2, memsize2, cudaMemcpyHostToDevice);

	double *du2re = 0;
	cudaMalloc((void**)&du2re, memsize2);
	//cudaMemcpy(du2re, _u2re, memsize2, cudaMemcpyHostToDevice);

	double *du2im = 0;
	cudaMalloc((void**)&du2im, memsize2);
	//cudaMemcpy(du2im, _u2im, memsize2, cudaMemcpyHostToDevice);

	PropFw2D2 << <calcBlock(threadsPerBlock, _m2, _n2), threadsPerBlock >> >(_k, _m1, _n1, dx1, dy1, dz1, du1re, du1im, _m2, _n2, dx2, dy2, dz2, du2re, du2im);

	cudaDeviceSynchronize();


	double* u2re_out = (double*)malloc(memsize2);
	//cudaMalloc((void**)&u2re_out, memsize2);
	cudaMemcpy(u2re_out, du2re, memsize2, cudaMemcpyDeviceToHost);
	double* u2im_out = (double*)malloc(memsize2);
	//cudaMalloc((void**)&u2im_out, memsize2);
	cudaMemcpy(u2im_out, du2im, memsize2, cudaMemcpyDeviceToHost);


	for (int i = 0; i < _m2*_n2; i++)
	{
		_u2re[i] = u2re_out[i];
		_u2im[i] = u2im_out[i];
	}

	//memfree
	cudaFree(dx1);
	cudaFree(dy1);
	cudaFree(dz1);
	cudaFree(du1re);
	cudaFree(du1im);

	cudaFree(dx2);
	cudaFree(dy2);
	cudaFree(dz2);
	cudaFree(du2re);
	cudaFree(du2im);

	free(u2re_out);
	free(u2im_out);
	//cudaFreeHost(u2re_out);
	//cudaFreeHost(u2im_out);

	cudaDeviceReset();
}

extern "C" void
PropFw2dCuda_f(float _k,
int _n1, float* _x1, float* _y1, float* _z1, float* _u1re, float* _u1im,
int _n2, float* _x2, float* _y2, float* _z2, float* &_u2re, float* &_u2im)
{
	cudaSetDevice(1);

	size_t memsize1 = _n1*sizeof(float);
	size_t memsize2 = _n2*sizeof(float);

	//1
	float *dx1 = 0;
	cudaMalloc((void**)&dx1, memsize1);
	cudaMemcpy(dx1, _x1, memsize1, cudaMemcpyHostToDevice);

	float *dy1 = 0;
	cudaMalloc((void**)&dy1, memsize1);
	cudaMemcpy(dy1, _y1, memsize1, cudaMemcpyHostToDevice);

	float *dz1 = 0;
	cudaMalloc((void**)&dz1, memsize1);
	cudaMemcpy(dz1, _z1, memsize1, cudaMemcpyHostToDevice);

	float *du1re = 0;
	cudaMalloc((void**)&du1re, memsize1);
	cudaMemcpy(du1re, _u1re, memsize1, cudaMemcpyHostToDevice);

	float *du1im = 0;
	cudaMalloc((void**)&du1im, memsize1);
	cudaMemcpy(du1im, _u1im, memsize1, cudaMemcpyHostToDevice);


	//2
	float *dx2 = 0;
	cudaMalloc((void**)&dx2, memsize2);
	cudaMemcpy(dx2, _x2, memsize2, cudaMemcpyHostToDevice);

	float *dy2 = 0;
	cudaMalloc((void**)&dy2, memsize2);
	cudaMemcpy(dy2, _y2, memsize2, cudaMemcpyHostToDevice);

	float *dz2 = 0;
	cudaMalloc((void**)&dz2, memsize2);
	cudaMemcpy(dz2, _z2, memsize2, cudaMemcpyHostToDevice);

	float *du2re = 0;
	cudaMalloc((void**)&du2re, memsize2);
	//cudaMemcpy(du2re, _u2re, memsize2, cudaMemcpyHostToDevice);

	float *du2im = 0;
	cudaMalloc((void**)&du2im, memsize2);
	//cudaMemcpy(du2im, _u2im, memsize2, cudaMemcpyHostToDevice);

	PropFw2D_f << <_n2 / 512, 512>> >(_k, _n1, dx1, dy1, dz1, du1re, du1im, _n2, dx2, dy2, dz2, du2re, du2im);

	cudaDeviceSynchronize();


	float* u2re_out = (float*)malloc(memsize2);
	//cudaMallocHost((void**)&u2re_out, memsize2);
	cudaMemcpy(u2re_out, du2re, memsize2, cudaMemcpyDeviceToHost);
	float* u2im_out =   (float*)malloc(memsize2);
	//cudaMallocHost((void**)&u2im_out, memsize2);
	cudaMemcpy(u2im_out, du2im, memsize2, cudaMemcpyDeviceToHost);


	for (int i = 0; i < _n2; i++)
	{
		_u2re[i] = u2re_out[i];
		_u2im[i] = u2im_out[i];
	}

	//memfree
	cudaFree(dx1);
	cudaFree(dy1);
	cudaFree(dz1);
	cudaFree(du1re);
	cudaFree(du1im);

	cudaFree(dx2);
	cudaFree(dy2);
	cudaFree(dz2);
	cudaFree(du2re);
	cudaFree(du2im);

	free(u2re_out);
	free(u2im_out);
	//cudaFreeHost(u2re_out);
	//cudaFreeHost(u2im_out);

	cudaDeviceReset();
}
