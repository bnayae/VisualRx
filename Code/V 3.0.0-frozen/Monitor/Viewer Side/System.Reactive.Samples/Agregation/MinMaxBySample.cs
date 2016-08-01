using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class MinMaxBySample : SampleBase<IList<Person>>
    {
        public override string Title => "Min By";

        public override double Order => (double)SampleOrder.MinMaxBy;

        public override string Query
        {
            get
            {
                var query = @"
*****************************
Return all value when the key is Min / Max
*****************************

IObservable<Person> xs = ...;
var mins = from w in source.Window(3)
            from min in w.MinBy(p => p.Value)
            select min;";
                return query;
            }
        }

        protected override IObservable<IList<Person>> OnQuery()
        {
            var source = Observable.Interval(TimeSpan.FromSeconds(0.1))
                                    .Select(_ => new Person())
                                    .Take(200)
                                    .Monitor("Source", Order);
            var wins = source.Window(40);
            wins = wins.MonitorMany("Win", Order + 0.1);
            var mins = from w in wins
                       from min in w.MinBy(p => p.Value)
                       select min;
            mins = mins.Monitor("Min", Order + 0.2, (v,m) => string.Join(",", v));
            return mins;
        }
    }

    public class Person
    {
        private static int _counter = 0;
        private static Random _rnd = new Random(Guid.NewGuid().GetHashCode());
        public Person()
        {
            Id = _counter++;
            Value = _rnd.Next(0, 50);
        }
        public int Id { get; set; }
        public double Value { get; set; }

        public override string ToString()
        {
            return $"Person [{Id}]={Value}";
        }
    }
}
