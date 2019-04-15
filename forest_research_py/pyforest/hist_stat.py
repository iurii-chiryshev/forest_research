import string

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


# читаем из файла
dataset = pd.read_csv("C:/Users/iurii.chiryshev/Desktop/stat.txt",sep=";")
# получаем размерность
rows, cols = dataset.shape
print("ROWS:", rows, "COLS:", cols)

AXES = dataset.axes[1].values[1:cols]
DATA = dataset.values
X = DATA[:,0].astype(int) # первый столбец - значения по Х
Y = DATA[:,1:cols].astype(float) # остальные столбцы - Y



colors = itertools.cycle(('g','g','g','g','r','r','r','r'))
marker = itertools.cycle(('o','s','v','*','o','s','v','*'))
plt.subplot(2, 2, 1)
for y in np.arange(0,cols-1,2):
    plt.plot(X, Y[:,y],marker = next(marker), label = AXES[y],color = next(colors))
plt.grid(b=True, which='major', linestyle='-')
#plt.grid(b=True, which='minor', linestyle=':')
plt.legend(loc='best')

plt.subplot(2, 2, 2)
for y in np.arange(1,cols-1,2):
    plt.plot(X, Y[:,y],marker = next(marker),label = AXES[y],color = next(colors))
plt.grid(b=True, which='major', linestyle='-')
#plt.grid(b=True, which='minor', linestyle=':')
plt.legend(loc='best')


plt.show()
