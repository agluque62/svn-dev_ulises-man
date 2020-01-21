using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Xml;
using System.Xml.Serialization;
using System.Management;

// using NLog;
using Newtonsoft.Json;

namespace Utilities
{
    public class NicEventMonitor : IDisposable
    {
        /// <summary>
        /// Log this class
        /// </summary>
        // private static Logger _Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 
        /// </summary>
        public enum LanStatus { Unknown = 0, Up = 1, Down = 2 }
        ///// <summary>
        ///// 
        ///// </summary>
        //public enum TeamType { Marvell, Intel, Unknown }
        /// <summary>
        /// 
        /// </summary>
        public event Action<int, LanStatus> StatusChanged = null;            // Lan 0/1, Estado LanStatus
        public event Action<String, Exception> MessageError = null;

        /// <summary>
        /// 
        /// </summary>
        public class NicEventMonitorConfig
        {
            /// <summary>
            /// Log this class
            /// </summary>
            public string TeamingType { get; set; }
            public string WindowsLog { get; set; }
            public string EventSource { get; set; }
            public int [] UpEventId { get; set; }
            public int [] DownEventId { get; set; }
            public int PropertyIndex { get; set; }

            public NicEventMonitorConfig()
            {
                TeamingType = "Intel";
                WindowsLog = "System";
                EventSource = "iANSMiniport";
                UpEventId = new int[] { 15 };
                DownEventId = new int[] { 11 };
                PropertyIndex = 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cfg"></param>
        public NicEventMonitor(NicEventMonitorConfig cfg,
            Action<int, LanStatus> change,
            Action<String, Exception> message,
            string filepath = "")
        {
            try
            {
                StatusChanged = change;
                MessageError = message;
                Cfg = cfg;
                FilePath = filepath;
                Init();
            }
            catch (Exception x)
            {
                RaiseMessageError(x);
            }
            Cfg = cfg;
            FilePath = filepath;
        }

        public NicEventMonitor(string jconfig,
            Action<int, LanStatus> change,
            Action<String, Exception> message,
            string filepath = "")
        {
            try
            {
                StatusChanged = change;
                MessageError = message;
                Cfg = JsonConvert.DeserializeObject<NicEventMonitorConfig>(jconfig);
                FilePath = filepath;
                Init();
            }
            catch (Exception x)
            {
                RaiseMessageError(x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            try
            {
                watcher.Enabled = false;
                watcher.Dispose();
                NICList.Clear();
            }
            catch (Exception x)
            {
                RaiseMessageError(x);
            }
        }

        public bool GetState(Action<string, LanStatus> cb)
        {
            if(NICList.Count > 0)
            {
                NICList.ForEach((nic) =>
                {
                    cb(nic.DeviceId, nic.Status);
                });
                return true;
            }
            return false;
        }

        public static void SimulateSystemWriteEvent(string source, string idLan, int codeevent)
        {
#if DEBUG

            try
            {
                using (EventLog eventLog = new EventLog("System", Environment.MachineName, source))
                {
                    EventInstance eventInstance = new EventInstance(codeevent, 1, EventLogEntryType.Information);
                    object[] prop = new object[] 
                    {
                        idLan, idLan
                    };
                    eventLog.WriteEvent(eventInstance, prop);
                }
            }
            catch (Exception x)
            {
                Debug.WriteLine("Excepcion: " + x.Message);
                
            }
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        protected void Init()
        {
            LogEntriesGet();
            SearchForPhysicalDevices();
            InitialStatusGet();
            if (FilePath == "")
            {
                EventLogQuery logquery = FilePath == "" ? new EventLogQuery(Cfg.WindowsLog, PathType.LogName) : new EventLogQuery(FilePath, PathType.FilePath);
                watcher = new EventLogWatcher(logquery);
                watcher.EventRecordWritten += new EventHandler<EventRecordWrittenEventArgs>(watcher_EventRecordWritten);
                watcher.Enabled = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void SearchForPhysicalDevices()
        {
            try
            {
                NICList.Clear();
                foreach (EventRecord evento in _LogEntries)
                {
                    if (evento.Properties.Count > 0)
                    {
                        string idDevice = evento.Properties.Count > Cfg.PropertyIndex ? evento.Properties[Cfg.PropertyIndex].Value.ToString() : "Unknow";
#if DEBUG1
                        if (NICList.Contains(new NICItem() { DeviceId = idDevice }) == false)
#else
                        if (NICList.Count < 2 && NICList.Contains(new NICItem() { DeviceId = idDevice }) == false)
#endif
                        {
                            NICList.Add(new NICItem() { DeviceId = idDevice, Status = LanStatus.Down/*, Index = Index++*/ });
                        }
                    }
                }

                int Index = 0;
                NICList = NICList.OrderBy(lan => lan.DeviceId).ToList();
                NICList.ForEach(i => { i.Index = Index++; });
            }
            catch (Exception x)
            {
                RaiseMessageError(x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        protected string DeviceGetFromXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlNode node = doc.DocumentElement.SelectSingleNode(".EventData");
            return node != null ? node.InnerText : "No encontrado";
        }

        /// <summary>
        /// 
        /// </summary>
        protected void InitialStatusGet()
        {
            try
            {
                /** Leer la lista de eventos */
                for (int lan = 0; lan < NICList.Count; lan++)
                {
                    string LanDevice = NICList[lan].DeviceId;

                    //List<EventRecord> last_lan_ev_down = _LogEntries.Where(e => Cfg.DownEventId == (e.Id) && e.ToXml().Contains(LanDevice)).OrderByDescending(e => e.TimeCreated).ToList();
                    //List<EventRecord> last_lan_ev_up = _LogEntries.Where(e => Cfg.UpEventId == (e.Id) && e.ToXml().Contains(LanDevice)).OrderByDescending(e => e.TimeCreated).ToList();

                    List<EventRecord> last_lan_ev_down = _LogEntries.Where(e => Cfg.DownEventId.Contains(e.Id) && e.ToXml().Contains(LanDevice)).OrderByDescending(e => e.TimeCreated).ToList();
                    List<EventRecord> last_lan_ev_up = _LogEntries.Where(e => Cfg.UpEventId.Contains(e.Id) && e.ToXml().Contains(LanDevice)).OrderByDescending(e => e.TimeCreated).ToList();

                    long last_lan_down = last_lan_ev_down.Count == 0 ? DateTime.MinValue.Ticks : last_lan_ev_down[0].TimeCreated.Value.Ticks;
                    long last_lan_up = last_lan_ev_up.Count == 0 ? DateTime.MinValue.Ticks : last_lan_ev_up[0].TimeCreated.Value.Ticks;

                    NICList[lan].Status = last_lan_up > last_lan_down ? LanStatus.Up : LanStatus.Down;
                    RaiseStatusChanged(lan);
                }
            }
            catch (Exception x)
            {
                RaiseMessageError(x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void LogEntriesGet()
        {
            //string evtQuery = String.Format("*[System[(Provider/@Name=\"{0}\") and ((EventID={1}) or (EventID={2}) )]]", Cfg.EventSource, Cfg.DownEventId, Cfg.UpEventId);
            string evtQuery = String.Format("*[System[(Provider/@Name=\"{0}\")]]", Cfg.EventSource);
            EventLogQuery logquery = FilePath == "" ? new EventLogQuery(Cfg.WindowsLog, PathType.LogName, evtQuery) :
                new EventLogQuery(FilePath, PathType.FilePath, evtQuery);

            EventLogReader elr = new EventLogReader(logquery);

            EventRecord entry;
            _LogEntries.Clear();
            while ((entry = elr.ReadEvent()) != null)
            {
                if (Cfg.UpEventId.Contains(entry.Id)||Cfg.DownEventId.Contains(entry.Id))
                    _LogEntries.Add(entry);
            }
            _LogEntries = _LogEntries.OrderByDescending(e => e.TimeCreated).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void watcher_EventRecordWritten(object sender, EventRecordWrittenEventArgs e)
        {
            if (e.EventRecord.LogName == Cfg.WindowsLog && e.EventRecord.ProviderName == Cfg.EventSource &&
                //( e.EventRecord.Id == Cfg.UpEventId || e.EventRecord.Id == Cfg.DownEventId))
                (Cfg.UpEventId.Contains(e.EventRecord.Id) || Cfg.DownEventId.Contains(e.EventRecord.Id)))
            {
                lock (NICList)
                {
                    // 20181106. Configurar los eventos e ID de las LAN's
                    if (e.EventRecord.Properties.Count > Cfg.PropertyIndex)
                    {
                        string idLan = e.EventRecord.Properties[Cfg.PropertyIndex/*0*/].Value.ToString();
                        NICItem lan = NICList.Where(nic => nic.DeviceId == idLan).FirstOrDefault();
                        if (lan != null)
                        {
                            //lan.Status = e.EventRecord.Id == Cfg.UpEventId ? LanStatus.Up : LanStatus.Down;
                            lan.Status = Cfg.UpEventId.Contains(e.EventRecord.Id) ? LanStatus.Up : LanStatus.Down;
                            RaiseStatusChanged(lan.Index);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mensaje"></param>
        private void RaiseMessageError(Exception x,
        [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
        [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
        [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            MessageError?.Invoke(String.Format("[{0},{1},{2}]: {3}",
                    System.IO.Path.GetFileName(sourceFilePath), sourceLineNumber,x.Message, memberName), x);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lan"></param>
        private void RaiseStatusChanged(int lan)
        {
            if (lan < NICList.Count)
            {
#if DEBUG
                Debug.WriteLine("Notificando evento de LAN.", NICList[lan].DeviceId);
#endif
                StatusChanged?.Invoke(lan, NICList[lan].Status);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string FilePath { get; set; }
        private NicEventMonitorConfig Cfg { get; set; }
        private List<EventRecord> _LogEntries = new List<EventRecord>();
        private class NICItem : IEquatable<NICItem>
        {
            public int Index { get; set; }
            public string DeviceId { get; set; }
            public LanStatus Status { get; set; }
            public bool Equals(NICItem other)
            {
                return other.DeviceId == DeviceId;
            }
        }
        private List<NICItem> NICList = new List<NICItem>();
        private EventLogWatcher watcher = null;
    }
}
