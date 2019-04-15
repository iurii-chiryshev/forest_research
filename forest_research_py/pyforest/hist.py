from numpy.ma import sin

__author__ = 'iurii.chiryshev'
import itertools
from sklearn import cross_validation, decomposition
from sklearn.cross_validation import KFold, train_test_split
from sklearn.ensemble import RandomForestClassifier, GradientBoostingClassifier
import matplotlib.pyplot as plt
import numpy as np
from sklearn.metrics import roc_curve, auc, precision_recall_curve
from ranking import detection_error_tradeoff
from settings import CONFIG
from matplotlib import rc

sum = 0
pi = 3.1415926535
pi_2 = pi / 2
step = 200
for i in np.arange(0,step):
    sum = sum + sin(i * pi * 2 / step) * 2 + 1.0
sum = sum / step
sum -= 1.0
sum *= pi_2
sum = 1/sum


# читаем из файла
dataset = np.loadtxt("C:/Users/iurii.chiryshev/Desktop/hist.txt",delimiter=";")
# получаем размерность
rows, cols = dataset.shape
print("ROWS:", rows, "COLS:", cols)

# пеервый столбец - метки класса 0,1
MARKER = dataset[:,0].astype(int)
COLORS = ['r','g','b']
# все остальное

DATA = [dataset[:,i] for i in np.arange(1,cols)]
LABEL = ["disp center","disp circle", "mean center", "mean circle"]



plt.subplot(2, 2, 1)
for u in np.unique(MARKER):
    plt.plot(DATA[0][np.where(MARKER == u)], DATA[1][np.where(MARKER == u)],'.', color=COLORS[u])
plt.xlabel(LABEL[0])
plt.ylabel(LABEL[1])
plt.grid(b=True, which='major', linestyle='-')

plt.subplot(2, 2, 2)
for u in np.unique(MARKER):
    plt.plot(DATA[2][np.where(MARKER == u)], DATA[3][np.where(MARKER == u)],'.', color=COLORS[u])
plt.xlabel(LABEL[2])
plt.ylabel(LABEL[3])
plt.grid(b=True, which='major', linestyle='-')

plt.subplot(2, 2, 3)
for u in np.unique(MARKER):
    plt.plot(DATA[0][np.where(MARKER == u)], DATA[2][np.where(MARKER == u)],'.', color=COLORS[u])
plt.xlabel(LABEL[0])
plt.ylabel(LABEL[2])
plt.grid(b=True, which='major', linestyle='-')


plt.subplot(2, 2, 4)
for u in np.unique(MARKER):
    plt.plot(DATA[0][np.where(MARKER == u)], DATA[3][np.where(MARKER == u)],'.', color=COLORS[u])
plt.xlabel(LABEL[0])
plt.ylabel(LABEL[3])
plt.grid(b=True, which='major', linestyle='-')




plt.show()
