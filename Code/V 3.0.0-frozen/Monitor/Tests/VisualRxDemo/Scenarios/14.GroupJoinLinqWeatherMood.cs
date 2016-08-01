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
    public class GroupJoinLinqWeatherMoodScenario : IScenario
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

                //var join = ws.GroupJoin(ms,
                //    item => ws, // closing weather period
                //    item => Observable.Empty<Unit>(), // closing mood (point)
                //    (w /* Weather */, moods) => new { Weather = w, Moods = moods });

                var join = from w in ws
                           join m in ms
                                on ws   // a stream which close the weather period
                                        // a stream which close the mood period (immediate closing)
                                equals Observable.Empty<Unit>() // closing mood (point)
                                into moods // the IObservable<Mood> which related to the current weather
                            select new { Weather = w, Moods = moods };

                join = join.Monitor("Joined Weather", 3, (t, m) => t.Weather.ToString());

                int index = 0;
                join.Subscribe(tpl =>
                    {
                        int tmp = Interlocked.Increment(ref index);
                        var moodStream = tpl.Moods
                            //.Distinct()
                            .Monitor(tpl.Weather.ToString(), tmp + 3);
                        moodStream.Subscribe();
                    });

                //join.Wait();
            };
        
        public string Title
        {
            get { return "GroupJoin LINQ (Weather and Mood)"; }
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
                on ws  // closing weather scope
                equals Observable.Empty<Unit>() // closing mood (point)
                into moods // moods stream related to a weather
            select new { Weather = w, Moods = moods };

join.Subscribe(tpl =>
    {
        Console.WriteLine(tpl.Weather)
        tpl.Moods.Subscribe(mood => Console.WriteLine(mood));
    });
";
            }
        }

        public double Order
        {
            get { return 14; }
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
