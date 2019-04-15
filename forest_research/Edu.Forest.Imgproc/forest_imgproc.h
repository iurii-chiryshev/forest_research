#pragma once
#include "forest_export.hpp"
#include "forest_opencv.hpp"
namespace fr{
using namespace cv;

//************************************
// Method:    gammaCorrerction
// Parameter: const Mat & src - ������� ����������� CV_8U
// Parameter: Mat & dst - �������� ����������� CV_8U
// Parameter: float gamma
// ����� ��������� �����������
//************************************
void FOREST_PROC_API correctGamma(const Mat& src,Mat& dst, double gamma = 2.0);
}


