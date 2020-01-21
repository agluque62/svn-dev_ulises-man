using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Net.Http;
using System.Dynamic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace U5kManServer
{
    public class PDSItem
    {
        public class PDSItemRes
        {
            public string id { get; set; }
            public string dep { get; set; }
            public int prio { get; set; }
            public int std { get; set; }
            public int tp { get; set; }
            public int ver { get; set; }
        }
        public string id { get; set; }
        public string ip { get; set; }
        public int tp { get; set; }
        public int ver { get; set; }
        public List<PDSItemRes> res {get;set;}
    }

    public class PresenceDataService : IDisposable
    {
        public bool IsStarted { get; set; }
        public int TickInSeconds { get; set; }
        public bool Testing { get; set; }
        public PresenceDataService()
        {
            IsStarted = false;
            Testing = false;
            TickInSeconds = 20;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            if (IsStarted == false)
            {
                if (Testing == true)
                {
                    U5kManService.GlobalData = new U5kManStdData();
                }

                _timer.Interval = 1000;
                _timer.AutoReset = false;
                _timer.Elapsed += OnTimeElapsed;
                _timer.Enabled = true;
                last_action = DateTime.MinValue;
                IsStarted = true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (IsStarted == true)
            {
                IsStarted = false;
                _timer.Enabled = false;
                _timer.Elapsed -= OnTimeElapsed;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        Timer _timer = new Timer();
        DateTime last_action;
        private void OnTimeElapsed(object sender, ElapsedEventArgs e)
        {
            U5kGenericos.TraceCurrentThread(this.GetType().Name + " OnTimeElapsed");
            _timer.Enabled = false;
            if (IsStarted)
            {
                if ((DateTime.Now - last_action) > TimeSpan.FromSeconds(TickInSeconds))
                {
                    GetDataFromPresenceServer();
                    RefreshDataOnService();
                    last_action = DateTime.Now;
                }
            }
            _timer.Enabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        PDSItem [] presenceServers;
        void GetDataFromPresenceServer()
        {
            try
            {
                string page = "http://192.168.0.212:1023/tifxinfo";

                // ... Use HttpClient.
                using (var client = new HttpClient())
                using (var result = client.GetAsync(page).Result)
                using (var content = result.IsSuccessStatusCode ? result.Content : null)
                {
                    // ... Read the string.
                    string data = content.ReadAsStringAsync().Result;
                    presenceServers = JsonConvert.DeserializeObject<PDSItem[]>(data);
                }
            }
            catch (Exception x)
            {
            }
        }
        /// <summary>
        /// 
        /// </summary>
        void RefreshDataOnService()
        {
            GlobalServices.GetWriteAccess((data) =>
            {
                List<Uv5kManDestinosPabx.DestinoPabx> pbxdes = data.STDPBXS;
                try
                {
                }
                catch (Exception x)
                {
                }
            });

        }
    }
}
