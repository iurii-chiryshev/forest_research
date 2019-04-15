#include "forest_imgproc.h"

namespace fr {

using namespace cv;

void correctGamma(const Mat& src,Mat& dst, double gamma /*= 2.0*/)
{
	CV_Assert(src.depth() == CV_8U);
	double power = 1.0 / gamma;

	Mat lut(1, 256, CV_8UC1 );
	uchar * ptr = lut.ptr();
	for( int i = 0; i < 256; i++ )
	{
		ptr[i] = saturate_cast<uchar>( pow( i / 255.0, power ) * 255.0 );
	}
	cv::LUT( src, lut, dst);
}

}