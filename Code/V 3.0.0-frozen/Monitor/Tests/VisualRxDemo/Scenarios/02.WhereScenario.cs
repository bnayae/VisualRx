using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class WhereScenario : IScenario
    {
        private Action _act = () =>
            {
                var xs = Observable.Interval(TimeSpan.FromSeconds(1)).Take(10);
                xs = xs.Monitor("Interval 1 second", 1);
                xs = from item in xs 
                     where item % 2 == 0
                     select item;
                xs = xs.Monitor("Where", 2);

                xs.Wait();
            };
        
        public string Title
        {
            get { return "Where"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var xs = Observable.Interval(TimeSpan.FromSeconds(1)).Take(10);
xs = from item in xs 
        where item % 2 == 0
        select item;
xs.Subscribe(v => {});
";
            }
        }

        public double Order
        {
            get { return 2; }
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
