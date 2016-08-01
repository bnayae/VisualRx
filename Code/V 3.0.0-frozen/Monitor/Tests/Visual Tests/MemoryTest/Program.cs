using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Contrib.Monitoring;
using System.Text;
using System.Threading.Tasks;

namespace MemoryTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var info = VisualRxSettings.Initialize(
                VisualRxWcfDiscoveryProxy.Create());

            Console.WriteLine(info.Result);


            Console.ReadKey();
        }
    }
}
