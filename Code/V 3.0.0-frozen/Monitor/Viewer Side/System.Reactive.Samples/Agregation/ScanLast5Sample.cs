using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class ScanLast5Sample : SampleBase<int>
    {
        public override string Title => "Scan Last 5";

        public override double Order => (double)SampleOrder.ScanLast5;

        public override string Query
        {
            get
            {
var query = @"IObservable<int> source = ...;
var scan = source.Scan(ImmutableQueue<long>.Empty,
    (acc, cur) =>
    {
        var result = acc.Enqueue(cur);
        if (result.Count() > 5)
            result = result.Dequeue();
        return result;
    });
var gaps = scan.Select(m => m.Sum());
var filter = gaps.Where(m => m < 30);";
                return query;
            }
        }

        protected override IObservable<int> OnQuery()
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var source = Observable.Interval(TimeSpan.FromMilliseconds(100))
                .Select(_ => rnd.Next(0, 100))
                .Take(55);
            #region Monitor

            source = source.Monitor("source", Order + 0.1);

            #endregion // Monitor
            var scan = source.Scan(ImmutableQueue<int>.Empty,
                (acc, cur) =>
                {
                    var result = acc.Enqueue(cur);
                    if (result.Count() > 5)
                        result = result.Dequeue();
                    return result;
                });
            #region Monitor

            scan = scan.Monitor("Scan", Order + 0.2, (v, m) => string.Join(",", v));

            #endregion // Monitor
            var secondFilter = scan.Where(m => m.Count() == 5);
            #region Monitor

            secondFilter = secondFilter.Monitor("Second Filter", Order + 0.3, (v, m) => string.Join(",", v));

            #endregion // Monitor
            var gaps = secondFilter.Select(m => m.Max() - m.Min());
            #region Monitor

            gaps = gaps.Monitor("Gaps", Order + 0.4);

            #endregion // Monitor
            var filter = gaps.Where(m => m > 80);
            #region Monitor

            filter = filter.Monitor("Filter", Order + 0.5);

            #endregion // Monitor
            return filter;
        }
    }
}
