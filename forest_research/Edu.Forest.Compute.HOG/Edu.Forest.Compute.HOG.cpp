// Edu.Forest.Compute.HOG.cpp: определяет точку входа для консольного приложения.
//

#include "stdafx.h"
#include "Edu.Forest.Imgproc/forest_hog.h"
#include <iostream>
#include <fstream>
#include <conio.h>
#include <stdio.h>
#include <time.h>
using namespace cv;
using namespace fr;
using namespace std;

enum 
{
	TRANS_NONE = 1,
	TRANS_FLIP = 2,
};

//************************************
// Method:    get_img_names
// Parameter: std::string fileName
// Parameter: vector<string> & imgs
// получить список всех изображений из txt файла
//************************************
void load_images(std::string fileName, vector<string>& imgs)
{
	string str;
	std::ifstream file(fileName.c_str());
	if ( !file.is_open() )
		return;
	while( !file.eof() )
	{
		std::getline(file, str);
		if (str.empty()) break;
		imgs.push_back(str);
	}
	file.close();
}

void drawEv(Mat& src,Mat& dst)
{
	double maxVal, minVal;
	cv::minMaxIdx(src,&minVal,&maxVal);
	cout << maxVal << " " << minVal << endl;
	cv::Size dsz(320,320);
	dst = Mat(dsz,CV_8UC3);
	dst = cv::Scalar::all(0);

	int n = 10;
	for(int i = 0; i <= n; i++)
	{
		float scale = (float)i/n;
		cv::line(dst,cv::Point(0,(int)(scale*dsz.height)),cv::Point(dsz.width,(int)(scale*dsz.height)),cv::Scalar(128,128,128),1);
		cv::line(dst,cv::Point((int)(scale*dsz.width),0),cv::Point((int)(scale*dsz.width),dsz.height),cv::Scalar(128,128,128),1);
	}


	Mat buf = Mat(src.rows + 1,src.cols,src.type(),cv::Scalar::all(0));
	for(int i = 0; i < src.rows; i++)
	{
		buf.at<float>(i+1,0) = buf.at<float>(i,0) + src.at<float>(i,0);
	}
	double xScale = (double)(dsz.width)/src.rows;
	double yScale = (double)(dsz.height)/buf.at<float>(src.rows);
	for(int i = 0; i < src.rows; i++)
	{
		cv::circle(dst,cv::Point((int)(i*xScale),dsz.height - (int)(buf.at<float>(i+1,0)*yScale)),2,cv::Scalar(0,255,0));
	}
}

void saveToCsv(const std::string& fileName, const Mat& samples, const Mat& response) {
	std::fstream outputFile;
	outputFile.open(fileName, std::ios::out);
	CV_Assert(samples.rows == response.rows && "the images must be same rows");
	CV_Assert(samples.cols > 0 && "sample cols must be non zero");
	const int rows = samples.rows, cols = samples.cols;
	// первый столбец в файле - это метки классов response
	for (int i = 0; i<rows; i++)
	{
		float *sptr = (float*)samples.ptr(i);
		int *rptr = (int*)response.ptr(i);
		// первый столбец в файле - это метки классов response
		outputFile << rptr[0];
		// все остальное - это данные
		for (int j = 0; j<cols; j++)
		{
			outputFile << "," << sptr[j];
		}
		outputFile << endl;

	}
	outputFile.close();
}

struct SampleInvoker
{
	SampleInvoker(Ptr<HOG> hog,vector<string>& names,Mat& samples,int transform, int scale,int winSize)
	{
		_hog = hog;
		_names = &names;
		_samples = &samples;
		_transform = transform;
		_scale = scale;
		_winSize = winSize;
	}
	void operator()(const Range& range)
	{
		const int	y0 = range.start,
					y1 = range.end,
					step = _samples->step;
		const vector<string>& names = *_names;
		RNG rng(time(NULL));
		for(int y = y0; y< y1;y++)
		{
			uchar* samplePtr = _samples->data + step*y*_scale;
			Mat img = cv::imread(names[y],CV_LOAD_IMAGE_COLOR);
			/*должно быть квадратное и не пустое*/
			CV_Assert( img.data  && (img.cols == img.rows) && "the image must be non-empty and square");
			Mat gray_img,scale_img;
			cvtColor(img,gray_img,CV_BGR2GRAY);

			double scaleFactor = double(_winSize + 2)/img.cols;
			cv::Rect roi(1,1,_winSize,_winSize);
			cv::resize(gray_img,scale_img,Size(),scaleFactor,scaleFactor);
			/*применить*/
			_hog->computeOne(scale_img(roi),(float*)samplePtr);
			if((_transform & TRANS_FLIP) == TRANS_FLIP)
			{
				samplePtr += step;
				//for(int i = -1; i <= 1; i++,samplePtr += step)
				//{
					cv::Mat flip_img;
					cv::flip(scale_img,flip_img,rng.uniform(-1,2));
					/*применить*/
					_hog->computeOne(flip_img(roi),(float*)samplePtr);
				//}
				
			}
			cout << y << " samples ready... \r";
		}
		cout << endl;
	}
private:
	Ptr<HOG> _hog;
	vector<string>* _names;
	Mat* _samples;
	int _transform;
	int _scale;
	int _winSize;

};

