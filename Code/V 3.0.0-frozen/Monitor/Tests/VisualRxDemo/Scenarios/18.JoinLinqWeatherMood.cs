using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VisualRxDemo.Scenarios
{
    public class JoinLinqWeatherMoodScenario : IScenario
    {
        private Action _act = () =>
            {
                #region IObservable<Weather> ws = ...

                IObservable<Weather> ws = Observable.Generate(0, i => i < 5, i => i + 1, i => (Weather)(i % 6),
                    i => i == 0 ? TimeSpan.Zero : TimeSpan.FromSeconds(3));

                ws = ws.Monitor("Weather", 1);
                ws = ws.Publish().RefCount();

                #endregion // IObservable<Weather> ws = ...
                
                #region IObservable<Mood> ms = ...

                IObservable<Mood> ms = Observable.Generate(0, i => i < 15, i => i + 1, i => (Mood)(i % 6),
                    i => i == 0 ? TimeSpan.FromSeconds(0.5) : TimeSpan.FromSeconds(1));

                ms = ms.Monitor("Moods", 2);

                #endregion // IObservable<Mood> ms = ...

                var join = from w in ws
                           join m in ms
                                on ws   // a stream which close the weather period
                                        // a stream which close the mood period (immediate closing)
                                equals Observable.Empty<Unit>() // closing mood (point)
                                //into moods // the IObservable<Mood> which related to the current weather
                            select new { Weather = w, Moods = m };

                join = join.Monitor("Joined Weather", 3);
                join.Wait();
            };
        
        public string Title
        {
            get { return "Join LINQ (Weather and Mood)"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
IObservable<Weather> ws = ...;
IObservable<Mood> ms = ...;

var join = from w in ws
            join m in ms
                on ws   // a stream which close the weather period
                        // a stream which close the mood period (immediate closing)
                equals Observable.Empty<Unit>() // closing mood (point)
                //into moods // the IObservable<Mood> which related to the current weather
            select new { Weather = w, Moods = m };

join.Subscribe(...);
";
            }
        }

        public double Order
        {
            get { return 18; }
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
