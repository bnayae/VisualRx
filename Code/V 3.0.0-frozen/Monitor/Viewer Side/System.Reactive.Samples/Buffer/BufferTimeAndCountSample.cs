using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class BufferTimeAndCountSample : SampleBase<IList<int>>
    {
        public override string Title => "Buffer (time and count)";

        public override double Order => (double)SampleOrder.BufferTimeAndCount;

        public override string Query
        {
            get
            {
                var query = @"IObservable<int> xs = ...;
var ys = xs.Buffer(TimeSpan.FromSeconds(2), 3);
                ";
                return query;
            }
        }

        protected override IObservable<IList<int>> OnQuery()
        {
            var xs = Observable.Generate(1, i => i < 15, i => i + 1, i => i ,
                i => TimeSpan.FromMilliseconds(i * 100));
            xs = xs.Monitor("Source", Order + 0.1);
            var ys = xs.Buffer(TimeSpan.FromSeconds(2), 3);
            ys = ys.Monitor("Buffer", Order + 0.2, (lst, marble) => string.Join(",", lst.ToArray()));
            return ys;
        }
    }
}
