from Verilog_VCD import *
from optparse import OptionParser
import os

import plotly
from plotly.graph_objs import Scatter, Layout, Figure
from plotly import tools
import numpy as np

x_times = []
x_values = []

class step_path(object):
    '''
    Encapsulates a path defined by a number of unit-spaced steps occurring at specific times.
    '''
    def __init__(self, times, values):
        self.time_value = zip(times, values)
        
    def append(self, time, value):
        self.time_value.append((time, value))
    
    def steps_exact(self, step = True):
        '''
        Returns a line which follows the steps taken as accurately as possible.
        Set "step" to true to include two points per value change:
          - one at T
        '''
        last_v = None
        for (t, v) in self.time_value:
            if last_v is not None and step:
                yield (t, last_v)
            yield (t, v)
            last_v = v;
            
    def sample(self, rate):
        (last_t, last_v) = self.time_value[0]
        (this_t, this_v) = self.time_value[1]
        time = last_t # First time
        index = 2
        while (time < self.time_value[-1][0]):
            while time > this_t and index < len(self.time_value):
                (last_t, last_v) = (this_t, this_v)
                (this_t, this_v) = self.time_value[index]
                index = index + 1
            
            factor = (time - last_t) / (this_t - last_t)
            v = (1.0 - factor) * last_v + factor * this_v
            yield (time, v)
            time = time + rate
    
    def times(self):
        return [time for (time, value) in self.time_value]
    
    def values(self):
        return [value for (time, value) in self.time_value]

class step_path_vcd(step_path):
    '''
    Encapsulates a path defined by a number of unit-spaced steps occurring at specific times.
    '''
    def __init__(self, step_signal, dir_signal):
        self.time_value = [(0, 0)] # Assume the signal begins at time 0.  The initial value is always 0.
        x = 0
        dir_index = 0
        for (time, value) in step_signal:
            if value == '1':
                # Rising Edge, what's the "dir" signal say?
                while dir_index + 1 < len(dir_signal) and dir_signal[dir_index + 1][0] < time:
                    dir_index = dir_index + 1
                dir = dir_signal[dir_index][1]
                if dir == '1':
                    x = x + 1
                else:
                    x = x - 1
                self.time_value.append((time, x))
    
    def cleanup(self):
        '''
        A step signal might remain inactive for a long period of time,
        (I.E, the axis has stopped).  The value will remain high or low
        for this period of time though, and add some interesting artifacts
        to the graphs.  Attempt to clean up sections with no step activity
        for a long period of time.
        '''
        
        i = 0
        while (i < len(self.time_value)-1):
            tn1 = self.time_value[i-1][0] if i > 0 else None
            t0 = self.time_value[i][0]
            t1 = self.time_value[i+1][0]
            t2 = self.time_value[i+2][0] if i < len(self.time_value) - 2 else None
            if (t1 - t0 > 0.1):
                v = self.time_value[i][1]
                d0 = t0 - tn1 if tn1 is not None else None
                d1 = t2 - t1 if t2 is not None else None
                
                # They won't both be none
                d1 = d0 if d1 is None else d1
                d0 = d1 if d0 is None else d0
                
                if ((d0 + d1) * 4 < (t1 - t0)):
                    self.time_value.insert(i+1, (t1 - d0 * 4, v))
                    self.time_value.insert(i+1, (d0 * 4 + t0, v))
                    i = i + 2
            i = i + 1
                    
    
def gen_axis_graphs(vcd_dict, step_signal_name, dir_signal_name, axis_name, hue):
    x_steps = None
    x_dir = None
    for key in vcd_dict.keys():
        name = vcd_dict[key]['nets'][0]['name']
        print("Key {0} name is {1}".format(key, name))
        if step_signal_name in vcd_dict[key]['nets'][0]['name'].lower():
            x_steps = vcd_dict[key]['tv']
        if dir_signal_name in vcd_dict[key]['nets'][0]['name'].lower():
            x_dir = vcd_dict[key]['tv']
    
    if x_steps is None or x_dir is None:
        return [None, None, None, None]
    
    x_path = step_path_vcd(x_steps, x_dir)
    
    # Ignore any axis with no data
    if len(x_path.times()) < 5:
        return [None, None, None, None]
    
    x_path.cleanup()
    
    
    # Sample velocity at a consistent interval of 25uS
    sample_period = 0.000025
    
    
    # Low pass filter code from here: https://tomroelandts.com/articles/how-to-create-a-simple-low-pass-filter
    fc = sample_period * 75 # Cutoff frequency as a fraction of the sampling rate (in (0, 0.5))
    b = sample_period * 150  # Transition band, as a fraction of the sampling rate (in (0, 0.5)).
    N = int(np.ceil((4 / b)))
    if not N % 2: N += 1  # Make sure that N is odd.
    n = np.arange(N)
     
    # Compute sinc filter.
    h = np.sinc(2 * fc * (n - (N - 1) / 2.))
     
    # Compute Blackman window.
    w = 0.42 - 0.5 * np.cos(2 * np.pi * n / (N - 1)) + \
        0.08 * np.cos(4 * np.pi * n / (N - 1))
     
    # Multiply sinc filter with window.
    h = h * w
     
    # Normalize to get unity gain.
    h = h / np.sum(h)
    
    print("Shift: {0}".format((N - 1) / 2))
    vraw = step_path([], [])
    (lt, lv) = (None, None)
    for (t, v) in x_path.steps_exact(False):
        if lt is not None and lv is not None:
            vraw.append(t, (v - lv) / (t - lt))
        (lt, lv) = (t, v)
    
    
    
    v_sampled_t = []
    v_sampled_v = []
    (lt, lv) = (None, None)
    for (t, v) in vraw.sample(sample_period):
        v_sampled_t.append(t)
        v_sampled_v.append(v)
    
    # Pass the sampled velocity values through a low-pass filter
    vel_filtered_v = np.convolve(v_sampled_v, h)[(N - 1) / 2:]
    vel_filtered_t = v_sampled_t[:len(v_sampled_t) - (N - 1) / 2]
    
    acc = []
    for i in range(1, len(vel_filtered_v)):
        acc.append((vel_filtered_v[i] - vel_filtered_v[i-1]) / sample_period)
    acc.insert(0, acc[0])
    
    return [
        Scatter(yaxis='y1', x=x_path.times(), y=x_path.values(), mode = 'lines', name="{0} position".format(axis_name),
            line=dict(shape='hv', color='hsv({0}, 0.7, 1.0)'.format(hue % 360))),
            
        Scatter(yaxis='y1', x=vraw.times(), y=vraw.values(), mode = 'lines', name="{0} velocity".format(axis_name),
            line=dict(color='hsv({0}, 0.7, 1.0)'.format((hue) % 360, width=1))),
            
        Scatter(yaxis='y1', x=vel_filtered_t, y=vel_filtered_v, mode = 'lines', name="{0} v. filtered".format(axis_name),
            line=dict(color='hsv({0}, 0.7, 1.0)'.format((hue) % 360))),
            
        Scatter(yaxis='y1', x=vel_filtered_t, y=acc, mode = 'lines', name="{0} acceleration".format(axis_name),
            line=dict(color='hsv({0}, 0.7, 1.0)'.format((hue) % 360))),
        ]


