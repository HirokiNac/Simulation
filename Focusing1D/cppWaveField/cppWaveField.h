// cppWaveField.h

#pragma once

using namespace System;
using namespace System::Numerics;

namespace cppWaveField {

	public ref class WF
	{
		// TODO: ���̃N���X�́A���[�U�[�̃��\�b�h�������ɒǉ����Ă��������B
	public:
		static void fProp(double lambda,
			array<double>^x1, array<double>^y1, array<Complex>^u1, 
			array<double>^x2, array<double>^y2, array<Complex>^u2);
	};
}
