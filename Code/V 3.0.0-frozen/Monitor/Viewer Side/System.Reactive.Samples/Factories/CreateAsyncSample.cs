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
    public class CreateAsyncSample : SampleBase<int>
    {
        public override string Title => "Create Async";

        public override double Order => (double)SampleOrder.CreateAsync;

        public override string Query
        {
            get
            {
                var query = @"Observable.Create<int>(async observer =>
{
    for (int i = 0; i < 10; i++)
    {
        await Task.Delay(500);
        observer.OnNext(i);
    }
    observer.OnCompleted();
    return Disposable.Empty; // No way to be aware about disposal
});";
                return query;
            }
        }

        protected override IObservable<int> OnQuery()
        {
            var xs = Observable.Create<int>(async observer =>
            {
                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(500);
                    observer.OnNext(i);
                }
                observer.OnCompleted();
                return Disposable.Empty; // No way to be aware about disposal
            });
            xs = xs.Monitor("Create Async", Order);
            return xs;
        }
    }
}
