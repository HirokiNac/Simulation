// これは メイン DLL ファイルです。

#include "stdafx.h"
#include "ClsNac.fftw.h"
#pragma comment(lib, "libfftw3-3.lib")
#pragma comment(lib, "libfftw3f-3.lib")
#pragma comment(lib, "libfftw3l-3.lib")

void ClsNac::fftw::Execute1D(array<Complex>^ _in, array<Complex>^ _out, int _dir)
{
	int len = _in->Length;

	//FFT入力用
	fftw_complex *in_fftw = (fftw_complex*)fftw_malloc(sizeof(fftw_complex)*len);
	for (int i = 0; i < len; i++)
	{
		in_fftw[i][0] = _in[i].Real;
		in_fftw[i][1] = _in[i].Imaginary;
	}

	//FFT結果
	fftw_complex *out_fftw = (fftw_complex*)fftw_malloc(sizeof(fftw_complex)*len);

	//FFTプラン
	fftw_plan p = fftw_plan_dft_1d(len, in_fftw, out_fftw,_dir, FFTW_ESTIMATE);
	fftw_execute(p);

	if (_dir == FFTW_BACKWARD)
	{
		double len_2 = 1.0 / Math::Sqrt(len);
		for (int i = 0; i < len; i++)
		{
			_out[i] = Complex(out_fftw[i][0] / len_2, out_fftw[i][1]);
		}
	}
	else
	{
		for (int i = 0; i < len; i++)
		{
			_out[i] = Complex(out_fftw[i][0], out_fftw[i][1]);
		}
	}

	fftw_destroy_plan(p);
	fftw_free(in_fftw);
	fftw_free(out_fftw);

}

void ClsNac::fftw::Forward1D(array<double>^_in, array<Complex>^_out)
{
	int len = _in->Length;

	//FFT入力用
	double *in_fftw = (double*)fftw_malloc(sizeof(double)*len);
	for (int i = 0; i < len; i++)
	{
		in_fftw[i] = _in[i];
	}

	//FFT結果
	fftw_complex *out_fftw = (fftw_complex*)fftw_malloc(sizeof(fftw_complex)*len);

	//FFTプラン
	fftw_plan p = fftw_plan_dft_r2c_1d(len, in_fftw, out_fftw, FFTW_ESTIMATE);
	fftw_execute(p);

	for (int i = 0; i < len; i++)
		_out[i] = Complex(out_fftw[i][0], out_fftw[i][1]);

	fftw_destroy_plan(p);
	fftw_free(in_fftw);
	fftw_free(out_fftw);

}

void ClsNac::fftw::Backward1D(array<Complex>^ _in, array<double>^ _out)
{
	int len = _in->Length;

	//FFT入力用
	fftw_complex *in_fftw = (fftw_complex*)fftw_malloc(sizeof(fftw_complex)*len);
	for (int i = 0; i < len; i++)
	{
		in_fftw[i][0] = _in[i].Real;
		in_fftw[i][1] = _in[i].Imaginary;
	}

	//FFT結果
	double *out_fftw = (double*)fftw_malloc(sizeof(double)*len);

	//FFTプラン
	fftw_plan p = fftw_plan_dft_c2r_1d(len, in_fftw, out_fftw, FFTW_ESTIMATE);
	fftw_execute(p);

	double len_2 = 1.0 / Math::Sqrt(len);
	for (int i = 0; i < len; i++)
		_out[i] = out_fftw[i] / len_2;

	fftw_destroy_plan(p);
	fftw_free(in_fftw);
	fftw_free(out_fftw);

}


