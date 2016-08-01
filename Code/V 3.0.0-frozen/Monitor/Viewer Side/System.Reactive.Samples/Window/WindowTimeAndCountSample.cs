using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class WindowTimeAndCountSample : SampleBase<int>
    {
        public override string Title => "Window (time and count)";

        public override double Order => (double)SampleOrder.WindowTimeAndCount;

        public override string Query
        {
            get
            {
                var query = @"IObservable<int> xs = ...;
var ys = xs.Window(TimeSpan.FromSeconds(2), 3);
                ";
                return query;
            }
        }

        protected override IObservable<int> OnQuery()
        {
            var xs = Observable.Generate(1, i => i < 15, i => i + 1, i => i ,
                i => TimeSpan.FromMilliseconds(i * 100));
            xs = xs.Monitor("Source", Order + 0.1);
            var ys = xs.Window(TimeSpan.FromSeconds(2), 3);
            ys = ys.MonitorMany("Window", Order + 0.2);
            return ys.SelectMany(m => m); ;
        }
    }
}
