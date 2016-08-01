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
    public class GroupByUntilWithSumLinqSample : SampleBase<long>
    {
        public override string Title => "GroupBy Until With Sum";

        public override double Order => (double)SampleOrder.GroupByUntilSum;

        public override string Query
        {
            get
            {
                var query = @"var source = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(20)
            var groups = source.GroupByUntil(m => m % 3, g => g.Skip(2).FirstOrDefaultAsync());
          
            var xs = from g in groups
                     from sum in g.Sum()
                     select new { g.Key, Sum = sum};";
                return query;
            }
        }

        protected override IObservable<long> OnQuery()
        {
            var source = Observable.Interval(TimeSpan.FromSeconds(0.5))
                                   .Monitor("Source", Order).Take(20);
          
            var groups = source.GroupByUntil(m => m % 3, g => g.Skip(2).FirstOrDefaultAsync());
          
            var xs = from g in groups
                     from sum in g.Sum()
                     select new { g.Key, Sum = sum};
            xs = xs.Monitor("Group", Order + 0.1);
            return xs.Select(m => m.Sum);
        }
    }
}
