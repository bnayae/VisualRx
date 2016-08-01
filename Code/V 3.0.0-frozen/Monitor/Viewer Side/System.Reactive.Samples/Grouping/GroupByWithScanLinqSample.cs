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
    public class GroupByWithScanLinqSample : SampleBase<string>
    {
        public override string Title => "GroupBy With Scan Linq ";

        public override double Order => (double)SampleOrder.GroupByScanLinq;

        public override string Query
        {
            get
            {
                var query = @"var source = Observable.Interval(TimeSpan.FromSeconds(0.5))
var xs = from i in source
            group i by i % 3 into g
            from acc in g.Scan($"""", (p,c) => $""{ p}, { c}"")
                     select acc; ";
                return query;
            }
        }

        protected override IObservable<string> OnQuery()
        {
            var source = Observable.Interval(TimeSpan.FromSeconds(0.5))
                                   .Monitor("Source", Order).Take(10);
            var xs = from i in source
                     group i by i % 3 into g
                     from acc in g.Select(m => m.ToString()) // avoiding seed value
                                  .Scan((p,c) => $"{p}, {c}")
                                  .Monitor($"Group {g.Key}", Order + (g.Key / 10) + 0.1)
                     select acc;
            return xs.Monitor($"All Group", Order + 0.9);
        }
    }
}
