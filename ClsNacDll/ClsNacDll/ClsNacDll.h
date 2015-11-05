// ClsNacDll.h

#pragma once

using namespace System;
using namespace System::Numerics;

namespace ClsNacDll {

	public ref class ampwf
	{
		// TODO: このクラスの、ユーザーのメソッドをここに追加してください。
	public:
		static void fp(array<double>^ x, array<double>^ y, array<double>^ z);
		static void fp2(array<Complex>^ a);
	};
}
