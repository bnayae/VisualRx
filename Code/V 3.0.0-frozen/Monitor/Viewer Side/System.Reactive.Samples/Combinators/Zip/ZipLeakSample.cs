using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class ZipLeakSample : SampleBase<string>
    {
        public override string Title => "Zip (leak)";

        public override double Order => (double)SampleOrder.ZipLeak;

        public override string Query
        {
            get
            {
                var query = @"var xs = Observable.Interval(TimeSpan.FromSeconds(1))
    .Take(10);
var ys = Observable.Interval(TimeSpan.FromSeconds(0.5))
    .Take(20);
var zs = Observable.Zip(xs, ys, (item1, item2) => $""{item1}, {item2}"");";
                return query;
            }
        }

        protected override IObservable<string> OnQuery()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(1))
                .Take(10);
            xs = xs.Monitor("Interval", Order);
            var ys = Observable.Interval(TimeSpan.FromSeconds(0.5))
                .Take(20);
            ys = ys.Monitor("Timer", Order + 0.1);
            var zs = Observable.Zip(xs, ys, (item1, item2) => $"{item1}, {item2}");
            zs = zs.Monitor("Zip", Order + 0.2);
            return zs;
        }
    }
}
