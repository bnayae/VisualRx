using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class GroupByScenario : IScenario
    {
        private Action _act = () =>
            {
                var xs = Observable.Interval(TimeSpan.FromSeconds(1)).Take(30);
                xs = xs.Monitor("Interval 1 second", 1);
                var ys = from item in xs
                         group item by item % 5;
                ys = ys.MonitorGroup("Group", 2);

                ys.Wait();
            };
        
        public string Title
        {
            get { return "GroupBy"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var xs = Observable.Interval(TimeSpan.FromSeconds(1)).Take(30);
var ys = from item in xs
            group item by item % 5;

ys.Subscribe(...);";
            }
        }

        public double Order
        {
            get { return 40; }
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
