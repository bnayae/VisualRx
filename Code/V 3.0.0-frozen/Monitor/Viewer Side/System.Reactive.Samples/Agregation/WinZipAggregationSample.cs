using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class WinZipAggregationSample : SampleBase<string>
    {
        public override string Title => "Win + Zip aggregation";

        public override double Order => (double)SampleOrder.WinZipAggregation;

        public override string Query
        {
            get
            {
                var query = @"IObservable<int> xs = ...;
var wins = xs.Window(TimeSpan.FromSeconds(2));
var ys = from win in wins
            from zip in Observable.Zip(                         
                win.Min(),
                win.Max(),
                win.Sum())
            select $""Min: { zip[0]}, Max: { zip[1]}, Sum { zip[2]}"";";
                return query;
            }
        }

        protected override IObservable<string> OnQuery()
        {
            var xs = Observable.Interval(TimeSpan.FromMilliseconds(500))
                .Take(15);
            xs = xs.Monitor("Source", Order + 0.1);
            var wins = xs.Window(TimeSpan.FromSeconds(2));
            var ys = from win in wins
                     from zip in Observable.Zip(                         
                         win.Min(),
                         win.Max(),
                         win.Sum())
                     select $"Min:{zip[0]}, Max:{zip[1]}, Sum {zip[2]}";
            ys = ys.Monitor("Zipped Aggregation", Order + 0.2);

            return ys;
        }
    }
}
