using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using U5kManServer.Services;

namespace UnitTesting
{
    [TestClass]
    public class CentralServicesMonitorTest
    {
        const int levelTrace = 1;
        CentralServicesMonitor MonitorOfServices;
        void PrepareMonitor()
        {
            MonitorOfServices = new CentralServicesMonitor(() =>
            {
                return true;
            }, (alarma, str1, str2, str3) =>
            {
                Debug.WriteLine(String.Format("Event: {0}, {1}, {2}, {3}", alarma, str1, str2, str3));
                MonitorDataSave();
            }, (m, x) =>
            {
                Debug.WriteLine(String.Format("{0}: {1}", x==null ? "Msg: " : "Err: ",
                    m + x?.Message));
            }, (l, m) =>
            {
                if (l <= levelTrace)
                    Debug.WriteLine("Trace: " + m);
            }
            );
            MonitorOfServices.Start();
            Debug.WriteLine("Monitor Ready.");
        }
        void DisposeMonitorOn(int seg)
        {
            Task.Delay(seg*1000).Wait();
            MonitorOfServices.Dispose();
            Debug.WriteLine("Monitor Disposed.");
        }
        void Wait(int seg)
        {
            Task.Delay(seg * 1000).Wait();
        }
        void MonitorDataSave()
        {
            lock (MonitorOfServices)
            {
                MonitorOfServices.DataGetForWebServer((csi) =>
                {
                    File.WriteAllText("WebDataMonitor.json",
                        JsonConvert.SerializeObject(csi, Formatting.Indented));
                });
            }
        }

        [TestMethod]
        public void StartingAndStoppingTest()
        {
            PrepareMonitor();
            DisposeMonitorOn(20);

            PrepareMonitor();
            DisposeMonitorOn(50);
        }

        [TestMethod]
        public void ReceivingData()
        {
            PrepareMonitor();

            Wait(2);
            MonitorOfServices.SetRadioMasterForTesting("10.12.60.35");
            Wait(20);

            MonitorOfServices.GetRadioData((data) =>
            {
                File.WriteAllText("RadioData.json",
                    JsonConvert.SerializeObject(data, Formatting.Indented));
            });
            DisposeMonitorOn(1);
        }
    }
}
