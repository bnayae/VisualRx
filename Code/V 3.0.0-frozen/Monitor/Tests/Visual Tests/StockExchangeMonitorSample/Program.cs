#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Contrib.Monitoring;
using System.Reactive.Contrib.Monitoring.Contracts;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion Using

namespace Bnaya.Samples
{
    internal class Program
    {
        private const double SPEED_SEC = 0.01;
        private const string VIEWER_PROC_NAME = "Visual Rx";
        private const string VIEWER_EXE = @"Visual Rx.exe";
        private const string VIEWER_PATH = @"..\..\..\ViewerSide\" + VIEWER_EXE;
        private const string VIEWER_PATH_PROD = @"..\..\..\" + VIEWER_EXE;
        private const string CMD_LINE = @"PluginFolder:""..\Tests\Visual Tests\StockExchange\Plugins""";
        private const string CMD_LINE_PROD = @"PluginFolder:"".\Tests\Visual Tests\StockExchange\Plugins""";
        private static ThreadLocal<Random> _rnd = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        private static void Main(string[] args)
        {
            TryToOpenViewer();

            var bc = new BlockingCollection<string>();

            Task<VisualRxInitResult> info = VisualRxSettings.Initialize(
               VisualRxWcfDiscoveryProxy.Create()//,
                //MonitorWcfQueuedProxy.Create(),
                //VisualRxTraceSourceProxy.Create()
                );

            #region Loaded Proxies Log

            VisualRxInitResult infos = info.Result;
            Console.WriteLine(infos);

            #endregion Loaded Proxies Log

            var surrogate = new MonitorSurrogate<IList<double>>(MarbleSerializationOptions.Serializable);

            var xs = StockProvider()
                .Buffer(2, 1)
                .Monitor("Stock", 100, surrogate);

            for (int i = 0; i < 4; i++)
			{
                int local = i;
                var ys = StockProvider(_rnd.Value.Next(50, 150))
                    .Buffer(2, 1)
                    .Monitor("StockGraph " + local, local, surrogate);
                xs = xs.Merge(ys);
			}
            xs.Subscribe(_ => bc.Add("."));

            foreach (var item in bc.GetConsumingEnumerable())
            {
                Console.Write(item);
            }
            GC.KeepAlive(xs);
        }

        #region StockProvider

        public static IObservable<double> StockProvider(double seed = 100)
        {
            var xs = Observable.Generate(
                seed,
                i => i > 0,
                i => i + (_rnd.Value.NextDouble() - 0.5) * 2,
                i => i,
                i => TimeSpan.FromSeconds(SPEED_SEC),
                NewThreadScheduler.Default);

            return xs;
        }

        #endregion StockProvider

        #region TryToOpenViewer

        /// <summary>
        /// Tries to open viewer.
        /// </summary>
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

        #endregion // TryToOpenViewer
    }
}