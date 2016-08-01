#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Contrib.Monitoring;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Contrib.Profiling.Proxies;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Concurrency;
using System.Reflection;

#endregion Using

namespace System.Reactive.Contrib.TestMonitor
{
    internal class Program
    {
        private const int STRESS_MILLISECOND_ITERATIONS = 3;
        private const int STRESS_MILLISECOND = 10;
        private const int STRESS_COUNT = 5000;
        private const string VIEWER_PROC_NAME = "Visual Rx";
        private const string VIEWER_EXE = @"Visual Rx.exe";
        private const string VIEWER_PATH = @"..\..\..\ViewerSide\" + VIEWER_EXE;
        private const string VIEWER_PATH_PROD = @"..\..\..\" + VIEWER_EXE;
        private const string CMD_LINE = @"PluginFolder:""..\Tests\Visual Tests\Sanity\Plugins""";
        private const string CMD_LINE_PROD = @"PluginFolder:"".\Tests\Visual Tests\Sanity\Plugins""";

        private static void Main(string[] args)
        {
            TryToOpenViewer();

            Thread.Sleep(1200);
            Console.WriteLine("Initialize");
            /* Create a ConsoleTraceListener and add it to the trace listeners. */
            var testProxy = MonitorTestProxy.Create();
            VisualRxSettings.CollectMachineInfo = true;
            Task<VisualRxInitResult> info = VisualRxSettings.Initialize(
                VisualRxWcfDiscoveryProxy.Create(),
                //VisualRxWcfFixedAddressProxy.CreateDefaultHttp(Environment.MachineName),
                //VisualRxWcfQueuedProxy.Create(),
                //VisualRxTraceSourceProxy.Create(),
                testProxy);
            
            #region Loaded Proxies Log

            VisualRxInitResult infos = info.Result;
            Console.WriteLine(infos);

            #endregion Loaded Proxies Log

            // the test proxy will publish only items with "Interval" keyword
            VisualRxSettings.AddFilter((marble, proxyKind) =>
                proxyKind != MonitorTestProxy.KIND ||
                proxyKind == MonitorTestProxy.KIND &&
                marble.Keywords.Contains("Interval"));

            IObservable<string> stream1 = Observable.Return("** 42 **").Monitor("Return", 0.1);
            IObservable<string> stream2 = Observable.Empty<string>().Monitor("Empty", 0.2);
            IObservable<string> stream3 = Observable.Never<string>().Monitor("Never", 0.3);
            IObservable<string> stream4 = Observable.Throw<string>(new Exception()).Monitor("Throw", 0.4);
            IObservable<int> stream5 = Observable.Generate(0, i => i < 5, i => i + 1, i => i, i => TimeSpan.FromSeconds(i))
                .Monitor("Generate", 0.4);

            stream1.Subscribe(Console.WriteLine);
            stream2.Subscribe(Console.WriteLine);
            stream3.Subscribe(Console.WriteLine);
            stream4.Subscribe(Console.WriteLine, ex => Console.WriteLine(ex));
            Thread.Sleep(3000);
            stream5.Subscribe(Console.WriteLine);

            #region Observable.Range

            var rengeStream = Observable.Range(1, 3)
                .Monitor("Range 3", orderingIndex: 1);

            rengeStream.Subscribe(item => Console.Write("{0},", item), () => Console.WriteLine("Complete"));

            #endregion Observable.Range

            #region 1 sec Interval Take 50

            var srm1 = Observable.Interval(TimeSpan.FromSeconds(1)).Take(50)
                .Monitor("Sec 1", 2, "Interval", "Merge");
            //srm1.Subscribe(Console.WriteLine);

            #endregion 1 sec Interval Take 50

            #region 3 sec Interval Take 14

            var srm2 = Observable.Interval(TimeSpan.FromSeconds(3))
                .Take(14)
                .Monitor("Sec 3", 2, "Interval", "Merge");
            //srm2.Subscribe(Console.WriteLine);

            #endregion 3 sec Interval Take 14

            #region 0.5 sec Interval With befor and after where

            var srm3 = Observable.Interval(TimeSpan.FromSeconds(0.5))
                .Take(19)
                .Monitor("Sec 0.5 Before Where", 4, "Interval", "Where")
                .Where(v => v % 3 == 0)
                .Monitor("After Where", 5, "Merge", "Where");
            //srm3.Subscribe(Console.WriteLine);

            #endregion 0.5 sec Interval With befor and after where

            #region Merge 1 sec, 3 sec and 0.5 sec Intervals

            Observable.Merge(srm1, srm2, srm3)
                .Monitor("After Merge", 6, "Merge")
                .Subscribe(Console.WriteLine);

            #endregion Merge 1 sec, 3 sec and 0.5 sec Intervals

            #region Observable.Throw

            var srm4 = Observable.Throw<NullReferenceException>(new NullReferenceException())
                .Monitor("Error", 7);
            srm4.Subscribe(Console.WriteLine, ex => Console.WriteLine("Handle Error: {0}", ex.Message));

            #endregion Observable.Throw

            Thread.Sleep(8000);

            #region Filter test

            bool isIntervalOnly = !testProxy.Data.Any(
                marble => !marble.Keywords.Contains("Interval"));
            if (!isIntervalOnly)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Test proxy is interval only = {0} / Count = {1}",
                isIntervalOnly, testProxy.Data.Count());
            Console.ResetColor();

            #endregion Filter test

            #region Stress Test

            for (int i = 0; i < STRESS_MILLISECOND_ITERATIONS; i++)
            {
                var srm5 = Observable.Interval(
                        TimeSpan.FromMilliseconds(STRESS_MILLISECOND),
                        NewThreadScheduler.Default)
                    .Take(STRESS_COUNT)
                    .Monitor("Stress " + i.ToString(), 100 + (i * 0.01),"Stress");
                srm5.Subscribe(Console.WriteLine);
            }

            #endregion Stress Test

            Console.ReadKey();
        }

        private static void TryToOpenViewer()
        {            
            Console.WriteLine("Try to open viewer");
            try
            {
                string location = Assembly.GetExecutingAssembly().Location;
                string baseDir = Path.GetDirectoryName(location);

                bool isViewerAlive = Process.GetProcessesByName(VIEWER_PROC_NAME).Any();
                if (!isViewerAlive)
                {
                    string arguments = null;
                    string dir = null;
                    if (File.Exists(VIEWER_PATH))
                    {
                        arguments = CMD_LINE;
                        dir = Path.Combine(baseDir, VIEWER_PATH);
                    }
                    else if (File.Exists(VIEWER_PATH_PROD))
                    {
                        arguments = CMD_LINE_PROD;
                        dir = Path.Combine(baseDir, VIEWER_PATH_PROD);
                    }
                    if (arguments != null)
                    {
                        var pinfo = new ProcessStartInfo
                        {
                            Arguments = arguments,
                            FileName = VIEWER_EXE,
                        };
                        dir = Path.GetDirectoryName(dir);
                        string workDir = Path.GetFullPath(dir);
                        if (Directory.Exists(workDir))
                            pinfo.WorkingDirectory = Path.GetFullPath(dir);
                        Process p = Process.Start(pinfo);
                        Thread.Sleep(2000);
                    }
                }
            }
            #region Exception Handling

            catch (Exception ex)
            {
                Console.WriteLine("Fail to open the viewer: {0}", ex);
            }

            #endregion Exception Handling
        }
    }
}