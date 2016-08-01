using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class ReturnVsRangeSample : SampleBase<int>
    {
        public override string Title => "Return vs. Range";

        public override double Order => (double)SampleOrder.ReturnVsRange;

        public override string Query
        {
            get
            {
                var query = @"Observable.Return(42);
Observable.Range(10, 5);";
                return query;
            }
        }

        protected override IObservable<int> OnQuery()
        {
            var xs = Observable.Return(42);
            xs = xs.Monitor("Return", Order + 0.1);
            var ys = Observable.Range(10, 5);
            ys = ys.Monitor("Range", Order + 0.2);
            return Observable.Merge(xs, ys);
        }
    }
}
