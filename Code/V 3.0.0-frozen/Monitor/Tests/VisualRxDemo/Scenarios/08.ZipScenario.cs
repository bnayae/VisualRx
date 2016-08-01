using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class ZipScenario : IScenario
    {
        private Action _act = () =>
            {
                var rnd = new Random();
                var xs = Observable.Interval(TimeSpan.FromSeconds(1))
                    .Take(6);
                xs = xs.Monitor("Interval", 1);
                var ys = Observable.Timer(TimeSpan.FromSeconds(2.5),
                    TimeSpan.FromSeconds(1)).Take(6);
                ys = ys.Monitor("Timer", 2);
                var zs = xs.Zip(ys, (item1, item2) => string.Format("{0}, {1}", item1, item2));
                zs = zs.Monitor("Zip", 3);

                zs.Wait();
            };

        public string Title
        {
            get { return "Zip"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var rnd = new Random();
var xs = Observable.Interval(TimeSpan.FromSeconds(1))
    .Take(6);;
var ys = Observable.Timer(TimeSpan.FromSeconds(2.5),
    TimeSpan.FromSeconds(1)).Take(6);
var zs = xs.Zip(ys, 
    (item1, item2) => string.Format(""{0}, {1}"", item1, item2));

zs.Subscribe(v => {});
";
            }
        }

        public double Order
        {
            get { return 8; }
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