if __name__ == "__main__":
    parser = OptionParser()
    parser.add_option("-v", "--vcd", dest="vcd_filename",
                  help="Name of the VCD file to load", metavar="FILE")
    (options, args) = parser.parse_args()
    
    vcd_dict = Verilog_VCD.parse_vcd(options.vcd_filename, opt_timescale="s")
    print vcd_dict.keys()
    
    
    
    
    x_axis_data = gen_axis_graphs(vcd_dict, "x_step", "x_dir", "X", 15)
    y_axis_data = gen_axis_graphs(vcd_dict, "y_step", "y_dir", "Y", 150)
    z_axis_data = gen_axis_graphs(vcd_dict, "z_step", "z_dir", "Z", 255)
    
    axis_data = [x_axis_data, y_axis_data, z_axis_data]
    
    fig = tools.make_subplots(rows=5, cols=1, specs=[[{'rowspan': 2}], # Row 1
                                                     [None],         # Row 2
                                                     [{'rowspan': 2}], # Row 3
                                                     [None],         # Row 4
                                                     [{}]],            # Row 5
        shared_xaxes=True, shared_yaxes=True, vertical_spacing=0.001)
    
    filename = os.path.splitext(os.path.split(options.vcd_filename)[-1])[0];
    fig['layout'].update(title="Motion Profile from Steps ({0})".format(filename))
    fig['layout']['yaxis1'].update(title="Position (steps)")
    fig['layout']['yaxis3'].update(title="Velocity (steps/s)")
    fig['layout']['yaxis5'].update(title="Acceleration (steps/s^2)")
    
    for trace in [p for (p, v, vf, a) in axis_data]:
        if trace is not None:
            fig.append_trace(trace, 1, 1)
    
    for trace in [t for (p, v, vf, a) in axis_data for t in [v, vf]]:
        if trace is not None:
            fig.append_trace(trace, 3, 1)
    
    for trace in [a for (p, v, vf, a) in axis_data]:
        if trace is not None:
            fig.append_trace(trace, 5, 1)
    
    #fig.append_trace(x_axis_data[1], 2, 1)
    #fig.append_trace(y_axis_data[1], 2, 1)
    #fig.append_trace(z_axis_data[1], 2, 1)
    #
    #fig.append_trace(x_axis_data[2], 2, 1)
    #fig.append_trace(y_axis_data[2], 2, 1)
    #fig.append_trace(z_axis_data[2], 2, 1)
    #
    #
    #fig.append_trace(x_axis_data[3], 3, 1)
    #fig.append_trace(y_axis_data[3], 3, 1)
    #fig.append_trace(z_axis_data[3], 3, 1)
    
    
    #fig = Figure(
    #    data=axis_data,
    #            #Scatter(yaxis='y1', x=bt, y=bv, mode = 'lines', name="vel_sampled2"),
    #            #Scatter(yaxis='y1', x=time, y=vel_filtered, mode = 'lines', name="vel_filtered2"),
    #            #Scatter(yaxis='y1', x=time, y=av, mode = 'lines', name="acc_sampled2"),
    #            
    #            #Scatter(yaxis='y1', x=x_times, y=x_values, mode = 'lines', name="raw position"),
    #            #Scatter(yaxis='y1', x=x_times, y=vel_raw, mode = 'lines', marker=dict(maxdisplayed=1000), name="raw vel"),
    #
    #        #],
    #    layout=Layout(title="VCD Decoded Steps",
    #    yaxis=dict(
    #        title='Position (steps)  --  Velocity (steps/sec)  --  Acceleration (steps/sec^2)'
    #    ),
    #))
    plotly.offline.plot(fig, filename="{0}.html".format(filename))