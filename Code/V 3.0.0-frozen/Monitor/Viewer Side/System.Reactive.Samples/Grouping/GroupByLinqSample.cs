﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring.UI.Contracts;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public class GroupByLinqSample : SampleBase<long>
    {
        public override string Title => "GroupBy Linq";

        public override double Order => (double)SampleOrder.GroupByLinq;

        public override string Query
        {
            get
            {
                var query = @"var source = Observable.Interval(TimeSpan.FromSeconds(0.5))
    var xs = from i in source
                group i by i % 5;
                ";
                return query;
            }
        }

        protected override IObservable<long> OnQuery()
        {
            var source = Observable.Interval(TimeSpan.FromSeconds(0.5))
                                   .Monitor("Source", Order).Take(30);
            var xs = from i in source
                     group i by i % 5;
            xs = xs.MonitorGroup("Group", Order + 0.1);
            return xs.SelectMany(g => g);
        }
    }
}
