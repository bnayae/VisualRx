using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class GroupJoinFluent : SampleBase<Mood>
    {
        public override string Title => "Group Join (Fluent)";

        public override double Order => (double)SampleOrder.GroupJoinFluent;

        public override string Query
        {
            get
            {
                var query = @"IObservable<Weather> ws = ...;
IObservable<Mood> ms = ...;

var join = ws.GroupJoin(ms,
    item => ws, // closing weather period
    item => Observable.Empty<Unit>(), // closing mood (point)
    (w /* Weather */, moods) => new {Weather = w, Moods = moods};";
                return query;
            }
        }

        protected override IObservable<Mood> OnQuery()
        {
            #region IObservable<Weather> ws = ...

            IObservable<Weather> ws = Observable.Generate(0, i => i < 5, i => i + 1, i => (Weather)(i % 6),
                i => i == 0 ? TimeSpan.Zero : TimeSpan.FromSeconds(3));

            ws = ws.Monitor("Weather", Order);
            ws = ws.Publish().RefCount();

            #endregion // IObservable<Weather> ws = ...

            #region IObservable<Mood> ms = ...

            IObservable<Mood> ms = Observable.Generate(0, i => i < 15, i => i + 1, i => (Mood)(i % 6),
                i => i == 0 ? TimeSpan.FromSeconds(0.5) : TimeSpan.FromSeconds(1));

            ms = ms.Monitor("Moods", Order + 0.1);

            #endregion // IObservable<Mood> ms = ...

            int count = 0;
            var join = ws.GroupJoin(ms,
                item => ws, // closing weather period
                item => Observable.Empty<Unit>(), // closing mood (point)
                (w /* Weather */, moods) =>
                {
                    double c = Interlocked.Increment(ref count) * 0.01;
                    var monitored = moods.Monitor($"{w} #{c}", Order + 0.4 + c);
                    return new
                    {
                        Weather = w,
                        Moods = monitored
                    };
                });

            join = join.Monitor("Joined Weather", Order + 0.3, (t, m) => t.Weather.ToString());


            return join.SelectMany(m => m.Moods);
        }
    }
}
