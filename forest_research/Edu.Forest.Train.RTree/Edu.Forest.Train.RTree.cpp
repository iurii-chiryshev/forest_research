// Edu.Forest.Train.RTree.cpp: определяет точку входа для консольного приложения.
//

#include "stdafx.h"
#include "Edu.Forest.Imgproc/forest_hog.h"
#include <iostream>
#include <fstream>
#include <conio.h>
#include <stdio.h>
using namespace cv;
using namespace fr;
using namespace std;

bool writeSamples(const std::string& filename,Mat& samples, Mat& response, Range& positive)
{
	cout << "readind " << filename << "..." << endl;
	FileStorage fs(filename, CV_STORAGE_READ);
	if( fs.isOpened() )
	{
		fs["positive"] >> positive;
		fs["response"] >> response;
		fs["samples"] >> samples;
		fs.release();
		return true;
	}
	else
	{
		cout << "error read file: " << filename << endl;
		return false;
	}
	return false;
}

void createROC(const Mat& src, Mat& dst, cv::Size size)
{
	CV_Assert( src.rows == 2 && ( src.type() == CV_32FC1 || src.type() == CV_64FC1 ) );
	cv::Size dsz = ( (size == cv::Size() || (size.height != size.width)) ? Size(320,320) : size);
	dst.create(dsz,CV_8UC3);
	dst = Scalar::all(80);
	/*диагональ*/
	cv::line(dst,cv::Point(0,dsz.height),cv::Point(dsz.width,0),cv::Scalar(128,128,128),1);
	/*сетка*/
	int n = 10;
	for(int i = 0; i <= n; i++)
	{
		float scale = (float)i/n;
		cv::line(dst,cv::Point(0,(int)(scale*dsz.height)),cv::Point(dsz.width,(int)(scale*dsz.height)),cv::Scalar(128,128,128),1);
		cv::line(dst,cv::Point((int)(scale*dsz.width),0),cv::Point((int)(scale*dsz.width),dsz.height),cv::Scalar(128,128,128),1);
	}
	/*сами точки*/
	for(int i = 0; i < src.cols; i++)
	{
		cv::Point pt = cv::Point((int)(src.at<float>(0,i)*dsz.width),
			dsz.height - (int)(src.at<float>(1,i)*dsz.height));
		cv::circle(dst,pt,1,Scalar(0,255,0),2);
	}
}

void createSpSe(const Mat& src, Mat& dst, cv::Size size)
{
	CV_Assert( src.rows == 4 && ( src.type() == CV_32FC1 || src.type() == CV_64FC1 ) );
	cv::Size dsz = ( (size == cv::Size() || (size.height != size.width)) ? Size(320,320) : size);
	dst.create(dsz,CV_8UC3);
	dst = Scalar::all(80);
	/*сетка*/
	int n = 10;
	for(int i = 0; i <= n; i++)
	{
		float scale = (float)i/n;
		cv::line(dst,cv::Point(0,(int)(scale*dsz.height)),cv::Point(dsz.width,(int)(scale*dsz.height)),cv::Scalar(128,128,128),1);
		cv::line(dst,cv::Point((int)(scale*dsz.width),0),cv::Point((int)(scale*dsz.width),dsz.height),cv::Scalar(128,128,128),1);
	}
	/*сами точки*/
	for(int i = 0; i < src.cols; i++)
	{
		
		cv::Point pt0 = cv::Point((int)(src.at<float>(0,i)*dsz.width),
			dsz.height - (int)(src.at<float>(1,i)*dsz.height));
		cv::circle(dst,pt0,1,Scalar(0,255,0),2);

		cv::Point pt1 = cv::Point((int)(src.at<float>(2,i)*dsz.width),
			dsz.height - (int)(src.at<float>(3,i)*dsz.height));
		cv::circle(dst,pt1,1,Scalar(255,0,0),2);
	}
}

