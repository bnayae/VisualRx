using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class SelectScenario : IScenario
    {
        private Action _act = () =>
            {
                var xs = Observable.Interval(TimeSpan.FromSeconds(1)).Take(10);
                xs = xs.Monitor("Interval 1 second", 1);
                var ys = from item in xs select new string('*', (int)item);
                ys = ys.Monitor("Selected", 2);

                ys.Wait();
            };
        
        public string Title
        {
            get { return "Select"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var xs = Observable.Interval
            (TimeSpan.FromSeconds(1)).Take(10);
var ys = from item in xs 
            select new string('*', (int)item);
ys.Subscribe(v => {});
";
            }
        }

        public double Order
        {
            get { return 1; }
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
