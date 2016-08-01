using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class CreateBlockingSample : SampleBase<int>
    {
        public override string Title => "Create Blocking";

        public override double Order => (double)SampleOrder.CreateBlocking;

        public override string Query
        {
            get
            {
                var query = @"Observable.Create<int>(observer =>
    {
        for (int i = 0; i < 10; i++)
        {
            Thread.Sleep(500);
            observer.OnNext(i);
        }
        observer.OnCompleted();

        return Disposable.Empty; 
    });";
                return query;
            }
        }

        protected override IObservable<int> OnQuery()
        {
            var xs = Observable.Create<int>(observer =>
            {
                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(500);
                    observer.OnNext(i);
                }
                observer.OnCompleted();

                return Disposable.Empty; // completed when reach this point
            });
            xs = xs.Monitor("Create", Order);
            return xs;
        }
    }
}
