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
    public class MergeFluentSample : SampleBase<long>
    {
        public override string Title => "Merge (fluent)";

        public override double Order => (double)SampleOrder.MergeFluent;

        public override string Query
        {
            get
            {
                var query = @"var xs = Observable.Interval(TimeSpan.FromSeconds(2))
    .Select(v => -v).Take(5);
var ys = Observable.Timer(TimeSpan.FromSeconds(1.5),
    TimeSpan.FromSeconds(2)).Take(6);
var zs = xs.Merge(ys);";
                return query;
            }
        }

        protected override IObservable<long> OnQuery()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(2))
                .Select(v => -v).Take(5);
            xs = xs.Monitor("Interval 2 second", Order + 0.1);
            var ys = Observable.Timer(TimeSpan.FromSeconds(1.5),
                TimeSpan.FromSeconds(2)).Take(6);
            ys = ys.Monitor("Timer 1.5 second follow by 2 second", Order + 0.2);
            var zs = xs.Merge(ys);
            zs = zs.Monitor("Merge (fluent)", Order + 0.3);
            return zs;
        }
    }
}
