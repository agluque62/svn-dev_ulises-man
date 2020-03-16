using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace U5kSacta
{
    public class SactaConfig
    {
        public class SactaLan
        {
            public string ipmask { get; set; }
            public string mcast { get; set; }
            public int udpport { get; set; }
        }
        public class SactaSide
        {
            public Int32 Domain { get; set; }
            public Int32 Center { get; set; }
            public Int32 GrpUser { get; set; }
            public String SpiUsers { get; set; }
            public String SpvUsers { get; set; }
            public SactaLan lan1 { get; set; }
            public SactaLan lan2 { get; set; }
        }
        public class ScvSide
        {
            public Int32 Domain { get; set; }
            public Int32 Center { get; set; }
            public Int32 User { get; set; }
            public string Interfaz { get; set; }
            public int udpport { get; set; }
            public string Ignore { get; set; }
        }

        public Int32 Tick { get; set; }
        public Int32 TickPresencia { get; set; }
        public Int32 TimeoutPresencia { get; set; }
        public Int32 TimeoutActividadSacta { get; set; }
        public SactaSide sacta { get; set; }
        public ScvSide scv { get; set; }
        public static void GetConfig(Action< SactaConfig /*cfg*/, Exception> delivery)
        {
            SactaConfig cfg = null;
            Exception error = null;
            try
            {
                cfg = JsonConvert.DeserializeObject<SactaConfig>(File.ReadAllText(FileName));
            }
            catch(Exception x)
            {
                error = x;
                cfg = JsonConvert.DeserializeObject<SactaConfig>(SactaConfigDefault);
            }
            finally
            {
                delivery(cfg, error);
            }
        }
        public static void SetConfig(SactaConfig cfg, Action<Exception> delivery)
        {
            Exception error = null;
            try
            {
                File.WriteAllText(FileName, JsonConvert.SerializeObject(cfg));
            }
            catch(Exception x)
            {
                error = x;
            }
            finally
            {
                delivery(error);
            }
        }
        static string SactaConfigDefault
        {
            get
            {
                var sactaDefault = new
                {
                    Tick = 1000,
                    TickPresencia = 5000,
                    TimeoutPresencia = 30000,
                    TimeoutActividadSacta = 60000,
                    sacta = new
                    {
                        Domain = 1,
                        Center = 107,
                        GrpUser = 110,
                        SpiUsers = "111,112,113,114,7286,7287,7288,7289,15000",
                        SpvUsers = "86,87,88,89,7266,7267,7268,7269,34000",
                        lan1 = new
                        {
                            ipmask = "192.168.0.211",
                            mcast = "225.12.101.1",
                            udpport = 19204
                        },
                        lan2 = new
                        {
                            ipmask = "192.168.1.211",
                            mcast = "225.212.101.1",
                            udpport = 19204
                        }
                    },
                    scv = new
                    {
                        Domain = 1,
                        Center = 107,
                        User = 10,
                        Interfaz = "",
                        udpport = 15110,
                        Ignore = ""
                    }
                };
                return JsonConvert.SerializeObject(sactaDefault);
            }
        }
        const string FileName = "sacta-config.json";

    }
}
