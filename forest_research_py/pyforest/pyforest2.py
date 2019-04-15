import itertools
import os
from sklearn import cross_validation, decomposition
from sklearn.cross_validation import KFold, train_test_split
from sklearn.ensemble import RandomForestClassifier, GradientBoostingClassifier
import matplotlib.pyplot as plt
import numpy as np
from sklearn.metrics import roc_curve, auc, precision_recall_curve
from ranking import detection_error_tradeoff
from settings import CONFIG
from matplotlib import rc

font = {'family': 'Verdana',
        'weight': 'normal'}
rc('font', **font)

__author__ = 'iurii.chiryshev'

cross_val_data = dict()
for score in CONFIG['scorings']:
    cross_val_data[score] = {'mean': [] , 'std': []}

roc_data = dict()
pr_data = dict()
det_data = dict()

for dsk_name in CONFIG['desk_names']:
    roc_data[dsk_name] = dict()
    pr_data[dsk_name] = dict()
    det_data[dsk_name] = dict()

for dsk_name in CONFIG['desk_names']:
    csv_data = os.path.join(CONFIG['path'],dsk_name + '-35' + '.csv')
    # читаем из файла
    dataset = np.loadtxt(csv_data,delimiter=",")
    # получаем размерность
    rows, cols = dataset.shape
    print("ROWS:", rows, "COLS:", cols)
    # в первом столбце лежат ответы (y) все остальное - это вектора признаков
    # вытаскиваем их
    X = dataset[:,1:cols]
    y = dataset[:,0].astype(int)
    # разделяем данные на тестовую и тренировочную выбоку
    X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.33, random_state=CONFIG['random_state'])
    # для кросс валидации так же понадобятся данные разделяем на n_folds частей
    kf = KFold(rows,n_folds=3, shuffle=True, random_state=CONFIG['random_state'])
    # кроссвалидация по разным метрикам
    model_rfc = RandomForestClassifier(n_estimators = 64)
    for score in CONFIG['scorings']: # цикл по метрикам
        values = cross_validation.cross_val_score(model_rfc, X, y, cv=kf, scoring=score)
        mean = values.mean()
        std = values.std()
        cross_val_data[score]['mean'].append(mean)
        cross_val_data[score]['std'].append(std)
        print('name = %s, scoring = \'%s\', mean value = %f, std value = %f' % (dsk_name,score,mean,std))
    #обучаем сам классификатор
    proba = model_rfc.fit(X_train, y_train).predict_proba(X_test)
    # roc кривая
    fpr, tpr, thresholds = roc_curve(y_test, proba[:, 1])
    roc_auc  = auc(fpr, tpr)
    roc_data[dsk_name]['fpr'] = fpr
    roc_data[dsk_name]['tpr'] = tpr
    roc_data[dsk_name]['roc_auc'] = roc_auc
    # pr кривая
    precision , recall , thresholds = precision_recall_curve(y_test, proba[:, 1])
    pr_data[dsk_name]['precision'] = precision
    pr_data[dsk_name]['recall'] = recall
    #det curve
    fps, fns, thresholds = detection_error_tradeoff(y_test, proba[:, 1])
    det_data[dsk_name]['fps'] = fps
    det_data[dsk_name]['fns'] = fns
    print(' ')

marker = itertools.cycle(('1','2','3','4','+','.',','))

plt.title(CONFIG['csv_name'])
# рисуем roc кривые
plt.subplot(2, 2, 1)
for n in CONFIG['desk_names']: # цикл по дескрипторам
    data = roc_data[n]
    plt.plot(data['fpr'], data['tpr'],marker = next(marker), label='%s (%0.4f)' % (n,data['roc_auc']))
plt.plot([0, 1], [0, 1], 'k--')
plt.xlim([0.0, 1.0])
plt.ylim([0.0, 1.0])
plt.xlabel(u'Доля ложно положительных примеров') # (FPR)
plt.ylabel(u'Доля истинно положительных примеров') #  (TPR)
plt.legend(loc='best')
plt.grid(True)

# рисуем det кривые
plt.subplot(2, 2, 2)
for n in CONFIG['desk_names']: # цикл по дескрипторам
    data = det_data[n]
    plt.plot(data['fps'], data['fns'],marker = next(marker), label='%s' % (n))
plt.yscale('log')
plt.xscale('log')
plt.legend(loc='best')
plt.xlabel(u'Доля ошибок первого рода') #(FPR)
plt.ylabel(u'Доля пропущенных изображений') #(FNR)
plt.grid(b=True, which='major', linestyle='-')
plt.grid(b=True, which='minor', linestyle=':')


# рисуем pr кривые
plt.subplot(2, 2, 3)
for n in CONFIG['desk_names']: # цикл по дескрипторам
    data = pr_data[n]
    plt.plot(data['recall'], data['precision'],marker = next(marker), label='%s trees' % (n))
plt.ylim([0.0, 1.01])
plt.xlim([0.0, 1.0])
plt.xlabel('Recall')
plt.ylabel('Precision')
plt.legend(loc='best')
plt.grid(True)


# рисуем столбцовую диаграмму по количеству деревьев
plt.subplot(2, 2, 4)
group_index = np.arange(len(CONFIG['desk_names']))
group_names = [i for i in CONFIG['desk_names']]
score_len = len(CONFIG['scorings'])
bar_width = 0.8/score_len
for i in range(score_len):
    score = CONFIG['scorings'][i]
    color = CONFIG['colors'][i]
    plt.bar(group_index + i*bar_width,
                    cross_val_data[score]['mean'],
                    bar_width,
                    alpha=0.4,
                    color=color,
                    yerr=cross_val_data[score]['std'],
                    error_kw={'ecolor': '0.3'},
                    label=CONFIG['trans_scores'][score])
plt.xlabel('')
plt.ylabel('')
plt.xticks(group_index + bar_width*score_len/2, group_names, rotation = 'vertical')
plt.legend(loc='best')
plt.tight_layout()
plt.grid(True)
plt.show()
