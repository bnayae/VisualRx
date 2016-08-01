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
    public class WindowScenario : IScenario
    {
        private Action _act = () =>
            {
                var xs = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(10);
                xs = xs.Monitor("Interval", 1);
                IObservable<IObservable<long>> ys = xs.Window(3);
                ys = ys.MonitorMany("Window", 2);

                var sync = new ManualResetEventSlim();
                ys.Subscribe(win =>
                    {
                        win.Subscribe();
                    },
                    () => sync.Set());
                sync.Wait();
            };
        
        public string Title
        {
            get { return "Window"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
IObservable<long> xs = Observable.Interval(TimeSpan.FromSeconds(0.5))
                                 .Take(10);
IObservable<IObservable<long>> ys = xs.Window(3);

ys.Subscribe(win =>
    {
        win.Subscribe(v => {});
    });
";
            }
        }

        public double Order
        {
            get { return 10; }
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
