using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class BufferSlidingCountWithCUstomTriggers : SampleBase<IList<long>>
    {
        public override string Title => "Buffer (custom trigger)";

        public override double Order => (double)SampleOrder.BufferCustomTriggerCount;

        public override string Query
        {
            get
            {
                var query = @"var xs = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(10);
var startTrigger = xs.Where(m => m % 4 == 0);                   
xs = xs.Buffer(startTrigger);";
                return query;
            }
        }

        protected override IObservable<IList<long>> OnQuery()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                .Take(15)
                .Monitor("Interval", Order + 0.1)
               .Publish(hot =>
                {
                    var startTrigger = hot.Where(m => m % 4 == 0);
                    var ys = hot.Buffer(startTrigger)
                                .Where(m => m.Count != 0);
                    ys = ys.Monitor("Buffer", Order + 0.2, (lst, marble) => string.Join(",", lst.ToArray()));
                    return ys;
                });
            return xs; 
        }

        //protected override IObservable<IList<long>> OnQuery()
        //{
        //    var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
        //        .Take(15)
        //        .Publish()
        //        .RefCount();

        //    xs = xs.Monitor("Interval", Order + 0.1);
        //    var startTrigger = xs.Where(m => m % 4 == 0);
        //    var ys = xs.Buffer(startTrigger)
        //                .Where(m => m.Count != 0);
        //    ys = ys.Monitor("Buffer", Order + 0.2, (lst, marble) => string.Join(",", lst.ToArray()));
        //    return ys;
        //}
    }
}
