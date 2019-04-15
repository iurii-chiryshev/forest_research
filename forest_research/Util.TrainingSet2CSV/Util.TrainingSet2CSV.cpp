#include "stdafx.h"
#include <conio.h>
#include <string>
#include <sstream>
#include <fstream>
#include <vector>
#include "opencv/cv.h"
#include "opencv/highgui.h"
#include "opencv2/features2d/features2d.hpp"
#include "opencv2/objdetect/objdetect.hpp"
#include "opencv2/gpu/gpu.hpp"
#include "opencv2/ml/ml.hpp"

using namespace cv;
using namespace std;

void split_string(const string &s, char delim, vector<string> &elems) {
	stringstream ss;
	ss.str(s);
	string item;
	while (getline(ss, item, delim)) {
		elems.push_back(item);
	}
}

void load_images(std::string fileName, vector<vector<string>>& imgs)
{
	string str;
	std::ifstream file(fileName.c_str());
	if (!file.is_open())
		return;
	while (!file.eof())
	{
		std::getline(file, str);
		if (str.empty()) break;
		vector<string> elements;
		split_string(str, ',', elements);
		imgs.push_back(elements);
	}
	file.close();
}

void createAndSave(const vector<vector<string>>& data, const string& filename) {
	int match_method = CV_TM_SQDIFF;
	for (int i = 0; i < data.size();i++) {
		const vector<string>& item = data[i];
		string img_name = item[0];
		vector<string> tmpl_names(++item.begin(), item.end());
		Mat img = imread(img_name), img_display;
		cout  <<i << " " << img_name << endl;
		img.copyTo(img_display);
		for (int j = 0; j < tmpl_names.size();j++) {
			string tmpl_name = tmpl_names[j];
			Mat tmpl = imread(tmpl_name), result;
			matchTemplate(img, tmpl, result, match_method);
			
			normalize(result, result, 0, 1, NORM_MINMAX, -1, Mat());
			double minVal; double maxVal; Point minLoc; Point maxLoc;
			Point matchLoc;
			minMaxLoc(result, &minVal, &maxVal, &minLoc, &maxLoc, Mat());
			/// For SQDIFF and SQDIFF_NORMED, the best matches are lower values. For all the other methods, the higher the better
			if (match_method == TM_SQDIFF || match_method == TM_SQDIFF_NORMED)
			{
				matchLoc = minLoc;
			}
			else
			{
				matchLoc = maxLoc;
			}
			cv::Rect matchRect(matchLoc, Point(matchLoc.x + tmpl.cols, matchLoc.y + tmpl.rows));
			int r = (int)(matchRect.width / 3.);
			cv::Point cp = Point(matchRect.x + matchRect.width / 2, matchRect.y + matchRect.height / 2);
			cv::rectangle(img_display, matchLoc, Point(matchLoc.x + tmpl.cols, matchLoc.y + tmpl.rows), cv::Scalar(0, 0, 255), 3);
			cv::circle(img_display, cp, r, cv::Scalar(0, 255, 0), 3);
			cout << j << endl;
		}
		Mat out_image;
		if (img_display.cols > 1024)
		{
			cv::resize(img_display, out_image, cv::Size(), 1024. / img_display.cols, 1024. / img_display.cols);
		}
		else
		{
			img_display.copyTo(out_image);
		}
		imshow(img_name, out_image);
		cv::waitKey(10);
	}
}

int _tmain(int argc, _TCHAR* argv[])
{
	vector<vector<string>> vpos, vneg;
	load_images("positive.csv",vpos);
	createAndSave(vpos, "");
	while (!_kbhit())
	{
		cv::waitKey(10);
	}
	cv::destroyAllWindows();
	return 0;
}

