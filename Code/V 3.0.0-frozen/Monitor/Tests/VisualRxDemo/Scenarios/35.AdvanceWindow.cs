using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class AdvanceWindowScenario : IScenario
    {
        private Action _act = () =>
            {
                var xs = Observable.Create<int>(async o =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        await Task.Delay(500);
                        o.OnNext(i);
                    }

                    await Task.Delay(2000);
                    for (int i = 3; i < 6; i++)
                    {
                        await Task.Delay(500);
                        o.OnNext(i);
                    }

                    await Task.Delay(1500);
                    for (int i = 3; i < 6; i++)
                    {
                        await Task.Delay(500);
                        o.OnNext(i);
                    }

                    o.OnCompleted();
                    return Disposable.Empty;
                });
                xs = xs.Publish().RefCount();
                xs = xs.Monitor("Source", 1);
                var ts = xs.Throttle(TimeSpan.FromSeconds(0.7)).Publish().RefCount();
                ts = ts.Monitor("Throttle", 2);
                var ws = from w in xs.Window(ts).MonitorMany("Windows", 6)
                         from z in Observable.Zip(
                                w.FirstOrDefaultAsync().Monitor("First", 3),
                                w.LastOrDefaultAsync().Monitor("Last", 4))
                         select $"{z[0]} -> {z[1]}";
                ws = ws.Monitor("Result", 5);

                ws.Subscribe();
            };
        
        public string Title
        {
            get { return "Advance Window"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var xs = Observable.Interval(TimeSpan.FromMilliseconds(100)).Take(35);
var ts = xs.Throttle(TimeSpan.FromSeconds(0.7));
var ws = from w in xs.Window(ts)
            from z in Observable.Zip(
                w.FirstOrDefaultAsync(),
                w.LastOrDefaultAsync())
            select ...; 
ws.Subsribe(...);
";
            }
        }

        public double Order
        {
            get { return 35; }
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
