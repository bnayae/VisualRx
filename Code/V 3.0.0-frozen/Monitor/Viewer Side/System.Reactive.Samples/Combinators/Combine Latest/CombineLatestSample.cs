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
    public class CombineLatestSample : SampleBase<string>
    {
        public override string Title => "Combine Latest";

        public override double Order => (double)SampleOrder.CombineLatest;

        public override string Query
        {
            get
            {
                var query = @"var xs = Observable.Timer(TimeSpan.FromSeconds(2.5), 
                    TimeSpan.FromSeconds(2))
                         .Select(m => (char)('A' + m))
    .Take(10);
var ys = Observable.Interval(TimeSpan.FromSeconds(1))
    .Take(20);
var zs = Observable.CombineLatest(xs, ys, (item1, item2) => $""{item1}, {item2}"");";
                return query;
            }
        }

        protected override IObservable<string> OnQuery()
        {
            var xs = Observable.Timer(TimeSpan.FromSeconds(2.5), TimeSpan.FromSeconds(2))
                               .Select(m => (char)('A' + m))
                .Take(10);
            xs = xs.Monitor("Timer", Order);
            var ys = Observable.Interval(TimeSpan.FromSeconds(1))
                .Take(10);
            ys = ys.Monitor("Interval", Order + 0.1);
            var zs = Observable.CombineLatest(xs, ys, (item1, item2) => $"{item1}, {item2}");
            zs = zs.Monitor("CombineLatest", Order + 0.2);
            return zs;
        }
    }
}
