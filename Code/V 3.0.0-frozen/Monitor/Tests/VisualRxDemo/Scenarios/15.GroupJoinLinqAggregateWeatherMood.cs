using System;
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
    public class GroupJoinLinqAggregateWeatherMoodScenario : IScenario
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
                                into moods // the IObservable<Mood> which related to the current weather
                            let accMoods = moods.Aggregate(string.Empty, 
                                                  (prev, mood) => string.Format("{0}{1}, ", prev, mood)) 
                            from moodsAsString in accMoods
                            select new { Weather = w, Moods = moodsAsString};

                join = join.Monitor("Joined", 3);
                join.Wait();
            };
        
        public string Title
        {
            get { return "GroupJoin LINQ & Aggregate (Weather and Mood)"; }
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
                on ws   // closing weather
                equals Observable.Empty<Unit>() // closing mood (point)
                into moods // the IObservable<Mood> which related to the current weather
            let accMoods = moods.Aggregate(string.Empty, 
                                    (prev, mood) => string.Format(""{0}{1}, "", prev, mood)) 
            from moodsAsString in accMoods
            select new { Weather = w, Moods = moodsAsString};

join.Subscribe(tpl =>
        Console.WriteLine(""{0}, {1}"", tpl.Weather, tpl.Moods));
";
            }
        }

        public double Order
        {
            get { return 15; }
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
