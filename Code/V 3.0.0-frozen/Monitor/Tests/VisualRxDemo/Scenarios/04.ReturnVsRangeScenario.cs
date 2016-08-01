using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class ReturnVsRangeScenario : IScenario
    {
        private Action _act = () =>
            {
                var xs = Observable.Return(42);
                xs = xs.Monitor("Return", 1);
                var ys = Observable.Range(10,5);
                ys = ys.Monitor("Range", 2);
                xs.Subscribe();

                ys.Wait();
                GC.KeepAlive(xs);
            };
        
        public string Title
        {
            get { return "Return Vs. Range"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var xs = Observable.Return(42);
var ys = Observable.Range(10,5);
xs.Subscribe(v => {});
ys.Subscribe(v => {});
";
            }
        }

        public double Order
        {
            get { return 4; }
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
