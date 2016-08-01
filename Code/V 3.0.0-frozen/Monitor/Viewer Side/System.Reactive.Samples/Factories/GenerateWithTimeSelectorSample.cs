using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class GenerateWithTimeSelectorSample : SampleBase<int>
    {
        public override string Title => "Generate (with duration)";

        public override double Order => (double)SampleOrder.GenerateWithTimeSelector;

        public override string Query
        {
            get
            {
                var query = @"Observable.Generate(
1, // init
i => i <= 16, // condition
i => i * 2, // iterate
i => i, // select result
i => TimeSpan.FromSeconds(0.5 + i * 0.2));";
                return query;
            }
        }

        protected override IObservable<int> OnQuery()
        {
            var xs = Observable.Generate(
                1, // init
                i => i <= 16, // condition
                i => i * 2, // iterate
                i => i, // select result
                i => TimeSpan.FromSeconds(0.5 + i * 0.2)); // time selector
            xs = xs.Monitor("Generate", Order);
            return xs;
        }
    }
}
