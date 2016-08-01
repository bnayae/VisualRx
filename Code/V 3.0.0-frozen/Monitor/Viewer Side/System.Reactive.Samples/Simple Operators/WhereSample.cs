using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class WhereSample : SampleBase<long>
    {
        public override string Title => "Where";

        public override double Order => (double)SampleOrder.Where;

        public override string Query
        {
            get
            {
                var query = @"Observable.Interval(TimeSpan.FromSeconds(0.5))
                                .Take(10)
                               .Where(m = m % 2 == 0);";
                return query;
            }
        }

        protected override IObservable<long> OnQuery()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                                .Take(10)
                               .Monitor("Where Source", Order + 0.1)
                               .Where(m => m % 2 == 0)
                               .Monitor("Where Operator", Order + 0.2);
            return xs;
        }
    }
}
