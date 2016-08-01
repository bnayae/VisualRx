using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class CustomWindowScenario : IScenario
    {
        private Action _act = () =>
            {
                int counter = 4;
                var rnd = new Random();
                var xs = Observable.Interval(TimeSpan.FromMilliseconds(100))
                            .Take(80);
                xs = xs.Monitor("Source", 1);
                var ws = xs.Window(
                                Observable.Interval(TimeSpan.FromSeconds(1)), // opening
                                open => Observable.Timer(TimeSpan.FromSeconds(Math.Abs(--counter % 4) + 1))
                                );
                ws = ws.MonitorMany("Windows", 7);

                ws.Subscribe(w =>
                {
                    w.Subscribe(_ => { });                    
                });
            };
        
        public string Title
        {
            get { return "Custom Window"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var rnd = new Random();
var xs = Observable.Interval(TimeSpan.FromMilliseconds(100)).Take(35);
var ws = xs.Window(
                Observable.Interval(TimeSpan.FromSeconds(1))
                            .Timestamp(), // opening
                ts => Observable.Timer(
                                TimeSpan.FromSeconds(
                                    ts.Timestamp.Second % 3))
                );

ws.Switch().Subsribe(...);
                ";
            }
        }

        public double Order
        {
            get { return 34; }
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
