using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class WindowSumScenario : IScenario
    {
        private Action _act = () =>
            {
                IObservable<long> xs = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(10);
                xs = xs.Monitor("Interval", 1);
                IObservable<IObservable<long>> windows = xs.Window(3);
                windows = windows.MonitorMany("Window", 2);

                IObservable<long> sums = from win in windows
                           let sum = win.Sum()
                           from item in sum
                           select item;
                sums = sums.Monitor("Sum", 3);
                sums.Wait();
            };
        
        public string Title
        {
            get { return "Window + Sum"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
IObservable<long> xs = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(10);

IObservable<IObservable<long>> windows = xs.Window(3);

IObservable<long> sums = from win in windows
            let sum = win.Sum()
            from item in sum
            select item;

sums.Subscribe(item => ... );
";
            }
        }

        public double Order
        {
            get { return 11; }
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
