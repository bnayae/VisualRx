using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class WindowSumSample : SampleBase<int>
    {
        public override string Title => "Window (sum)";

        public override double Order => (double)SampleOrder.WindowSum;

        public override string Query
        {
            get
            {
                var query = @"IObservable<int> xs = ...;
var wins = xs.Window(TimeSpan.FromSeconds(2));
var ys = from win in wins
            from sum in win.Sum()
            select sum;";
                return query;
            }
        }

        protected override IObservable<int> OnQuery()
        {
            var xs = Observable.Generate(1, i => i < 15, i => i + 1, i => i ,
                i => TimeSpan.FromMilliseconds(i * 100));
            xs = xs.Monitor("Source", Order + 0.1);
            var wins = xs.Window(TimeSpan.FromSeconds(2));
            var ys = from win in wins
                     from sum in win.Sum()
                     select sum;
            ys = ys.Monitor("Sum", Order + 0.2);

            return ys;
        }
    }
}
