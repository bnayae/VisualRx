using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class IntervalVsTimerScenario : IScenario
    {
        private Action _act = () =>
            {
                var xs = Observable.Interval(TimeSpan.FromSeconds(1)).Take(5);
                xs = xs.Monitor("Interval 1 second", 1);
                var ys = Observable.Timer(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(0.5)).Take(10);
                ys = ys.Monitor("Timer 3 second follow by 0.5 second", 2);
                xs.Subscribe();

                ys.Wait();
                GC.KeepAlive(xs);
            };
        
        public string Title
        {
            get { return "Interval Vs. Timer"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var xs = Observable.Interval(
               TimeSpan.FromSeconds(1)).Take(5);
var ys = Observable.Timer(TimeSpan.FromSeconds(3), 
               TimeSpan.FromSeconds(0.5)).Take(10);
xs.Subscribe(v => {});
ys.Subscribe(v => {});
";
            }
        }

        public double Order
        {
            get { return 3; }
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
