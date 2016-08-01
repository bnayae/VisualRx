using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class BufferCountSample : SampleBase<IList<long>>
    {
        public override string Title => "Buffer (count)";

        public override double Order => (double)SampleOrder.BufferCount;

        public override string Query
        {
            get
            {
                var query = @"var xs = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(10);
                                                 .Buffer(3);";
                return query;
            }
        }

        protected override IObservable<IList<long>> OnQuery()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(10);
            xs = xs.Monitor("Interval", Order + 0.1);
            var ys = xs.Buffer(3);
            ys = ys.Monitor("Buffer", Order + 0.2, (lst, marble) => string.Join(",", lst.ToArray()));
            return ys;
        }
    }
}
