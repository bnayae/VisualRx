using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public enum Weather { Sunny, Rainy, Cloudy, Snowy, Stormy, Windy };
    public enum Mood { Loving, Bored, Happy, Sad, Excited, Angry };
}
