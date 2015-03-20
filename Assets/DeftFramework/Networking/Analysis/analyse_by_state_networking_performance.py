import numpy as np 
import seaborn as sb 
import matplotlib.pyplot as plt 
import pandas as pd 
import datetime as dt
import sys

sb.set_style("dark")
start = dt.datetime(year=2000,month=1,day=1)

datetimes = lambda x: dt.timedelta(seconds=x) + start

a = pd.read_csv(sys.argv[1])
b = pd.read_csv(sys.argv[2])
a["time"] = a["time"].apply(datetimes)
a = a.set_index("time").resample("100ms")
a = a.interpolate("cubic")
b["time"] = b["time"].apply(datetimes)
b = b.set_index("time").resample("100ms")
b = b.interpolate("cubic")
diff = a - b
diff = diff.abs()
diff = diff.mean(axis=1)
diff.plot(title=sys.argv[3])
plt.savefig("state-difference.pdf")
