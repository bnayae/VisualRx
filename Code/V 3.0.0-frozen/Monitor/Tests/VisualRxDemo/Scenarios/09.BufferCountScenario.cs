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
    public class BufferScenario : IScenario
    {
        private Action _act = () =>
            {
                var xs = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(10);
                xs = xs.Monitor("Interval", 1);
                var ys = xs.Buffer(3);
                ys = ys.Monitor("Buffer", 2, (lst, marble) => string.Join(",", lst.ToArray()));

                ys.Wait();
            };
        
        public string Title
        {
            get { return "Buffer"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var xs = Observable.Interval(TimeSpan.FromSeconds(0.5)).Take(10);
var ys = xs.Buffer(3);
ys.Subscribe(v => {});
";
            }
        }

        public double Order
        {
            get { return 9; }
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