int _tmain(int argc, _TCHAR* argv[])
{
	Mat samples, response; // признаки и ответы
	Range positive; // диапазон, где лежат бревна
	/*читаем из файла*/
	if(writeSamples("HOG64-9.xml",samples,response,positive) == false)
	{
		return 1;
	}
	int nsamples = samples.rows, dimension = samples.cols;
	cout << "  dimension= " << dimension << endl;
	cout << "  number of samples= " << nsamples << endl;
	cout << "  number of positive samples= " << positive.end << endl;
	cout << "  number of negative samples= " << (nsamples - positive.end) << endl;
	/*матрица, отвечающая за то на каких примерах алгоритм
	  будет обучаться а на каких проверяться */
	Mat sampleIdx(nsamples,1,CV_8U,Scalar(0));
	for(int i = positive.start; i < positive.end; i+= 2)
	{
		/*из положительных примеров возьмем каждый третий*/
		sampleIdx.at<uchar>(i,0) = 1;
	}
	for(int i = positive.end; i < nsamples; i+= 2)
	{
		/*из отрицательных примеров возьмем каждый второй*/
		sampleIdx.at<uchar>(i,0) = 1;
	}
	/*создаем random forest*/
	CvRTParams params;
	params.max_depth = 15/*15*/; // глубина дерева
	params.min_sample_count = 1; //
	/*params.nactive_vars = std::sqrt(dimension*2);*/
	params.calc_var_importance = false;
	params.term_crit.type = CV_TERMCRIT_ITER + CV_TERMCRIT_EPS; // критерий останова
	params.term_crit.max_iter = 100 /*200*/; // количество деревьев
	params.term_crit.epsilon = 0.001; // oob ошибка
	CvERTrees rf; 
	Mat varIdx(1, dimension, CV_8U, Scalar(1)); 
	Mat varTypes(1, dimension + 1, CV_8U, Scalar(CV_VAR_ORDERED)); 
	varTypes.at<uchar>(dimension) = CV_VAR_CATEGORICAL; 
	/*тренеруем*/
	cout << "random trees params" << endl;
	cout << "  max depth= " << params.max_depth << endl;
	cout << "  min sample count= " << params.min_sample_count << endl;
	cout << "  number of variables= " << (int)(params.nactive_vars <= 0 ? std::sqrt(dimension): params.nactive_vars) << endl;
	cout << "  max number of trees in the forest (max iter)= " << params.term_crit.max_iter << endl;
	cout << "  forest accuracy, oob error (epsilon)= " << params.term_crit.epsilon << endl;
	cout << "trainig random trees..." << endl;
	rf.train(samples, CV_ROW_SAMPLE,response, varIdx, sampleIdx, varTypes, Mat(), params);
	/*записывваем*/
	rf.save("model-rf.xml", "simpleRTreesModel");
	// вычисляем ошибку на обучающей выборке 
	cout << "computing train error..." << endl;
	float trainError = 0.0f; 
	int ntrainSamples = 0;
	for (int i = 0; i < nsamples; ++i) 
	{ 
		if(sampleIdx.at<uchar>(i,0) != 0) // на которых обучались
		{
			ntrainSamples++;
			int prediction = (int)(rf.predict(samples.row(i))); 
			if(response.at<int>(i,0) != prediction)
			{
				trainError += 1;
			}
		}
	} 
	trainError /= float(ntrainSamples);

	// вычисляем ошибку на тестовой выборке
	cout << "computing test error..." << endl;
	float testError = 0.0f; 
	int ntestSamples = 0;
	for (int i = 0; i < nsamples; ++i) 
	{ 
		if(sampleIdx.at<uchar>(i,0) == 0) // на которых не обучались
		{
			ntestSamples++;
			int prediction = (int)(rf.predict(samples.row(i))); 
			if(response.at<int>(i,0) != prediction)
			{
				testError += 1;
			}
		}
	} 
	testError /= float(ntestSamples);

	cout << "train samples= " << ntrainSamples << ", train error= " << trainError << endl;
	cout << "test samples= " << ntestSamples << ", test error= " << testError << endl;

	/*ROC- анализ*/
	cout << "computing ROC curve..." << endl;
	int n = 100; // количество точек для roc-кривой
	Mat roc(2,n+2,CV_32FC1,cv::Scalar::all(0));
	roc.at<float>(0,0) = roc.at<float>(1,0) = 0; // первая точка (0,0)
	roc.at<float>(0,n+1) = roc.at<float>(1,n+1) = 1; // последняя точка (1,1)
	Mat se_sp(4,n,CV_32FC1,cv::Scalar::all(0));
	for(int i = 0; i < n; i++ )
	{
		float threshProb = float(i)/n; // порог принятия решения
		float prob, truePosRate,falsePosRate;
		int truePos = 0, trueNeg = 0, falseNeg = 0, falsePos = 0;
		for (int j = positive.start; j < positive.end; j++)
		{
			/*идем по положительным примерам, на которых не обучались*/
			if(sampleIdx.at<uchar>(j,0) == 0) 
			{
				prob = rf.predict_prob(samples.row(j));
				if(prob > threshProb)
					truePos++; // верно классифицированные положительные примеры
				else
					falseNeg++; //положительные примеры, классифицированные как отрицательные (ошибка I рода)
			}
		}

		for(int j = positive.end; j < nsamples; j++)
		{
			/*идем по отрицательным примерам, на которых не обучались*/
			if(sampleIdx.at<uchar>(j,0) == 0) 
			{
				prob = rf.predict_prob(samples.row(j));
				if(prob > threshProb)
					falsePos++; // отрицательные примеры, классифицированные как положительные (ошибка II рода)
				else
					trueNeg++; //верно классифицированные отрицательные примеры
			}
		}
		truePosRate = float(truePos) / (truePos + falseNeg); // доля истинно положительных случаев (y)
		falsePosRate = float(falsePos) / (falsePos + trueNeg); // (x)
		roc.at<float>(0,i+1) = falsePosRate;
		roc.at<float>(1,i+1) = truePosRate;
		/*ошибки*/
		se_sp.at<float>(0,i) = se_sp.at<float>(2,i) = threshProb;
		se_sp.at<float>(1,i) = truePosRate;
		se_sp.at<float>(3,i) = 1.f - falsePosRate;
	}
	Mat imgROC,imgSpSe;
	createROC(roc,imgROC,cv::Size());
	createSpSe(se_sp,imgSpSe,cv::Size());
	imshow("ROC",imgROC);
	imshow("Sp\\Se",imgSpSe);

	while (!_kbhit())
	{
		cv::waitKey(10);
	} 
	cv::destroyAllWindows();
	return 0;
}

