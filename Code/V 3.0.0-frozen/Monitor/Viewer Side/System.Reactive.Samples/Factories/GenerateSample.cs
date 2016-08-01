using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class GenerateSample : SampleBase<int>
    {
        public override string Title => "Generate";

        public override double Order => (double)SampleOrder.Generate;

        public override string Query
        {
            get
            {
                var query = @"Observable.Generate(
1, // init
i => i <= 64, // condition
i => i * 2, // iterate
i => i /* select result */);";
                return query;
            }
        }

        protected override IObservable<int> OnQuery()
        {
            var xs = Observable.Generate(
                        1, // init
                        i => i <= 64, // condition
                        i => i * 2, // iterate
                        i => i /* select result */); 
            xs = xs.Monitor("Generate", Order);
            return xs;
        }
    }
}
