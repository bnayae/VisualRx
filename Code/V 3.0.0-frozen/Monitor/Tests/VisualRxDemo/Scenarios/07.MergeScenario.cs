using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class MergeScenario : IScenario
    {
        private Action _act = () =>
            {
                var xs = Observable.Interval(TimeSpan.FromSeconds(2))
                    .Select(v => -v).Take(5);
                xs = xs.Monitor("Interval 1 second", 1);
                var ys = Observable.Timer(TimeSpan.FromSeconds(3), 
                    TimeSpan.FromSeconds(1.5)).Take(6);
                ys = ys.Monitor("Timer 3 second follow by 0.5 second", 2);
                var zs = Observable.Merge(xs, ys);
                zs = zs.Monitor("Merge", 3);

                zs.Wait();
            };
        
        public string Title
        {
            get { return "Merge"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var xs = Observable.Interval(TimeSpan.FromSeconds(2))
    .Select(v => -v).Take(5);
var ys = Observable.Timer(TimeSpan.FromSeconds(3), 
        TimeSpan.FromSeconds(1.5)).Take(6);
var zs = Observable.Merge(xs, ys);

zs.Subscribe(v => {});
";
            }
        }

        public double Order
        {
            get { return 7; }
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
