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
lena = mp_img.imread("D:/2017/projects/foto/gran/Pl_18_16_52_718.bmp")

# downscaling has a "smoothing" effect
lena = scipy.misc.imresize(lena, 0.25, interp='cubic')

row = lena.shape[0]
col = lena.shape[1]
# create the x and y coordinate arrays (here we just use pixel indices)


X = np.array(list(np.ndindex((row, col))))
#X = StandardScaler(with_std=False).fit_transform(X)
sample_weight = lena.reshape(row * col, 1)
sample_weight = MinMaxScaler().fit_transform(sample_weight) # scale [0,1]

eps_step = 0.25
ms_step = 1
epss = np.arange(1,4,eps_step)
mss = np.arange(1,17,ms_step)
shape = [epss.shape[0],mss.shape[0]]
hist_nclass = np.empty(shape, dtype=int)
hist_empty = np.empty(shape, dtype=int)

for ep_id in np.arange(shape[0]):
    for ms_id in np.arange(shape[1]):
        db = DBSCAN(eps=epss[ep_id], min_samples=mss[ms_id]).fit(X,sample_weight=sample_weight)
        core_samples_mask = np.zeros_like(db.labels_, dtype=bool)
        core_samples_mask[db.core_sample_indices_] = True
        labels = db.labels_
        n_clusters_ = len(set(labels)) - (1 if -1 in labels else 0)
        n_empty = 0
        for l in labels:
            if l == -1:
                n_empty += 1
        hist_nclass[ep_id,ms_id] = n_clusters_
        hist_empty[ep_id,ms_id] = n_empty

#hist = hist_nclass / (hist_empty + 1)

hist = np.empty(shape, dtype=float)
for ep_id in np.arange(shape[0]):
    for ms_id in np.arange(shape[1]):
        if hist_empty[ep_id,ms_id] == 0 or hist_nclass[ep_id,ms_id] == 1:
            hist[ep_id,ms_id] = 0
        else:
            hist[ep_id,ms_id] = hist_nclass[ep_id,ms_id] / hist_empty[ep_id,ms_id]


xpos, ypos = np.meshgrid(epss - (eps_step / 4.), mss - (ms_step / 4.))
xpos = xpos.flatten('F')
ypos = ypos.flatten('F')
zpos = np.zeros_like(xpos)

# Construct arrays with the dimensions for the 16 bars.
dx = (eps_step / 2.) * np.ones_like(zpos)
dy = (ms_step / 2.) * np.ones_like(zpos)
dz = hist_nclass.flatten()

fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')
ax.bar3d(xpos, ypos, zpos, dx, dy, dz, color='b', zsort='average')

dz = hist.flatten()
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')
ax.bar3d(xpos, ypos, zpos, dx, dy, dz, color='b', zsort='average')

plt.show()