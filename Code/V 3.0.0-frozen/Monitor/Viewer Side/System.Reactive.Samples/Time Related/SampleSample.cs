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
    public class SampleSample : SampleBase<int>
    {
        public override string Title => "Sample";

        public override double Order => (double)SampleOrder.Sample;

        public override string Query
        {
            get
            {
                var query = @"     
IObservable<int xs = ....      
xs = xs.Sample(TimeSpan.FromSeconds(1.5));
                ";
                return query;
            }
        }

        protected override IObservable<int> OnQuery()
        {
            var xs = Observable.Create<int>(async observer =>
            {
                var disp = new BooleanDisposable();
                for (int i = 1; i <= 20; i++)
                {
                    if (disp.IsDisposed)
                        break;
                    await Task.Delay(150 * i);
                    observer.OnNext(i);
                }
                observer.OnCompleted();
                return disp;
            });
            xs = xs.Monitor("Source", Order)
                    .Sample(TimeSpan.FromSeconds(1.5))
                    .Monitor("Sample", Order + 0.1);
            return xs;
        }
    }
}
