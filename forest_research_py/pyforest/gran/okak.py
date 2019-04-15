__author__ = 'iurii.chiryshev'
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.colors as clrs
from mpl_toolkits.mplot3d import Axes3D
import matplotlib.image as mp_img
from sklearn.cluster import DBSCAN
from sklearn import metrics
from sklearn.datasets.samples_generator import make_blobs
from sklearn.preprocessing import StandardScaler
from sklearn.preprocessing import MinMaxScaler

# generate some sample data
import scipy.misc
import scipy.ndimage
lena = mp_img.imread("D:/2017/projects/foto/gran/Pl_18_16_52_718.bmp")

lena = scipy.ndimage.filters.gaussian_filter(lena,sigma= 5)

# downscaling has a "smoothing" effect
lena = scipy.misc.imresize(lena, 0.25, interp='cubic')



#lena = np.invert(lena)



row = lena.shape[0]
col = lena.shape[1]
# create the x and y coordinate arrays (here we just use pixel indices)


X = np.array(list(np.ndindex((row, col))))
#X = StandardScaler(with_std=False).fit_transform(X)
sample_weight = lena.reshape(row * col, 1)
sample_weight = MinMaxScaler().fit_transform(sample_weight) # scale [0,1]

db = DBSCAN(eps=1.5, min_samples=4).fit(X,sample_weight=sample_weight)
core_samples_mask = np.zeros_like(db.labels_, dtype=bool)
core_samples_mask[db.core_sample_indices_] = True
labels = db.labels_
n_clusters_ = len(set(labels)) - (1 if -1 in labels else 0)

print('Estimated number of clusters: %d' % n_clusters_)

unique_labels = set(labels)
colors = [plt.cm.prism(each) for each in np.linspace(0, 1, len(unique_labels))]
#facecolors = np.array([ clrs.rgb2hex(plt.cm.prism(each)) if each != 0. else "#000000" for each in MinMaxScaler().fit_transform(labels)],dtype=str).reshape(row,col)
# create the figure
xx, yy = np.mgrid[0:row, 0:col]


############################# 2d #####################################
for k, col in zip(unique_labels, colors):
    if k == -1:
        # Black used for noise.
        col = [0, 0, 0, 1]

    class_member_mask = (labels == k)

    xy = X[class_member_mask & core_samples_mask]
    plt.plot(xy[:, 0], xy[:, 1], 'o', markerfacecolor=tuple(col),
             markeredgecolor='k', markersize=10)

    xy = X[class_member_mask & ~core_samples_mask]
    plt.plot(xy[:, 0], xy[:, 1], 'o', markerfacecolor=tuple(col),
             markeredgecolor='k', markersize=4)

#plt.title('Estimated number of clusters: %d' % n_clusters_)
# plt.show()


############################# 3d #####################################
# fig = plt.figure()
# ax = fig.gca(projection='3d')
# ax.plot_surface(xx,
#                 yy,
#                 lena ,
#                 rstride=1,
#                 cstride=1,
#                 #facecolors=facecolors,
#                 cmap=plt.get_cmap('hot'),
#                 linewidth=0)
#
#
# plt.figure()
# CS = plt.contour(xx, yy, lena)

# show it
plt.show()
