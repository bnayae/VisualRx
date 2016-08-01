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
    public class JoinWeatherMoodScenario : IScenario
    {
        private static ThreadLocal<Random> _rnd = new ThreadLocal<Random>(() => new Random());

        private Action _act = () =>
            {
                #region IObservable<Weather> ws = ...

                IObservable<Weather> ws = Observable.Generate(0, i => i < 5, i => i + 1, i => (Weather)(i % 6), 
                    i => i == 0? TimeSpan.Zero : TimeSpan.FromSeconds(3));

                ws = ws.Monitor("Weather", 1);
                ws = ws.Publish().RefCount();

                #endregion // IObservable<Weather> ws = ...

                #region IObservable<Mood> ms = ...

                IObservable<Mood> ms = Observable.Generate(0, i => i < 15, i => i + 1, i => (Mood)(i % 6), 
                    i => i == 0? TimeSpan.FromSeconds(0.5) : TimeSpan.FromSeconds(1));

                ms = ms.Monitor("Moods", 2);

                #endregion // IObservable<Mood> ms = ...

                IObservable<Tuple<Weather, Mood>> result =
                   ws.Join(
                           ms, // the join with stream 
                           w => ws, // closing weather period
                           m => Observable.Empty<Unit>(), // closing mood (point)
                           (w /* Weather */, m /* Mood */) => Tuple.Create(w, m));

                result = result.Monitor("Joined", 3, (tpl, m) => string.Format("{0}, {1}", tpl.Item1, tpl.Item2));
                result.Wait();
            };

        public string Title
        {
            get { return "Join (Weather and Mood)"; }
        }

        public string Sample
        {
            get
            {
                return
                    @"
IObservable<Weather> ws = ...;
IObservable<Mood> ms = ...;

IObservable<Tuple<Weather, Mood>> result =
    ws.Join(
            ms, // the join with stream 
            w => ws, // closing weather period
            m => Observable.Empty<Unit>(), // closing mood (point)
            (w /* Weather */, m /* Mood */) => Tuple.Create(w, m));            
";
            }
        }

        public double Order
        {
            get { return 12; }
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
