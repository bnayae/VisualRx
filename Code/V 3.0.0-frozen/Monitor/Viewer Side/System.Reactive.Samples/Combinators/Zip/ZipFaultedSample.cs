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
    public class ZipFaultedSample : SampleBase<string>
    {
        public override string Title => "Zip (faulted)";

        public override double Order => (double)SampleOrder.ZipFaulted;

        public override string Query
        {
            get
            {
                var query = @"var xs = Observable.Interval(TimeSpan.FromSeconds(1))
    .Take(6);
var ys = Observable.Never<long>()
                    .Timeout(TimeSpan.FromSeconds(2));
var zs = Observable.Zip(xs, ys, (item1, item2) => $""{item1}, {item2}"");";
                return query;
            }
        }

        protected override IObservable<string> OnQuery()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(1))
                .Take(6);
            xs = xs.Monitor("Interval", Order);
            var ys = Observable.Never<long>()
                               .Timeout(TimeSpan.FromSeconds(2));
            ys = ys.Monitor("Fault after 2 second", Order + 0.2);
            var zs = Observable.Zip(xs, ys, (item1, item2) => $"{item1}, {item2}");
            zs = zs.Monitor("Zip", Order + 0.2);
            return zs;
        }
    }
}
