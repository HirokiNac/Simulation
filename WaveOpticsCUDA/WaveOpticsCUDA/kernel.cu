
#include "cuda_runtime.h"
#include "device_launch_parameters.h"

#include <stdio.h>
#include<stdlib.h>
#include <math.h>

const int threads = 1536;
const int numBlock = 32;
const dim3 threadsPerBlock = dim3(1024);


__global__
void Prop1D_kernel(const double _k,const int _dir,
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

__global__
void Prop2D_kernel(const double _k,const int _dir,
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
}

extern "C" void
Prop1DCuda(const double _k, const int _dir,
	const int _n1, const double* _x1, const double* _y1, const double* _u1re, const double* _u1im,
	const int _n2, const double* _x2, const double* _y2, double* _u2re, double* _u2im)
{
	cudaSetDevice(0);

	size_t memsize1 = _n1 * sizeof(double);
	size_t memsize2 = _n2 * sizeof(double);

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

	Prop1D_kernel << <numBlock, threads >> > (_k, _dir, _n1, dx1, dy1, du1re, du1im, _n2, dx2, dy2, du2re, du2im);

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
Prop2DCuda(const double _k,const int _dir,
	const int _n1,const  double* _x1,const  double* _y1,const double* _z1, const double* _u1re,const double* _u1im,
	const int _n2,const double* _x2,const  double* _y2,const double* _z2, double* _u2re, double* _u2im)
{
	cudaSetDevice(1);

	size_t memsize1 = _n1 * sizeof(double);
	size_t memsize2 = _n2 * sizeof(double);

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

	double *du2im = 0;
	cudaMalloc((void**)&du2im, memsize2);


	Prop2D_kernel << <numBlock,threads >> >(_k,_dir, _n1, dx1, dy1, dz1, du1re, du1im, _n2, dx2, dy2, dz2, du2re, du2im);

	cudaDeviceSynchronize();


	double* u2re_out = (double*)malloc(memsize2);
	//cudaMalloc((void**)&u2re_out, memsize2);
	cudaMemcpy(u2re_out, du2re, memsize2, cudaMemcpyDeviceToHost);
	double* u2im_out = (double*)malloc(memsize2);
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


