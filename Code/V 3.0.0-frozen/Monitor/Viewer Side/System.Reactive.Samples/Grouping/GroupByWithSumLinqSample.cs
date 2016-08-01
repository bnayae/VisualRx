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
    public class GroupByWithSumLinqSample : SampleBase<long>
    {
        public override string Title => "GroupBy With Sum Linq";

        public override double Order => (double)SampleOrder.GroupBySumLinq;

        public override string Query
        {
            get
            {
                var query = @"var source = Observable.Interval(TimeSpan.FromSeconds(0.5))
var xs = from i in source
            group i by i % 3 into g
            from sum in g.Sum()
            select new { g.Key, Sum = sum};";
                return query;
            }
        }

        protected override IObservable<long> OnQuery()
        {
            var source = Observable.Interval(TimeSpan.FromSeconds(0.5))
                                   .Monitor("Source", Order).Take(10);
            var xs = from i in source
                     group i by i % 3 into g
                     from sum in g.Sum()
                     select new { g.Key, Sum = sum};
            xs = xs.Monitor("Group", Order + 0.1);
            return xs.Select(m => m.Sum);
        }
    }
}
