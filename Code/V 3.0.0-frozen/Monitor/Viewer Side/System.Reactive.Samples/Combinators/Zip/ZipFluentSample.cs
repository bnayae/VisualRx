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
    public class ZipFluentSample : SampleBase<string>
    {
        public override string Title => "Zip (fluent)";

        public override double Order => (double)SampleOrder.ZipFluent;

        public override string Query
        {
            get
            {
                var query = @"var xs = Observable.Interval(TimeSpan.FromSeconds(1))
    .Take(6);
var ys = Observable.Timer(TimeSpan.FromSeconds(2.5),
    TimeSpan.FromSeconds(1)).Take(6);
var zs = xs.Zip(ys, (item1, item2) => $""{item1} {item2}"");";
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
            var zs = xs.Zip(ys, (item1, item2) => $"{item1}, {item2}");
            zs = zs.Monitor("Zip", Order + 0.2);
            return zs;
        }
    }
}
