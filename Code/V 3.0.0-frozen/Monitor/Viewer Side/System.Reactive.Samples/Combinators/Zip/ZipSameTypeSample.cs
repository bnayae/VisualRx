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
    public class ZipSameTypeSample : SampleBase<string>
    {
        public override string Title => "Zip (same types)";

        public override double Order => (double)SampleOrder.ZipSameTypes;

        public override string Query
        {
            get
            {
                var query = @"var xs = Observable.Interval(TimeSpan.FromSeconds(1))
    .Take(6);
var ys = Observable.Timer(TimeSpan.FromSeconds(2.5),
    TimeSpan.FromSeconds(1)).Take(6);
var zs = Observable.Zip(xs, ys)
                    .Select(m => $""{m[0]} {m[1]}"");";
                return query;
            }
        }

        protected override IObservable<string> OnQuery()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(1))
                .Take(6);
            xs = xs.Monitor("Interval", Order);
            var ys = Observable.Timer(TimeSpan.FromSeconds(2.5),
                TimeSpan.FromSeconds(1)).Take(6);
            ys = ys.Monitor("Timer", Order + 0.1);
            var zs = Observable.Zip(xs, ys)
                                .Select(m => $"{m[0]} {m[1]}");
            zs = zs.Monitor("Zip", Order + 0.2);
            return zs;
        }
    }
}
