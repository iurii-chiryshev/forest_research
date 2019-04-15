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

n = 1

terr = dict()
for i,s in enumerate([1, 2, 3]):
    terr[i] = {'mean': [] , 'std': [], 'label': []}

for index, ntinmber in enumerate([1, 2, 3]):
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
    terr[index]['label'].append("выборка")

    # lowess
    W = lowess(Y, X, frac=2.0 / 3.0, return_sorted=False)
    plt.subplot(3, 3, index + 1)
    n += 1
    plt.errorbar(X, Y, label="выборка", linestyle='-', color='b')  # ,yerr = STD,errorevery=15)
    #plt.plot(X, GT, label=" ", color='g', linewidth=1.5)
    plt.plot(X, W, label="LOWESS", color='r', linewidth=1.5)
    plt.legend(loc='lower center', mode='expand', ncol=3)
    plt.grid(b=True, which='major', linestyle='-')
    plt.grid(b=True, which='minor', linestyle=':')
    plt.ylabel('Диаметр, см')
    plt.xlabel('Длина, см')

    diff = np.array((W - GT)**2)
    terr[index]['mean'].append(diff.mean())
    terr[index]['std'].append(diff.std())
    terr[index]['label'].append("LOWESS")

    Xt = X.reshape((X.size, 1))
    Yt = Y
    # ransac
    from sklearn import linear_model, datasets

    model_ransac = linear_model.RANSACRegressor(linear_model.LinearRegression())
    model_ransac.fit(Xt, Yt)
    Y_ransac = model_ransac.predict(X[:, np.newaxis])

    plt.subplot(3, 3, index + 1 + 3)
    n += 1
    plt.errorbar(X, Y, label="выборка", linestyle='-', color='b')  # ,yerr = STD,errorevery=15)
    #plt.plot(X, GT, label=" ", color='g', linewidth=1.5)
    plt.plot(X, Y_ransac, label="RANSAC", color='r', linewidth=1.5)
    plt.legend(loc='lower center', mode='expand', ncol=3)
    plt.grid(b=True, which='major', linestyle='-')
    plt.grid(b=True, which='minor', linestyle=':')
    plt.ylabel('Диаметр, см')
    plt.xlabel('Длина, см')

    diff = np.array((Y_ransac - GT)**2)
    terr[index]['mean'].append(diff.mean())
    terr[index]['std'].append(diff.std())
    terr[index]['label'].append("RANSAC")



    # poly
    from sklearn.linear_model import Ridge
    from sklearn.preprocessing import PolynomialFeatures
    from sklearn.pipeline import make_pipeline

    plt.subplot(3, 3, index + 1 + 6)
    n += 1
    plt.errorbar(X, Y, label="выборка", linestyle='-', color='b')  # ,yerr = STD,errorevery=15)
    #plt.plot(X, GT, label=" ", color='g', linewidth=1.5)

    clrs = ['r', 'r', 'r', 'r']
    ls = ['-', '--', '-.', ':']
    for count, degree in enumerate([1, 3, 5, 7]):
        model = make_pipeline(PolynomialFeatures(degree), Ridge())
        model.fit(Xt, Yt)
        Y_poly = model.predict(X[:, np.newaxis])
        plt.plot(X, Y_poly, color=clrs[count], linestyle=ls[count], linewidth=1.5, label=str(degree))

        diff = np.array((Y_poly - GT)**2)
        terr[index]['mean'].append(diff.mean())
        terr[index]['std'].append(diff.std())
        terr[index]['label'].append("%d-ая\nстепень" % degree)

    plt.legend(loc='lower center', mode='expand', ncol=6)
    plt.grid(b=True, which='major', linestyle='-')
    plt.grid(b=True, which='minor', linestyle=':')
    plt.ylabel('Диаметр, см')
    plt.xlabel('Длина, см')

plt.show()
colors = ['b', 'r', 'r', 'r', 'r', 'r', 'r']
for index, ntinmber in enumerate([1, 2, 3]):
    plt.subplot(3, 3, index + 1)
    score = terr[index]
    score_len = len(score['mean'])
    bar_width = 0.8/score_len
    x = np.arange(score_len)
    plt.bar(x,score['mean'],color = colors)#,yerr = score['std'],ecolor = 'black')
    plt.ylabel('MSE')
    plt.xticks(x + bar_width*score_len/2, score['label'])
    plt.legend(loc='best')
    #plt.tight_layout()
    plt.grid(True)
plt.show()