void ClsNac::fftw::Execute2D(array<Complex, 2>^ _in, array<Complex, 2>^ _out, int _dir, bool _shift)
{
	int len1 = _in->GetLength(0);
	int len2 = _in->GetLength(1);

	//FFT入力用
	fftw_complex *in_fftw = (fftw_complex*)fftw_malloc(sizeof(fftw_complex)*len1*len2);
	for (int i = 0; i < len1; i++)
		for (int j = 0; j < len2; j++)
		{
			in_fftw[i*len2 + j][0] = _in[i, j].Real;
			in_fftw[i*len2 + j][1] = _in[i, j].Imaginary;
		}

	fftw_complex *out_fftw = (fftw_complex*)fftw_malloc(sizeof(fftw_complex)*len1*len2);

	fftw_plan p = fftw_plan_dft_2d(len1, len2, in_fftw, out_fftw, _dir, FFTW_ESTIMATE);
	fftw_execute(p);


	if (_dir == FFTW_BACKWARD)
	{
		//BACKWARD
		double len_2 = 1.0 / Math::Sqrt(len1*len2);

		for (int i = 0; i < len1 / 2; i++)
			for (int j = 0; j < len2 / 2; j++)
			{
				if (_shift)
				{
					//shiftあり
					_out[i, j] = Complex(out_fftw[i + len1 / 2, j + len2 / 2][0] * len_2, out_fftw[i + len1 / 2, j + len2 / 2][1]);
					_out[i + len1 / 2, j] = Complex(out_fftw[i, j + len2 / 2][0] * len_2, out_fftw[i, j + len2 / 2][1]);
					_out[i, j + len2 / 2] = Complex(out_fftw[i + len1 / 2, j][0] * len_2, out_fftw[i + len1 / 2, j][1] );
					_out[i + len1 / 2, j + len2 / 2] = Complex(out_fftw[i, j][0] * len_2, out_fftw[i, j][1] );
				}
				else
				{
					//shiftなし
					_out[i, j] = Complex(out_fftw[i*len2 + j][0] * len_2, out_fftw[i*len2 + j][1]);
					_out[i + len1 / 2, j] = Complex(out_fftw[(i + len1 / 2)*len2 + j][0] * len_2, out_fftw[(i + len1 / 2)*len2 + j][1]);
					_out[i, j + len2 / 2] = Complex(out_fftw[i*len2 + j + len2 / 2][0] * len_2, out_fftw[i*len2 + j + len2 / 2][1]);
					_out[i + len1 / 2, j + len2 / 2] = Complex(out_fftw[(i + len1 / 2)*len2 + j + len2 / 2][0] * len_2, out_fftw[(i + len1 / 2)*len2 + j + len2 / 2][1]);
				}
			}
	}
	else
	{
		//FORWARD
		for (int i = 0; i < len1 / 2; i++)
			for (int j = 0; j < len2 / 2; j++)
			{
				if (_shift)
				{
					//shiftあり
					_out[i, j] = Complex(out_fftw[i + len1 / 2, j + len2 / 2][0], out_fftw[i + len1 / 2, j + len2 / 2][1]);
					_out[i + len1 / 2, j] = Complex(out_fftw[i, j + len2 / 2][0], out_fftw[i, j + len2 / 2][1]);
					_out[i, j + len2 / 2] = Complex(out_fftw[i + len1 / 2, j][0], out_fftw[i + len1 / 2, j][1]);
					_out[i + len1 / 2, j + len2 / 2] = Complex(out_fftw[i, j][0], out_fftw[i, j][1]);
				}
				else
				{
					//shiftなし
					_out[i, j] = Complex(out_fftw[i*len2 + j][0], out_fftw[i*len2 + j][1]);
					_out[i + len1 / 2, j] = Complex(out_fftw[(i + len1 / 2)*len2 + j][0], out_fftw[(i + len1 / 2)*len2 + j][1]);
					_out[i, j + len2 / 2] = Complex(out_fftw[i*len2 + j + len2 / 2][0], out_fftw[i*len2 + j + len2 / 2][1]);
					_out[i + len1 / 2, j + len2 / 2] = Complex(out_fftw[(i + len1 / 2)*len2 + j + len2 / 2][0], out_fftw[(i + len1 / 2)*len2 + j + len2 / 2][1]);
				}
			}
	}

}


