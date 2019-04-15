from sklearn import cross_validation, decomposition
from sklearn.cross_validation import KFold, train_test_split
from sklearn.ensemble import RandomForestClassifier
import matplotlib.pyplot as plt
import numpy as np
from sklearn.metrics import roc_curve, auc, precision_recall_curve
from ranking import detection_error_tradeoff
from settings import CONFIG

__author__ = 'iurii.chiryshev'

# читаем из файла
dataset = np.loadtxt(CONFIG['csv_name'],delimiter=",")
# получаем размерность
rows, cols = dataset.shape
print("ROWS:", rows, "COLS:", cols)
# в первом столбце лежат ответы (y) все остальное - это вектора признаков
# вытаскиваем их
X = dataset[:,1:cols]
y = dataset[:,0].astype(int)

last_index = np.where(y==1)[0][-1]
print(last_index)
pos_X = X[last_index:rows-1]

pca = decomposition.PCA()
pca.fit(pos_X)

# plt.figure(1, figsize=(4, 3))
# plt.clf()
# plt.axes([.2, .2, .7, .7])
plt.plot(pca.explained_variance_, linewidth=2)
plt.xlabel('n_components')
plt.ylabel('explained_variance_')
plt.grid(True)
plt.show()
