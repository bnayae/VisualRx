using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reactive.Samples
{
    public enum SampleOrder
    {
        Select,
        Where,
        TimerVsInterval,
        ReturnVsRange,
        Generate,
        GenerateWithTimeSelector,
        CreateBlocking,
        CreateAsync,
        CreateAsyncWithCancellation,
        Sample,
        SampleWithCustomTrigger,
        Merge,
        MergeFluent,
        MergeFaulted,
        Zip,
        ZipFluent,
        ZipSameTypes,
        ZipFaulted,
        ZipLeak,
        CombineLatest,
        BufferCount,
        BufferSlidingCount,
        BufferTime,
        BufferTimeAndCount,
        BufferCustomTriggerCount,
        WindowCount,
        WindowTime,
        WindowTimeAndCount,
        CommonAggregators,
        MinMaxBy,
        Aggregate,
        Scan,
        ScanLast5,
        WindowSum,
        WinZipAggregation,
        Join,
        GroupByLinq,
        GroupByFluent,
        GroupBySumLinq,
        GroupByScanLinq,
        GroupByUntilSum,
        GroupJoinFluent,
        GroupJoinLinq,
    }
}