void ClsNac::fftw::Forward2D(array<double, 2>^ _in, array<Complex, 2>^ _out, bool _shift)
{
	int len1 = _in->GetLength(0);
	int len2 = _in->GetLength(1);

	//FFT入力用
	double *in_fftw = (double*)fftw_malloc(sizeof(double)*len1*len2);
	for (int i = 0; i < len1; i++)
		for (int j = 0; j < len2; j++)
		{
			in_fftw[i*len2 + j] = _in[i, j];
		}

	fftw_complex *out_fftw = (fftw_complex*)fftw_malloc(sizeof(fftw_complex)*len1*len2);

	fftw_plan p = fftw_plan_dft_r2c_2d(len1, len2, in_fftw, out_fftw, FFTW_ESTIMATE);
	fftw_execute(p);


	//FORWARD
	for (int i = 0; i < len1 / 2; i++)
		for (int j = 0; j < len2 / 2; j++)
		{
			double re, im;
			
			
			if (_shift)
			{
				//shiftあり
				_out[i, j] = Complex(out_fftw[i + len1 / 2, j + len2 / 2][0], out_fftw[i + len1 / 2, j + len2 / 2][1]);
				_out[i + len1 / 2, j] = Complex(out_fftw[i, j + len2 / 2][0], out_fftw[i, j + len2 / 2][1]);
				_out[i, j + len2 / 2] = Complex(out_fftw[i + len1 / 2, j][0], out_fftw[i + len1 / 2, j][1]);
				_out[i + len1 / 2, j + len2 / 2] = Complex(out_fftw[i, j][0], out_fftw[i, j][1]);
			}
			else
			{
				//shiftなし
				_out[i, j] = Complex(out_fftw[i*len2 + j][0], out_fftw[i*len2 + j][1]);
				_out[i + len1 / 2, j] = Complex(out_fftw[i + len1 / 2, j][0], out_fftw[i + len1 / 2, j][1]);
				_out[i, j + len2 / 2] = Complex(out_fftw[i, j + len2 / 2][0], out_fftw[i, j + len2 / 2][1]);
				_out[i + len1 / 2, j + len2 / 2] = Complex(out_fftw[i + len1 / 2, j + len2 / 2][0], out_fftw[i + len1 / 2, j + len2 / 2][1]);
			}
		}
}

void ClsNac::fftw::Backward2D(array<Complex,2>^ _in, array<double,2>^ _out, bool _shift)
{
	int len1 = _in->GetLength(0);
	int len2 = _in->GetLength(1);

	//FFT入力用
	fftw_complex *in_fftw = (fftw_complex*)fftw_malloc(sizeof(fftw_complex)*len1*len2);
	for (int i = 0; i < len1; i++)
		for (int j = 0; j < len2; j++)
		{
			in_fftw[i*len2 + j][0] = _in[i, j].Real;
			in_fftw[i*len2 + j][1] = _in[i, j].Imaginary;
		}

	double *out_fftw = (double*)fftw_malloc(sizeof(double)*len1*len2);

	fftw_plan p = fftw_plan_dft_c2r_2d(len1, len2, in_fftw, out_fftw, FFTW_ESTIMATE);
	fftw_execute(p);


		//BACKWARD
		double len_2 = 1.0 / Math::Sqrt(len1*len2);

		for (int i = 0; i < len1 / 2; i++)
			for (int j = 0; j < len2 / 2; j++)
			{
				if (_shift)
				{
					//shiftあり
					_out[i, j] = out_fftw[i + len1 / 2, j + len2 / 2] * len_2;
					_out[i + len1 / 2, j] = out_fftw[i, j + len2 / 2] * len_2;
					_out[i, j + len2 / 2] = out_fftw[i + len1 / 2, j] * len_2;
					_out[i + len1 / 2, j + len2 / 2] = out_fftw[i, j] * len_2;
				}
				else
				{
					//shiftなし
					_out[i, j] = out_fftw[i*len2 + j] * len_2;
					_out[i + len1 / 2, j] = out_fftw[i + len1 / 2, j] * len_2;
					_out[i, j + len2 / 2] = out_fftw[i, j + len2 / 2] * len_2;
					_out[i + len1 / 2, j + len2 / 2] = out_fftw[i + len1 / 2, j + len2 / 2] * len_2;
				}
			}
}

