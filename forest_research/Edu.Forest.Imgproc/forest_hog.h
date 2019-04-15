#pragma once
#include "forest_export.hpp"
#include "forest_opencv.hpp"

#include <string>
#include <math.h>
#include <vector>

namespace fr{
	using namespace cv;
	using namespace std;

#pragma warning(push)
#pragma warning(disable:4251) // ��������, ���� �� �������� ���������
	class FOREST_PROC_API HOG
	{
	public:
		/*����� ��� ����������*/
		enum
		{
			NORM_L1 = 1,
			NORM_L2 = 2,
			NORM_L2HYS = 3,
		};
		/*����� �������*/
		enum { DEFAULT_NLEVELS=64 };
		/*������� ������ ��� hog*/
		class MemoryManager
		{
		public:
			MemoryManager(HOG* hogPtr,cv::Mat& img,cv::Size& winStride);
			~MemoryManager();
			/*����������� ���������*/
			void recount(cv::Mat& newImg);
			int getXmapBytes(const cv::Size& imgSize) const;
			int getYmapBytes(const cv::Size& imgSize) const;
			int getIntegrBufBytes(const cv::Size& imgSize) const;
			int getGradsBytes(const cv::Size& imgSize) const;
			int getCellsBytes(const cv::Size& imgSize) const;
			int getBlocksBytes(const cv::Size& imgSize) const;
			int getWinsBytes(const cv::Size& imgSize) const;
			cv::Size getNCells(const cv::Size& imgSize) const;
			cv::Size getNBlocks(const cv::Size& imgSize) const;
			cv::Size getNWins(const cv::Size& imgSize) const;
			cv::Size getWinStride() const;
			Mat image;
			Mat grads;
			Mat cells;
			Mat blocks;
			Mat wins;
			int* xmap;
			int* ymap;
			float* integrBuf;
		private:
			HOG* _hogPtr;
			void* _memPtr;
			cv::Size _imgSize;
			cv::Size _winStride;
			MemoryManager();
			MemoryManager(const MemoryManager& mm);
		};
		//************************************
		// Method:    ctor
		// Parameter: cv::Size win_size - ������ ����, ����
		// Parameter: cv::Size block_size - ������ �����, ����
		// Parameter: cv::Size block_stride - ��� �������� �����, ����
		// Parameter: cv::Size cell_size - ������ ������, ����
		// Parameter: int nbins - ����� ����������� ���������
		// Parameter: int norma - ����������
		// Parameter: double threshold_L2hys - �����, ���� ������������ NORM_L2HYS ����������
		// Parameter: bool use_gamma - ������������ �����?
		// Parameter: double gamma - ����������� �����
		// Parameter: int nlevels - ����� ������� ��� ����� ��������� 
		//************************************
		HOG(cv::Size win_size= cv::Size(64, 64), cv::Size block_size=cv::Size(16, 16),
			cv::Size block_stride=cv::Size(8, 8), cv::Size cell_size=Size(8, 8),
			int nbins=9, int norma = HOG::NORM_L2, double threshold_L2hys=0.2);
		~HOG();

		//************************************
		// Method:    getDescriptorSize
		// Returns:   int
		// ����� �����������
		//************************************
		int getDescriptorSize() const;

		//************************************
		// Method:    getNBlocks
		// Returns:   cv::Size
		// ���������� ������ � ����
		//************************************
		cv::Size getNBlocks() const;

		//************************************
		// Method:    getNCells
		// Returns:   cv::Size
		// ���������� ����� � �����
		//************************************
		cv::Size getNCells() const;
 
		//************************************
		// Method:    computeOne
		// Parameter: Mat & win - ������� ����������� �������� win_size � ����� CV_8UC1
		// Parameter: float * descr - ��������� �� ������ ����������� ������ getDescriptorSize
		// ��������� ���������� ��� ����
		//************************************
		void computeOne(Mat& img,float* descr);

		void compute(Mat& img,int n);

		void detectMultiScale(Mat& img,vector<Rect>& foundLocations,vector<float>& foundWeights);

		void loadRTrees(const string& filename, const string& name);

	private:
		cv::Size _winSize;
		cv::Size _blockSize;
		cv::Size _blockStride;
		cv::Size _cellSize;
		/*����� ����������� ��� �����������*/
		int _nbins;
		/*��� ���������� ������*/
		int _norma;
		/*����� � ������ L2hys �����*/
		double _L2hysThreshold;
		/*������� ��� �����������*/
		cv::Mat _lut_qangle;
		/*������� ��� ��������� */
		cv::Mat _lut_grad;
		/*random tree*/
		Ptr<CvRTrees> _rtrees;
	
		//************************************
		// Method:    initLUT
		// ������������� ������ ��� �������� ������� ����������
		//************************************
		void initLUT();

		//************************************
		// Method:    computeGradient
		// Parameter: const Mat& image - ������� �����������, �����
		// Parameter: Mat& gradient - �������� �����������, �������� nbins
		// ������ ��������� �����������
		//************************************
		void computeGradient(Ptr<MemoryManager> memoryManager);
		
		//************************************
		// Method:    precompWins
		// Parameter: Mat& image
		// Parameter: Mat& blocks
		// Parameter: cv::Size& winStride
		// ������ ������������ ���� ����
		//************************************
		void precompWins(Ptr<MemoryManager> memoryManager);

		HOG(const HOG& hog);
	};























#pragma warning(pop)
}
	


