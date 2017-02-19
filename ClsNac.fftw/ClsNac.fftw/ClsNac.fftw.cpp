// これは メイン DLL ファイルです。

#include "stdafx.h"
#include<fftw3.h>
#include "ClsNac.fftw.h"

void ClsNac::fftw::Execute1D(array<Complex>^ _in, array<Complex>^ _out, int _sign)
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
	fftw_plan p = fftw_plan_dft_1d(len, in_fftw, out_fftw, _sign, FFTW_ESTIMATE);
	fftw_execute(p);
	
	double len_2 = 1.0 / Math::Sqrt(len);
	for (int i = 0; i < len; i++)
	{
		_out[i] = Complex(out_fftw[i][0] * len_2, out_fftw[i][1] * len_2);
	}

	fftw_destroy_plan(p);
	fftw_free(in_fftw);
	fftw_free(out_fftw);

}
void ClsNac::fftw::Execute1D(array<Complex>^ _in, array<double>^ _out, int _sign)
{
}

void ClsNac::fftw::Execute2D(array<Complex>^ _in, array<Complex>^ _out, int _sign, bool _shift)
{
}

void ClsNac::fftw::Execute2D(array<double>^ _in, array<Complex>^ _out, int _sign, bool _shift)
{
}

void ClsNac::fftw::Execute2D(array<Complex>^ _in, array<double>^ _out, int _sign, bool _shift)
{
}

