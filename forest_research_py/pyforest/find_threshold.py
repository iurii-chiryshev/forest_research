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

font = {'family': 'Verdana',
        'weight': 'normal'}
rc('font', **font)

path = "C:\\Users\\iurii.chiryshev\\Desktop\\results\\"
t = "t.csv"
a = "alpha.csv"
colors = itertools.cycle(('g','g','g','g','r','r','r','r'))
marker = itertools.cycle(('o','s','v','o','s','v'))
linestyle = itertools.cycle(('-.', '--'))

#################################################################################
# читаем из файла
dataset = pd.read_csv(path + t,sep=";")
# получаем размерность
rows, cols = dataset.shape
print("ROWS:", rows, "COLS:", cols)

AXES = dataset.axes[1].values[1:cols] # заголовки исключая первый столбец - X
DATA = dataset.values
X = DATA[:,0].astype(int) # первый столбец - значения по Х
Y = DATA[:,1:cols].astype(float) # остальные столбцы - Y
trans_scores = {'f1':u'F-мера','Precision':u'Точность','Recall':u'Полнота'}

#plt.subplot(1, 2, 1)
mcount = (cols-1) // 2
plt.subplot(2, 1, 1)
for y in np.arange(0, mcount,1):
    if y in [2,3]:
        continue
    if y in [0,1]:
        plt.plot(X, Y[:,y],label = trans_scores[AXES[y]], marker = next(marker))#,yerr=Y[:,y + mcount],linestyle=next(linestyle),linewidth = 1.5)
    else: # F- метрика
        plt.plot(X, Y[:,y],label = "F2-мера",marker = next(marker))#,linestyle='-', linewidth = 1.5)
plt.grid(b=True, which='major', linestyle='-')
plt.grid(b=True, which='minor', linestyle=':')
plt.xticks(np.arange(-5, 60,5))
plt.legend(loc='lower right')
plt.xlabel('P')
plt.ylabel('')

#plt.show()

################################################################################################################
# читаем из файла
dataset = pd.read_csv(path + a,sep=";")
# получаем размерность
rows, cols = dataset.shape
print("ROWS:", rows, "COLS:", cols)

AXES = dataset.axes[1].values[1:cols] # заголовки исключая первый столбец - X
DATA = dataset.values
X = DATA[:,0].astype(int) # первый столбец - значения по Х
Y = DATA[:,1:cols].astype(float) # остальные столбцы - Y

X2 = np.array([ 1 / (2**x) for x in X])



#plt.subplot(1, 2, 2)
mcount = (cols-1) // 2
plt.subplot(2, 1, 2)
for y in np.arange(0, mcount,1):
    if y in [2,3]:
        continue
    if y in [0,1]:
        plt.plot(X2, Y[:,y],label = trans_scores[AXES[y]],marker = next(marker))#,yerr=Y[:,y + mcount],linestyle=next(linestyle),linewidth = 1.5)
    else: # F- метрика
        plt.plot(X2, Y[:,y],label = "F2-мера",marker = next(marker))#,linestyle='-', linewidth = 1.5)
plt.xscale('log')
plt.grid(b=True, which='major', linestyle='-')
plt.grid(b=True, which='minor', linestyle=':')
plt.legend(loc='lower left')
plt.xlabel(r'$ \alpha $')
plt.ylabel('')

plt.show()
