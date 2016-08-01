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
    public class SampleScenario : IScenario
    {
        private Action _act = () =>
            {
                var xs = Observable.Create<int>(async obs =>
                            {
                                obs.OnNext(1);
                                await Task.Delay(1000).ConfigureAwait(false);
                                obs.OnNext(2);
                                await Task.Delay(4000).ConfigureAwait(false);
                                obs.OnNext(3);
                                await Task.Delay(500).ConfigureAwait(false);
                                obs.OnNext(4);
                                await Task.Delay(3000).ConfigureAwait(false);
                                for (int i = 0; i < 10; i++)
                                {
                                    await Task.Delay(500).ConfigureAwait(false);
                                    obs.OnNext(i + 5);
                                }
                                obs.OnCompleted();
                                return Disposable.Empty;
                            })
                                .Monitor("Origin", 0)
                                .Sample(TimeSpan.FromSeconds(2));
                xs = xs.Monitor("Sample", 1);
                xs.Subscribe();
            };
        
        public string Title
        {
            get { return "Sample"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var xs = Observable.Create(...)
                .Sample(TimeSpan.FromSeconds(2))
                .Take(20);
";
            }
        }

        public double Order
        {
            get { return 30; }
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
