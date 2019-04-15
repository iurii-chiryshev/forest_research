// Test.HOG.cpp: определяет точку входа для консольного приложения.
//


#include "stdafx.h"
#include "Edu.Forest.Imgproc/forest_hog.h"
#include "Edu.Forest.Imgproc/CpuInfo.h"
#include <iostream>
#include <conio.h>
#include <stdio.h>
#include <windows.h>

using namespace cv;
using namespace fr;
using namespace std;

void test_hog()
{
	string patch = "source";
	fr::HOG hog(Size(64,64),Size(16,16),Size(8,8),Size(8,8),9,HOG::NORM_L2);
	hog.loadRTrees("model-rf.xml", "simpleRTreesModel");
	for(int i = 1; i <= 1; i++ )
	{
		stringstream str_i;
		str_i << i;
		Mat img = cv::imread(str_i.str() + ".jpg", CV_LOAD_IMAGE_COLOR);
		if(img.cols > 2048)
		{
			Mat _img;
			cv::resize(img,_img,cv::Size(),2048./img.cols,2048./img.cols);
			img = _img;
		}
		Mat gray, min_img;
		cv::cvtColor(img,gray,CV_BGR2GRAY);
		vector<cv::Rect> locations;
		vector<float> weights;
		/////////////////////////////////
		hog.detectMultiScale(gray,locations,weights);
		for(int j = 0; j < (int)locations.size();j++)
		{
			/*cv::rectangle(img,locations[j],cv::Scalar(0,255,0),2);*/
			int r = (int)(locations[j].width/3.);
			cv::Point cp = Point(locations[j].x + locations[j].width/2,locations[j].y + locations[j].height/2);
			cv::circle(img,cp,r,cv::Scalar(0,255,0),4);
		}
		cout << "image " << i << " done!" << endl;
		cv::resize(img,min_img,cv::Size(),800./img.cols,800./img.cols);
		cv::imshow("roi" + str_i.str(),min_img);
		cv::waitKey(10);
	}
}

void create_hard_negative()
{
	string patch = "C:\\Users\\Iurii\\Desktop\\not forest\\not (";
	fr::HOG hog(Size(64,64),Size(16,16),Size(8,8),Size(8,8),9,HOG::NORM_L2);
	hog.loadRTrees("model-rf.xml", "simpleRTreesModel");
	for(int i = 1; i <= 65; i++ )
	{
		stringstream str_i;
		str_i << i;
		Mat img = cv::imread(patch + str_i.str() + ").jpg", CV_LOAD_IMAGE_COLOR);
		Mat gray;
		cv::cvtColor(img,gray,CV_BGR2GRAY);
		vector<cv::Rect> locations;
		vector<float> weights;
		/////////////////////////////////
		hog.detectMultiScale(gray,locations,weights);
		for(int j = 0; j < (int)locations.size();j++)
		{
			if((locations[j] & cv::Rect(0,0,img.cols,img.rows)) == locations[j])
			{
				stringstream str_f;
				str_f << (int)(weights[j]*100);
				Mat roi = img(locations[j]);
				GUID guid;
				UuidCreate(&guid);
				char* buffer;
				UuidToString(&guid, (RPC_CSTR*)&buffer);
				string str_guid = buffer;
				cv::imwrite(str_f.str() + "_" + str_guid + ".bmp",roi);
				RpcStringFree((RPC_CSTR*)&buffer);
			}
		}
		cout << "image " << i << " done!" << endl;
	}
}

double test_integral(int n)
{
	Mat gray, integr, img;
	img = cv::imread("D:\\2014\\Forest\\forest_research\\DSCF1211.jpg", CV_LOAD_IMAGE_COLOR);
	cv::cvtColor(img,gray,CV_BGR2GRAY);

	double start = (double)cv::getTickCount();
	for(int i = 0;i < n; ++i)
	{
		cv::integral(gray,integr,CV_32F);
		integr.release();
	}

	return  ((double)cv::getTickCount() - start)*1000./cv::getTickFrequency();
}

double test_cvhog(int n)
{
	Mat gray, integr, img;
	img = cv::imread("D:\\2014\\Forest\\forest_research\\DSCF1211.jpg", CV_LOAD_IMAGE_COLOR);
	cv::cvtColor(img,gray,CV_BGR2GRAY);
	vector<float> desc;
	cv::HOGDescriptor hd = cv::HOGDescriptor(Size(64,64),Size(16,16),Size(8,8),Size(8,8),9);
	double start = (double)cv::getTickCount();
	for(int i = 0;i < n; ++i)
	{
		hd.compute(gray,desc);
	}

	return  ((double)cv::getTickCount() - start)*1000./cv::getTickFrequency();
}

int _tmain(int argc, _TCHAR* argv[])
{
	CpuInfo ci;
	ci.printSelf();
	cout << "use opt= " << (cv::useOptimized()? "true":"false") << endl;
	cout <<	"CV_CPU_MMX= " << (cv::checkHardwareSupport(CV_CPU_MMX) ? "true":"false") << endl;
	cout <<	"CV_CPU_SSE= " << (cv::checkHardwareSupport(CV_CPU_SSE) ? "true":"false") << endl;
	cout <<	"CV_CPU_SSE2= " << (cv::checkHardwareSupport(CV_CPU_SSE2) ? "true":"false") << endl;
	cout <<	"CV_CPU_SSE3= " << (cv::checkHardwareSupport(CV_CPU_SSE3) ? "true":"false") << endl;
	cout <<	"CV_CPU_SSE4_1= " << (cv::checkHardwareSupport(CV_CPU_SSE4_1) ? "true":"false") << endl;
	cout <<	"CV_CPU_SSE4_2= " << (cv::checkHardwareSupport(CV_CPU_SSE4_2) ? "true":"false") << endl;
	cout <<	"CV_CPU_POPCNT= " << (cv::checkHardwareSupport(CV_CPU_POPCNT) ? "true":"false") << endl;
	cout <<	"CV_CPU_AVX= " << (cv::checkHardwareSupport(CV_CPU_AVX) ? "true":"false") << endl;
	test_hog();
	while (!_kbhit())
	{
		cv::waitKey(10);
	}
	cv::destroyAllWindows();
	return 0;
}

