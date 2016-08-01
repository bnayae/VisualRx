using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class CommonAggregationsSample : SampleBase<double>
    {
        public override string Title => "Common Aggregations";

        public override double Order => (double)SampleOrder.CommonAggregators;

        public override string Query
        {
            get
            {
                var query = @"IObservable<int> xs = ...;
var min = xs.Min();
var max = xs.Max();
var sum = xs.Sum();
var average = xs.Average();";
                return query;
            }
        }

        protected override IObservable<double> OnQuery()
        {
            var xs = Observable.Interval(TimeSpan.FromMilliseconds(500))
             .Take(6);
            #region Monitor

            xs = xs.Monitor("Source", Order + 0.1);

            #endregion // Monitor
            var min = xs.Min();
            #region Monitor

            min = min.Monitor("Min", Order + 0.2);

            #endregion // Monitor
            var max = xs.Max();
            #region Monitor

            max = max.Monitor("Max", Order + 0.3);

            #endregion // Monitor
            var sum = xs.Sum();
            #region Monitor

            sum = sum.Monitor("Sum", Order + 0.4);

            #endregion // Monitor
            var average = xs.Average();
            #region Monitor

            average = average.Monitor("Average", Order + 0.5);

            #endregion // Monitor
            var custom = xs.Aggregate((acc, cur) => acc + cur);
            #region Monitor

            custom = custom.Monitor("Custom", Order + 0.6);

            #endregion // Monitor
            var scan = xs.Scan((acc, cur) => acc + cur);
            #region Monitor

            scan = scan.Monitor("Scan", Order + 0.7);

            #endregion // Monitor
            var scan1 = xs.Scan(Tuple.Create(0L, 0L),
                (acc, cur) => Tuple.Create(acc.Item1 + acc.Item2, cur));
            #region Monitor

            scan1 = scan1.Monitor("Scan1", Order + 0.8);

            #endregion // Monitor

            var ys = Observable.Merge(
                average,
                min.Select(m => (double)m),
                max.Select(m => (double)m),
                sum.Select(m => (double)m),
                scan.Select(m => (double)m),
                scan1.Select(m => 1.0),
                custom.Select(m => (double)m));
            return ys;
        }
    }
}
