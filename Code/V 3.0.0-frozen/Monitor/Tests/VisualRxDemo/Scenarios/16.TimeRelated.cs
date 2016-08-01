using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class TimeRelatedScenario : IScenario
    {
        private Action _act = () =>
        {
            IObservable<int> xs = Observable.Generate(0, i => i < 7, i => i + 1, i => i,
                    i => TimeSpan.FromSeconds(i * 0.5));

            xs = xs.Monitor("Source", 1);
            xs = xs.Publish().RefCount();
            xs = xs.Monitor("Source", 1);
            xs = xs.SubscribeOn(TaskPoolScheduler.Default);

            IObservable<Timestamped<int>> timestamps = xs.Timestamp();
            timestamps = timestamps.Monitor("Timestamp", 2,
                (stamp, m) => string.Format("{0} at {1:HH:mm:ss}", stamp.Value, stamp.Timestamp));

            IObservable<TimeInterval<int>> timeinterval = xs.TimeInterval();
            timeinterval = timeinterval.Monitor("TimeInterval", 3,
                (interval, m) => string.Format("{0} gap of {1:ss\\.fff} seconds", interval.Value, interval.Interval));

            IObservable<int> timeout = xs.Timeout(TimeSpan.FromSeconds(1.3));
            timeout = timeout.Monitor("Timeout", 4);

            timeinterval.Subscribe();
            timestamps.Subscribe();
            timeout.Subscribe(v => Trace.WriteLine(v), ex => Trace.WriteLine(ex), () => Trace.WriteLine("Complete"));
        };

        public string Title
        {
            get { return "Time Related"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
IObservable<int> xs = ...;

IObservable<Timestamped<int>> timestamps = xs.Timestamp();

IObservable<TimeInterval<int>> timeinterval = xs.TimeInterval();

IObservable<int> timeout = xs.Timeout(TimeSpan.FromSeconds(2));
";
            }
        }

        public double Order
        {
            get { return 16; }
        }

        public ICommand Invoke
        {
            get
            {
                return new InvokeCommand(_act);
            }
        }
    }
}
