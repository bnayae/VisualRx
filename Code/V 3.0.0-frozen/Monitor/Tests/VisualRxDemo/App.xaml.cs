using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace VisualRxDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string VIEWER_PROC_NAME = "Visual Rx";
        private const string VIEWER_EXE = @"Visual Rx.exe";
        private const string VIEWER_PATH = @"..\..\ViewerSide\" + VIEWER_EXE;
        private const string VIEWER_PATH_PROD = @"..\..\" + VIEWER_EXE;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //TryToOpenViewer();
        }

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
                        dir = Path.Combine(baseDir, VIEWER_PATH);
                    }
                    else if (File.Exists(VIEWER_PATH_PROD))
                    {
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
