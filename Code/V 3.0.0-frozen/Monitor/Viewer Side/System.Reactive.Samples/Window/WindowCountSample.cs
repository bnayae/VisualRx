using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class WindowCountSample : SampleBase<long>
    {
        public override string Title => "Window (count)";

        public override double Order => (double)SampleOrder.WindowCount;

        public override string Query
        {
            get
            {
                var query = @"var xs = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(10);
                                                 .Window(3);";
                return query;
            }
        }

        protected override IObservable<long> OnQuery()
        {
            var xs = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(10);
            xs = xs.Monitor("Interval", Order + 0.1);
            var ws = xs.Window(3);
            ws = ws.MonitorMany("Window", Order + 0.2);

            var zs = from w in ws
                     from acc in w.Sum()
                     select acc;

            //var zs1 = ws.SelectMany(w => w.Sum());

            zs = zs.Monitor("Aggregation", Order + 0.3);

            return zs;
        }
    }
}
