using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class JoinSample : SampleBase<Tuple<Weather, Mood>>
    {
        public override string Title => "Join (Weather and Mood)";

        public override double Order => (double)SampleOrder.Join;

        public override string Query
        {
            get
            {
                var query = @"IObservable<Weather> ws = ...;
IObservable<Mood> ms = ...;

IObservable<Tuple<Weather, Mood>> result =
    ws.Join(
            ms, // the join with stream 
            w => ws, // closing weather period
            m => Observable.Empty<Unit>(), // closing mood (point)";
                return query;
            }
        }

        protected override IObservable<Tuple<Weather, Mood>> OnQuery()
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

            IObservable<Tuple<Weather, Mood>> result =
                    ws.Join(
                       ms, // the join with stream 
                       w => ws, // closing weather period
                       m =>  Observable.Empty<Unit>(), // closing mood (point)
                       (w /* Weather */, m /* Mood */) => Tuple.Create(w, m));

            result = result.Monitor("Joined", 3, (tpl, m) => string.Format("{0}, {1}", tpl.Item1, tpl.Item2));
            return result;
        }
    }
}
