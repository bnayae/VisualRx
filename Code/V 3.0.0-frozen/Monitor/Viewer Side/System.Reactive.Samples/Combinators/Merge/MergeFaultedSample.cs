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
    public class MergeFaultedSample : SampleBase<long>
    {
        public override string Title => "Merge (faulted)";

        public override double Order => (double)SampleOrder.MergeFaulted;

        public override string Query
        {
            get
            {
                var query = @"var xs = Observable.Interval(TimeSpan.FromSeconds(1))
    .Take(5);
var ys = Observable.Never<long>()
                   .Timeout(TimeSpan.FromSeconds(2));
var zs = xs.Merge(ys);";
                return query;
            }
        }

        protected override IObservable<long> OnQuery()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(1))
                .Take(5);
            xs = xs.Monitor("Interval 1 second", Order + 0.1);
            var ys = Observable.Never<long>()
                               .Timeout(TimeSpan.FromSeconds(2));
            ys = ys.Monitor("Fault after 2 second", Order + 0.2);
            var zs = xs.Merge(ys);
            zs = zs.Monitor("Merge (Fault)", Order + 0.3);
            return zs;
        }
    }
}



