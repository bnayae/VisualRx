using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class TimerVsIntervalSample : SampleBase<long>
    {
        public override string Title => "Timer vs. Interval";

        public override double Order => (double)SampleOrder.TimerVsInterval;

        public override string Query
        {
            get
            {
                var query = @"Observable.Interval(TimeSpan.FromSeconds(1)).Take(5);
Observable.Timer(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(0.5)).Take(10);
Observable.Timer(TimeSpan.FromSeconds(3))";
                return query;
            }
        }

        protected override IObservable<long> OnQuery()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(1)).Take(5);
            xs = xs.Monitor("Interval (1 second)", Order + 0.1);
            var ys = Observable.Timer(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(0.5)).Take(10);
            ys = ys.Monitor("Timer (3 second follow by 0.5 second)", Order + 0.2);
            var zs = Observable.Timer(TimeSpan.FromSeconds(3));
            zs = zs.Monitor("Timer (once after 3 second) ", Order + 0.3);
            return Observable.Merge(xs, ys, zs);
        }
    }
}
