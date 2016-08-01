using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class OHLCScenario : IScenario
    {
        private Action _act = () =>
            {
                var rnd = new Random();
                var xs = Observable.Generate(
                    100.0, 
                    m => m > 50 && m < 150,
                    m => m + (rnd.NextDouble() - 0.5) * 5,
                    m => m,
                    m => TimeSpan.FromMilliseconds(100))
                    .Take(35);
                xs = xs.Monitor("Source", 1);
                var ws = from w in xs.Window(TimeSpan.FromSeconds(1)).MonitorMany("Days", 2)
                         from acc in Observable.Zip(
                                                     w.FirstAsync().Monitor("First", 3),
                                                     w.Min().Monitor("Min", 4),
                                                     w.Max().Monitor("Max", 5),
                                                     w.LastAsync().Monitor("Last", 6))
                         select new { First = acc[0], Min = acc[1], Max = acc[2], Last = acc[3] };
                ws = ws.Monitor("OHLC", 7);

                ws.Wait();
            };
        
        public string Title
        {
            get { return "OHLC"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var xs = Observable.Generate(...)
    .Take(35);
var ws = from w in xs.Window(TimeSpan.FromSeconds(2))
            from acc in Observable.Zip(
                    w.FirstAsync(),
                    w.Min(),
                    w.Max()),
                    w.LastAsync())
            select new 
                {
                    First = acc[0], 
                    Min = acc[1], 
                    Max = acc[2], 
                    Last = acc[3]
                };
ws.Subscribe(...)
                ";
            }
        }

        public double Order
        {
            get { return 33; }
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
