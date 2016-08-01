using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class SelectSample : SampleBase<string>
    {
        public override string Title => "Select";

        public override double Order => (double)SampleOrder.Select;

        public override string Query
        {
            get
            {
                var query = @"var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
            .Take(10);
var ys = from i in xs
    select new string('*', (int)i + 1);";
                return query;
            }
        }

        protected override IObservable<string> OnQuery()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                                .Take(10)
                               .Monitor("Select Source", Order + 0.1);
            var ys = from i in xs
                     select new string('*', (int)i + 1);
            return ys.Monitor("Select Operator", Order + 0.2);
        }
    }
}
