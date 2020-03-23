#!/usr/bin/env python3

import sys
import numpy as np
from itertools import product
import importlib.util
from scipy.optimize import least_squares
from scipy.ndimage.interpolation import shift
from scipy.interpolate import UnivariateSpline, interp2d
# import matplotlib
# matplotlib.use('agg')
import matplotlib.pyplot as plt
import re
from glob import glob
import cv2


def natural_key(string_):
    return [int(s) if s.isdigit() else s for s in re.split(r'(\d+)', string_)]


def normalize(v):
    norm = np.linalg.norm(v)
    return v / norm if norm >= 1e-6 else v


def calc_center(prof):
    x = np.array(range(prof.shape[0]))
    xp = np.dot(x, prof)
    if prof.sum() < 1e-6:
        return prof.shape[0] / 2
    return xp / prof.sum()


def load_capture(srcdir):
    srcs = sorted(glob(srcdir + '/capture_*.png'), key=natural_key)
    srcs = list(filter(lambda x: 'black' not in x, srcs))
    print(srcs)
    black = cv2.imread(srcdir + '/capture_black.png', -1).astype(float)
    imgs = map(lambda src: cv2.imread(src, -1).astype(float) - black, srcs)
    return np.array(list(imgs))


def main():
    srcdir = '.'
    epi = load_capture(srcdir)
    print(epi.shape)
    np.save('epi_capture.npy', epi)

    h, w = epi.shape[1:3]
    delays = range(epi.shape[0])

    prof = epi[:, h // 2, w // 2]
    fig_prof = plt.figure()
    ch = ['b', 'g', 'r']
    for i in range(prof.shape[1]):
        plt.plot(delays, prof[:, i], label=ch[i], color=ch[i])
    plt.grid()
    plt.legend()

    fig_img = plt.figure()

    def onclick(event):
        if event.xdata is None or event.ydata is None:
            return
        x = max(0, min(w - 1, int(event.xdata)))
        y = max(0, min(h - 1, int(event.ydata)))
        # idx = x + y * w
        prof = epi[:, y, x]
        print(prof[:, 1])
        # prof_est = get_profiles(idx)
        for i in range(prof.shape[1]):
            fig_prof.gca().lines[i].set_data(delays, prof[:, i])

        fig_prof.gca().relim()
        fig_prof.gca().autoscale_view(True, True, True)
        fig_prof.canvas.draw()

    # sum_all = np.exp(epi).sum(axis=2)[:, :, 0]
    # direct = (np.exp(epi)[:, :, 0] / 255)
    # direct = np.clip(direct, 0, 1)
    # sum_all = (sum_all - sum_all.min()) / (sum_all.max() - sum_all.min())
    direct = np.copy(epi[epi.shape[0] // 2])
    direct /= direct.max()
    direct = np.clip(direct, 0, 1)
    plt.imshow(direct[:, :, ::-1])
    plt.colorbar()
    cid = fig_img.canvas.mpl_connect('button_press_event', onclick)
    plt.show()


if __name__ == '__main__':
    sys.exit(main())
