// ClsNac.WO.h

#pragma once
using namespace System;

namespace ClsNac
{
	template<typename T>

	public ref class Mirror2D
	{
	private:
		static const double PI = 3.1415926535897932384626433832795;
	public:
		enum  class fig { ellip, plane, parab };
		Mirror2D(T _L1, T _L2, T theta, fig _fig);
	};
}