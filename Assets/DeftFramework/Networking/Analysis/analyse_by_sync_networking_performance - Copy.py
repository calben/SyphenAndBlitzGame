import numpy as np 
import seaborn as sb 
import matplotlib.pyplot as plt 
import pandas as pd 
import datetime as dt
import sys

start = dt.datetime(year=2000,month=1,day=1)

datetimes = lambda x: dt.timedelta(seconds=x) + start

df = pd.read_csv(sys.argv[1] + ".csv")
df["time"] = df["time"].apply(datetimes)
df = df.set_index("time").resample("ms")
df = df.interpolate("linear")
pd.rolling_mean(df, window=2).plot(kind="line",y="positional_difference_square_magnitude",title=sys.argv[2])
plt.savefig("grenade.pdf")
