using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class DistictScenario : IScenario
    {
        private Action _act = () =>
            {
                var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                                .Monitor("Origin", 0)
                                .Select(v => v % 5)
                                .Take(20);
                xs = xs.Monitor("Select 0.5 second", 1);
                var ys = xs.Distinct();
                ys = ys.Monitor("Distinct", 2);

                ys.Wait();
            };
        
        public string Title
        {
            get { return "Distict"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                .Select(v => v % 5)
                .Take(20);
var ys = xs.Distinct();
";
            }
        }

        public double Order
        {
            get { return 32; }
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
