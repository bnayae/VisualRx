using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class GenerateScenario : IScenario
    {
        private Action _act = () =>
            {
                var xs = Observable.Generate(
                    1, // init
                    i => i <= 64, // condition
                    i => i * 2, // iterate
                    i => i, // select result
                    i => TimeSpan.FromSeconds(0.5)); // time selector
                xs = xs.Monitor("Generate", 4);

                xs.Wait();
            };
        
        public string Title
        {
            get { return "Generate"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var xs = Observable.Generate(
    1, // init
    i => i <= 64, // condition
    i => i * 2, // iterate
    i => i, // select result
    i => TimeSpan.FromSeconds(0.5)); // time selector

xs.Subscribe(v => {});
";
            }
        }

        public double Order
        {
            get { return 5; }
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
