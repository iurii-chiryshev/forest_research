#pragma once

#ifdef FOREST_IMGPROC_EXPORTS
#define FOREST_PROC_API __declspec(dllexport) 
#else
#define FOREST_PROC_API __declspec(dllimport) 
#endif /*EDUFORESTIMGPROC_EXPORTS*/

