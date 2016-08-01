using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class ScanScenario : IScenario
    {
        private Action _act = () =>
            {
                var xs = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(10);
                xs = xs.Monitor("Interval", 1);
                var ys = xs.Scan(0L, (acc, value) => acc + value);
                ys = ys.Monitor("Scan", 2);

                ys.Wait();
            };
        
        public string Title
        {
            get { return "Scan"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var xs = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(10);
var ys = xs.Scan(0L, (acc, value) => acc + value);
ys.Subscribe()
";
            }
        }

        public double Order
        {
            get { return 20; }
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
