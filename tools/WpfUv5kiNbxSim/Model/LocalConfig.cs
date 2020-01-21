using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WpfUv5kiNbxSim.Model
{
    class LocalConfig : IDisposable
    {
        const string FileName = "config.json";
        public class JSonNbxConfig
        {
            public string ip { get; set; }
            public int wp { get; set; }
        }
        public class JSonConfig
        {
            public string serverIP { get; set; }
            public int serverPort { get; set; }
            public List<JSonNbxConfig> nbxs { get; set; }
        }

        public LocalConfig(bool designTime=false)
        {
            if (!designTime)
            {
                Config = JsonConvert.DeserializeObject<JSonConfig>(File.ReadAllText(FileName));
            }
            else
            {
                Config = new JSonConfig()
                {
                    serverIP = "192.168.0.212",
                    serverPort = 1024,
                    nbxs = new List<JSonNbxConfig>()
                    {
                        new JSonNbxConfig(){ ip="192.168.0.60", wp=8000},
                        new JSonNbxConfig(){ ip="192.168.0.61", wp=8001},
                    }
                };
            }
        }

        public void Dispose()
        {
            if (Config != null) 
                File.WriteAllText(FileName, JsonConvert.SerializeObject(Config, Formatting.Indented));
        }

        public JSonConfig Config { get; set; }
    }
}
