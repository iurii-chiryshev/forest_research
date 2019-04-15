from sklearn.metrics.ranking import _binary_clf_curve

__author__ = 'iurii.chiryshev'
def detection_error_tradeoff(y_true, probas_pred, pos_label=None,
                             sample_weight=None):
    """Compute error rates for different probability thresholds

    Note: this implementation is restricted to the binary classification task.

    Parameters
    ----------
    y_true : array, shape = [n_samples]
        True targets of binary classification in range {-1, 1} or {0, 1}.

    probas_pred : array, shape = [n_samples]
        Estimated probabilities or decision function.

    pos_label : int, optional (default=None)
        The label of the positive class

    sample_weight : array-like of shape = [n_samples], optional
        Sample weights.

     Returns
     -------
     fps : array, shape = [n_thresholds]
         A count of false positives, at index i being the number of negative
         samples assigned a score >= thresholds[i]. The total number of
         negative samples is equal to fps[-1] (thus true negatives are given by
         fps[-1] - fps).

     fns : array, shape = [n_thresholds]
         A count of false negatives, at index i being the number of positive
         samples assigned a score < thresholds[i]. The total number of
         positive samples is equal to tps[-1] (thus false negatives are given by
         tps[-1] - tps).

     thresholds : array, shape = [n_thresholds]
         Decreasing score values.

     References
     ----------
     .. [1] `Wikipedia entry for Detection error tradeoff
             <https://en.wikipedia.org/wiki/Detection_error_tradeoff>`_
     .. [2] `The DET Curve in Assessment of Detection Task Performance
             <http://www.itl.nist.gov/iad/mig/publications/storage_paper/det.pdf>`_
     .. [3] `2008 NIST Speaker Recognition Evaluation Results
             <http://www.itl.nist.gov/iad/mig/tests/sre/2008/official_results/>`_
     .. [4] `DET-Curve Plotting software for use with MATLAB
             <http://www.itl.nist.gov/iad/mig/tools/DETware_v2.1.targz.htm>`_

     Examples
     --------
     import numpy as np
     from sklearn.metrics import detection_error_tradeoff
     y_true = np.array([0, 0, 1, 1])
     y_scores = np.array([0.1, 0.4, 0.35, 0.8])
     fps, fns, thresholds = detection_error_tradeoff(y_true, y_scores)
     fps
     array([ 0.5,  0.5,  0. ])
     fns
     array([ 0. ,  0.5,  0.5])
     thresholds
     array([ 0.35,  0.4 ,  0.8 ])

     """
    fps, tps, thresholds = _binary_clf_curve(y_true, probas_pred,
                                             pos_label=pos_label,
                                             sample_weight=sample_weight)
    fns = tps[-1] - tps
    tp_count = tps[-1]
    tn_count = (fps[-1] - fps)[0]

    # start with false positives is zero and stop with false negatives zero
    # and reverse the outputs so list of false positives is decreasing
    last_ind = tps.searchsorted(tps[-1]) + 1
    first_ind = fps[::-1].searchsorted(fps[0])
    sl = range(first_ind, last_ind)[::-1]
    return fps[sl] / tp_count, fns[sl] / tn_count, thresholds[sl]


