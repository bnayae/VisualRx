#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion Using

namespace System.Reactive.Contrib.Monitoring.UnitTests
{
    [TestClass]
    public class SubjectTests : TestsBase
    {
        #region SubjectTest

        [TestMethod]
        public void SubjectTest()
        {
            ISubject<long> xs = new Subject<long>();
            var ys = xs.Monitor("Subject", 1);

            ys.Subscribe();

            xs.OnNext(1);
            Thread.Sleep(1000);
            ys.OnNext(2);

            Thread.Sleep(100);
            GC.KeepAlive(ys);
        }

        #endregion SubjectTest

        #region Subject1Test

        [TestMethod]
        public void Subject1Test()
        {
            ISubject<int, string> xs = new SimpleSubject<int, string>(l => l.ToString().PadRight(l, '*'));
            var ys = xs.Monitor("Subject", 1);

            ys.Subscribe();

            xs.OnNext(1);
            Thread.Sleep(1000);
            ys.OnNext(2);
            Thread.Sleep(1000);
            ys.OnNext(3);

            Thread.Sleep(100);
            GC.KeepAlive(ys);
        }

        #endregion Subject1Test

        #region SimpleSubject

        private class SimpleSubject<TIn, TOut> : ISubject<TIn, TOut>
        {
            private readonly ConcurrentQueue<IObserver<TOut>> _q = new ConcurrentQueue<IObserver<TOut>>();
            private readonly Func<TIn, TOut> _convert;

            public SimpleSubject(Func<TIn, TOut> convert)
            {
                _convert = convert;
            }

            public void OnCompleted()
            {
                IObserver<TOut>[] observers = _q.ToArray();
                foreach (var observer in observers)
                {
                    observer.OnCompleted();
                }
            }

            public void OnError(Exception error)
            {
                IObserver<TOut>[] observers = _q.ToArray();
                foreach (var observer in observers)
                {
                    observer.OnError(error);
                }
            }

            public void OnNext(TIn value)
            {
                IObserver<TOut>[] observers = _q.ToArray();
                foreach (var observer in observers)
                {
                    observer.OnNext(_convert(value));
                }
            }

            public IDisposable Subscribe(IObserver<TOut> observer)
            {
                _q.Enqueue(observer);
                return Disposable.Empty;
            }
        }

        #endregion SimpleSubject
    }
}