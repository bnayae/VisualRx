using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class GroupByUntilScenario : IScenario
    {
        private Action _act = () =>
            {
                string[] stocks = { "Microsoft", "Apple", "Google", "Intel", "Amazon" };
                var rnd = new Random();
                var xs = Observable.Interval(TimeSpan.FromSeconds(1))
                                    .Take(30)
                                    .Select(i => 
                                                Tuple.Create(stocks[rnd.Next(0, stocks.Length)],
                                                            rnd.Next(10, 100)));

                xs = xs.Monitor("Interval 1 second", 1);
                var gs = xs.GroupByUntil(t => t.Item1,
                    g => g.Throttle(TimeSpan.FromSeconds(4)));
                gs = gs.MonitorGroup("Group", 3);

                var accs = from g in gs
                           from acc in g.Average(m => m.Item2)
                           select acc;
                accs = accs.Monitor("Average", 2);
                accs.Subscribe();
            };
        
        public string Title
        {
            get { return "Group By Until"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
string[] stocks = ...;
IObservable<Tuple<string, long>> xs =...;
var gs = xs.GroupByUntil(t => t.Item1,
    g => g.Throttle(TimeSpan.FromSeconds(2)));
var accs = from g in gs
            from acc in g.Average(m => m.Item2)
            select acc;
accs.Subscribe(...);
";
            }
        }

        public double Order
        {
            get { return 41; }
        }

        public ICommand Invoke
        {
            get
            {
                return new InvokeCommand(_act);
            }
        }
    }
}
