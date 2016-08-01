using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class CreateScenario : IScenario
    {
        private Action _act = () =>
            {
                var xs = Observable.Create<int>(observer =>
                    {
                        var disp = new BooleanDisposable();
                        Task.Run(() =>
                            {
                                for (int i = 0; i < 10; i++)
                                {
                                    if (disp.IsDisposed)
                                        break;
                                    Thread.Sleep(500);
                                    observer.OnNext(i);
                                }
                                observer.OnCompleted();
                            });
                        return disp;
                    }); 
                xs = xs.Monitor("Create", 5);

                xs.Wait();
            };
        
        public string Title
        {
            get { return "Create"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
var xs = Observable.Create<int>(observer =>
    {
        var disp = new BooleanDisposable();
        Task.Run(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    if (disp.IsDisposed)
                        break;
                    Thread.Sleep(500);
                    observer.OnNext(i);
                }
                observer.OnCompleted();
            });
        return disp;
    }); 

xs.Subscribe(v => {});
";
            }
        }

        public double Order
        {
            get { return 6; }
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
