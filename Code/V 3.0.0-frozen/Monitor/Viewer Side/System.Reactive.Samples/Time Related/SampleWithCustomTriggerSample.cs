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
    public class SampleWithCustomTriggerSample : SampleBase<long>
    {
        public override string Title => "Sample With Trigger";

        public override double Order => (double)SampleOrder.SampleWithCustomTrigger;

        public override string Query
        {
            get
            {
                var query = @"     
var trigger = Observable.Timer(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1));
IObservable<int xs = ....      
xs = xs.Sample(trigger);
                ";
                return query;
            }
        }

        protected override IObservable<long> OnQuery()
        {
            var source = Observable.Interval(TimeSpan.FromSeconds(0.2)).Take(20);
            var trigger = Observable.Timer(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(2))
                                    .Publish().RefCount().Take(20);

            //trigger.Monitor("Trigger", Order + 0.1)
            //    .Subscribe();
            //Thread.Sleep(1000);
            source = source.Monitor("Source", Order)
                    .Sample(trigger)
                    .Monitor("Sample", Order + 0.2);


            return source;
        }
    }
}
