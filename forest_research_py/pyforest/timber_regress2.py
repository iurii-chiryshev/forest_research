__author__ = 'iurii.chiryshev'

import itertools
from sklearn import cross_validation, decomposition
from sklearn.cross_validation import KFold, train_test_split
from sklearn.ensemble import RandomForestClassifier, GradientBoostingClassifier
import matplotlib.pyplot as plt
import numpy as np
from pandas import DataFrame, read_csv
import pandas as pd
from sklearn.metrics import roc_curve, auc, precision_recall_curve
from ranking import detection_error_tradeoff
from settings import CONFIG
from matplotlib import rc
import statsmodels.api as sm

lowess = sm.nonparametric.lowess

font = {'family': 'Verdana',
        'weight': 'normal'}
rc('font', **font)

path = "C:\\Users\\iurii.chiryshev\\Desktop\\results\\"
timber = "_timber1.csv"

colors = itertools.cycle(('g', 'g', 'g', 'g', 'r', 'r', 'r', 'r'))
marker = itertools.cycle(('o', 's', 'v', '*', 'o', 's', 'v', '*'))
linestyle = itertools.cycle(('-.', '--'))

terr = dict()
nts = [1, 2, 3, 4, 5 , 6]
for i,s in enumerate(nts):
    terr[i] = {'mean': [] , 'std': [], 'label': []}

for index, ntinmber in enumerate(nts):
    dataset = pd.read_csv(path + "_timber" + str(ntinmber) + ".csv", sep=";")
    # получаем размерность
    rows, cols = dataset.shape
    print("ROWS:", rows, "COLS:", cols)

    AXES = dataset.axes[1].values[1:cols]  # заголовки исключая первый столбец - X
    DATA = dataset.values
    X = DATA[:, 0].astype(float)  # первый столбец - значения по Х
    Y = DATA[:, 1].astype(float)  # остальные столбцы - Y
    Y *= 2  # диаметр
    STD = DATA[:, 2].astype(float)  # остальные столбцы - разброс

    # как бы истинный размер бревна
    GT = lowess(Y, X, frac=3.0 / 4.0, return_sorted=False)
    tval = 0
    for v in np.arange(6):
        GT = np.array([x if Y[i] - x > tval else Y[i] for i, x in enumerate(GT)])
        GT = lowess(GT, X, frac=3.0 / 4.0, return_sorted=False)  # lowess

    diff = np.array((Y - GT)**2)
    terr[index]['mean'].append(diff.mean())
    terr[index]['std'].append(diff.std())
    terr[index]['label'].append("initial data")

    # lowess
    W = lowess(Y, X, frac=2.0 / 3.0, return_sorted=False)
    diff = np.array((W - GT)**2)
    terr[index]['mean'].append(diff.mean())
    terr[index]['std'].append(diff.std())
    terr[index]['label'].append("LOWESS")


    # ransac
    from sklearn import linear_model, datasets
    Xt = X.reshape((X.size, 1))
    Yt = Y
    model_ransac = linear_model.RANSACRegressor(linear_model.LinearRegression())
    model_ransac.fit(Xt, Yt)
    Y_ransac = model_ransac.predict(X[:, np.newaxis])
    diff = np.array((Y_ransac - GT)**2)
    terr[index]['mean'].append(diff.mean())
    terr[index]['std'].append(diff.std())
    terr[index]['label'].append("RANSAC")



    # poly
    from sklearn.linear_model import Ridge
    from sklearn.preprocessing import PolynomialFeatures
    from sklearn.pipeline import make_pipeline

    for count, degree in enumerate([1, 3, 5, 7]):
        model = make_pipeline(PolynomialFeatures(degree), Ridge())
        model.fit(Xt, Yt)
        Y_poly = model.predict(X[:, np.newaxis])
        diff = np.array((Y_poly - GT)**2)
        terr[index]['mean'].append(diff.mean())
        terr[index]['std'].append(diff.std())
        terr[index]['label'].append("polynom \n %d degree" % degree)


labels = terr[0]['label']
cnt = len(labels)
for i, lab in enumerate(labels):
    m = []
    for j, val in enumerate(nts):
        m.append(terr[j]['mean'][i])
    npm = np.array(m)
    print('%s MSE= %f +- %f' % (lab,npm.mean(),npm.std()))

