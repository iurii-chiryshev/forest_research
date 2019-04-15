#pragma once
#include "forest_export.hpp"
#include "forest_opencv.hpp"
namespace fr{
using namespace cv;

//************************************
// Method:    gammaCorrerction
// Parameter: const Mat & src - входное изображение CV_8U
// Parameter: Mat & dst - выходное изображение CV_8U
// Parameter: float gamma
// Гамма коррекция изображения
//************************************
void FOREST_PROC_API correctGamma(const Mat& src,Mat& dst, double gamma = 2.0);
}