int _tmain(int argc, _TCHAR* argv[])
{
	/*парсим командную строку*/
	int transform  = (int)TRANS_FLIP,
		win_size = 32,
		nbins = 9,
		norm = HOG::NORM_L2,
		nsamples,
		npos,
		scale;
	if(argc == 1)
	{
		cout << "usage: " << argv[0] << endl;
		cout << "  [-fl <flip  images, if need>]" << endl;
		cout << "  [-ws <window size = "<< win_size << ">]" << endl;
		cout << "  [-nb <number of bins = "<< nbins << ">]" << endl;
		cout << "  [-norm <norma = "<< norm << ">] [1,2,3 -> L1,L2,L2hys]" << endl;
	}
	for(int i = 1; i < argc; i++ )
	{
		if( !strcmp( argv[i], "-fl" ) )
		{
			transform |= (int)TRANS_FLIP;
		}
		else if( !strcmp( argv[i], "-ws" ) )
		{
			win_size = atoi( argv[++i] );
			CV_Assert(win_size == 64 || win_size == 32);
		}
		else if( !strcmp( argv[i], "-nb" ) )
		{
			nbins = atoi( argv[++i] );
		}
		else if( !strcmp( argv[i], "-norm" ) )
		{
			norm = atoi( argv[++i] );
			CV_Assert(norm == HOG::NORM_L1 || norm == HOG::NORM_L2 || norm == HOG::NORM_L2HYS);
		}
	}
	scale = transform;
	/*читаем из файлов имена файлов*/
	vector<string> imgs;
	cout << "reading positive.txt..."<< endl;
	load_images("positive.txt",imgs);
	npos = (int)imgs.size();
	cout << "reading negative.txt..."<< endl;
	load_images("negative.txt",imgs);
	nsamples = (int)imgs.size();
	cout << "read " << nsamples << " samples\n";
	cout << npos << " - positive samples\n";
	cout << (nsamples - npos) << " - negative samples\n";
	if(npos == 0 || npos == nsamples)
	{
		return 1;
	}

	/*объект для расчета дескрипторов*/
	Ptr<HOG> hog(new HOG(cv::Size(win_size, win_size),
						cv::Size(win_size >> 2, win_size >> 2),
						cv::Size(win_size >> 3, win_size >> 3),
				
		Size(win_size >> 3, win_size >> 3),
						nbins,
						norm));
	/*матрица примеров*/
	Mat samples(nsamples*scale,hog->getDescriptorSize(),CV_32FC1);
	/*матрица ответов 1-positive, 0-negative*/
	Mat	response(nsamples*scale,1,CV_32SC1,cv::Scalar::all(0));

	/*заполняем примеры*/
	SampleInvoker si(hog,imgs,samples,transform,scale,win_size);
	si(Range(0,nsamples));
	/*заполнить ответы*/
	for(int i = 0; i < scale*npos; ++i)
	{
		response.at<int>(i,0) = 1;
	}
	/*запись в xml файл*/
	stringstream ss;
	ss << "HOG" << win_size << "-" << nbins;
	cout << "writing in files..." << endl;
	std::string xml = ss.str() + ".xml";
	FileStorage fs(xml, CV_STORAGE_WRITE);
	if( fs.isOpened() )
	{
		fs << "positive" << Range(0,scale*npos) << "response" << response << "samples" << samples;
		fs.release();
		cout << "file " << xml << " successfully written" << endl;
	}
	else
	{
		cout << "can't write file" << endl;
	}
	/*запись в csv файл*/
	std::string csv = ss.str() + ".csv";
	saveToCsv(csv, samples, response);
	cout << "file " << csv << " successfully written" << endl;

	/*метод главных компонент*/
	cout << "computing PCA..." << endl;
	Mat posSamples = samples(Rect(0,0,samples.cols,600/*scale*npos*/));
	cv::PCA _pca(posSamples,noArray(),CV_PCA_DATA_AS_ROW,1024);
	Mat ev,ev_img;
	_pca.eigenvalues.convertTo(ev,_pca.eigenvalues.type());
	drawEv(ev,ev_img);
	imshow("eigenvalues", ev_img);
	
	/*показываем результат*/
	Mat minsampl, gsampl;
	double f = samples.rows > samples.cols ? 640.0/samples.rows : 640.0/samples.cols;
	cv::resize(samples,minsampl,cv::Size(),f,f);
	double minVal, maxVal;
	cv::minMaxIdx(minsampl,&minVal,&maxVal);
	minsampl.convertTo(gsampl,CV_8UC1, 255.0/(maxVal - minVal), -minVal);
	imshow("samples", gsampl);



	
	/*ждем нажатия любой клавиши и выходим*/
	while (!_kbhit())
	{
		cv::waitKey(10);
	} 
	cv::destroyAllWindows();
	return 0;
}

