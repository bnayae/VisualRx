using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public static class Extensions 
    {
        public static IObservable<Sequence<T>> Seq<T>(
            this IObservable<T> source)
        {
            return Observable.Defer(() =>
                source.Scan(new Sequence<T>(-1, default(T)),
                            (prev, val) => new Sequence<T>(prev.Index + 1, val))
            );
        }

        public class Sequence<T>
        {
            public Sequence(int index, T value)
            {
                Index = index;
                Value = value;
            }
            public int Index { get; }
            public T Value { get; }

            public override string ToString()
            {
                return $"#{Index}: {Value}";
            }
        }
    }
}
