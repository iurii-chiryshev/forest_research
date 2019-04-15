__author__ = 'iurii.chiryshev'

# какие-то постоянные значения
CONFIG = {
    'random_state': 43,
    'csv_name': "D:\\2017\\projects\\foto\\hog\\HOG64-9-30.csv",
    'n_estimators': [16,32,48,64,96,128,192,256,512],
    'n_depth': [1,2,4,6,8,12,16,24,32],
    'scorings' : ['f1','precision','recall'],
    'colors' : ['r','g','b','c','m','y'],
    'path' : "D:\\2017\\projects\\foto\\hog\\",
    'desk_names': ['HOG64-8','HOG64-9','HOG48-8','HOG48-9','HOG32-8','HOG32-9'],
    'trans_scores': {'f1':u'F-мера',u'precision':u'Точность','recall':u'Полнота'},

}

