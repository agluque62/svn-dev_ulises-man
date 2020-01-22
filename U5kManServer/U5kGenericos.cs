using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using System.Diagnostics;
using System.Reflection;

using System.Net;
using System.Net.NetworkInformation;

using System.Threading;
using System.Globalization;

using System.Runtime.Serialization;
using System.Runtime.InteropServices;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace U5kManServer
{
    enum ClusterPollingMethods { Soap=0, DataBase=1, UdpRequest=2 }
    public class U5kGenericos
    {
        /// <summary>
        /// 
        /// </summary>
        public static string MiDireccionIP
        {
            get
            {
                string strHostName = "";
                strHostName = System.Net.Dns.GetHostName(); 
                System.Net.IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName); 
                System.Net.IPAddress[] addr = ipEntry.AddressList;
 
                return addr[addr.Length - 1].ToString(); 
            }
        }


        static PingReply Ping(string host)
        {
            PingReply reply;
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            // Use the default Ttl value which is 128, 
            // but change the fragmentation behavior.
            options.DontFragment = true;

            // Create a buffer of 32 bytes of data to be transmitted. 
            string data = "Ulises V 5000i. ManService......";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 200;  // ms
            reply = pingSender.Send(host, timeout, buffer, options);
            return reply;
        }
        public static bool Ping(string host, bool presente)
        {
            int maxReint = presente ? 3 : 1;
            int reint = 0;
            PingReply reply;
            do
            {
                reply = Ping(host);
                reint++;
                System.Threading.Thread.Sleep(10);
            } while (reply.Status != IPStatus.Success && reint < maxReint);
            return reply.Status == IPStatus.Success ? true : false;
        }
        public static void Ping(string host, bool presente, Action<bool, IPStatus[]> ResultDelivery)
        {
            int maxReint = presente ? 3 : 1;
            int reint = 0;
            PingReply reply;
            List<IPStatus> replies = new List<IPStatus>();
            do
            {
                reply = Ping(host);
                replies.Add(reply.Status);
                reint++;

                System.Threading.Thread.Sleep(10);
            } while (reply.Status != IPStatus.Success && reint < maxReint);

            ResultDelivery(reply.Status == IPStatus.Success ? true : false, replies.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Anterior"></param>
        /// <param name="Error"></param>
        /// <returns></returns>
        public static std AutomataEstadosGlobales(std Anterior, bool Error)
        {
            std NuevoEstado = std.NoInfo;

            switch (Anterior)
            {
                case std.NoInfo:
                    NuevoEstado = Error ? std.Error : std.Aviso;
                    break;
                case std.Aviso:
                    NuevoEstado = Error ? std.Error : std.Aviso;
                    break;
                case std.Error:
                    NuevoEstado = std.Error;
                    break;
                default:
                    NuevoEstado = std.Error;
                    break;
            }

            return NuevoEstado;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static void ReadFile(string path, ref string to)
        {
            try
            {
                StreamReader streamReader = new StreamReader(path);
                to = streamReader.ReadToEnd();
                streamReader.Close();
            }
            catch (Exception x)
            {
                NLog.LogManager.GetCurrentClassLogger().Trace(x, "ReadFile");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string Version
        {
            get
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
                return String.Format("{0}.{1}.{2}", fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string NodeboxUrl(string ip)
        {
            return String.Format(U5kManServer.Properties.u5kManServer.Default.strFormatNbxUrl, ip);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string PabxUrl(string ip)
        {
            return String.Format(U5kManServer.Properties.u5kManServer.Default.strFormatPabxUrl, ip);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strIn"></param>
        /// <returns></returns>
        static public string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"[^\w\.\ @-]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters, 
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static public bool ResetService { get; set; }

        /// <summary>
        /// 
        /// </summary>
        static public void CurrentCultureSet(Action<string> Trace)
        {
            if (U5kManService.cfgSettings != null)
            {
                string idioma = U5kManService.cfgSettings/* Properties.u5kManServer.Default*/.Idioma;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(idioma == "en" ? "en-US" : idioma == "fr" ? "fr-FR" : "es-ES");
#if DEBUG
                Trace(idioma);
#endif
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="NameOrIp"></param>
        /// <returns></returns>
        static public bool ValidateIp(string NameOrIp)
        {
            IPAddress tmp;
            bool isIp = IPAddress.TryParse(NameOrIp, out tmp);
            if (!isIp)
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(NameOrIp);
                return (hostEntry.AddressList.Length > 0) ? true : false;
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        static public bool IsLocalIp(string ip)
        {
            IPAddress IpChecking = null;
            if (IPAddress.TryParse(ip, out IpChecking))
            {
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (IPAddress localip in localIPs)
                {
                    if (localip.Equals(IpChecking)) return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        static public bool IsLocalIpExt(string ip)
        {
            IPAddress IpChecking = null;
            if (IPAddress.TryParse(ip, out IpChecking))
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var adapter in adapters)
                {
                    var properties = adapter.GetIPProperties();
                    //Console.WriteLine("{0} - {1}", adapter.Name, adapter.Description);
                    //Console.WriteLine(" -- {0}", adapter.OperationalStatus);
                    foreach (var address in properties.UnicastAddresses)
                    {
                        //Console.WriteLine(" -- {0}", address.Address);
                        if (address.Address == IpChecking) 
                            return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerialize"></param>
        /// <returns></returns>
        public static string SerializeContractObject<T>(T objectToSerialize)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(objectToSerialize.GetType());
                serializer.WriteObject(memoryStream, objectToSerialize);
                memoryStream.Position = 0;
                string retorno = reader.ReadToEnd();
#if DEBUG
                Console.WriteLine("StdGlobal {0} bytes", retorno.Count());
#endif
                return retorno;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strdata"></param>
        /// <param name="obj"></param>
        public static void DeserializaContractObject<T>(string strdata, ref T obj)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(strdata);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(obj.GetType());
                obj = (T)deserializer.ReadObject(stream);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="JObject"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        static public string JSerialize<JObject>(JObject obj)
        {
            return JsonConvert.SerializeObject(obj);
            //return "{}";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="JObject"></typeparam>
        /// <param name="strData"></param>
        /// <returns></returns>
        static public JObject JDeserialize<JObject>(string strData)
        {
            return JsonConvert.DeserializeObject<JObject>(strData);
            //return (JObject)new Object();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public static IPEndPoint SipEndPointFrom(string endpoint)
        {
            string[] parts = endpoint.Split(':');
            string ip = parts[0];
            string port = parts.Count() == 2 ? parts[1] : "5060";

            IPAddress Ip;
            int Port;
            if (IPAddress.TryParse(ip, out Ip) == false ||
                Int32.TryParse(port, out Port) == false)
                return null;

            return new IPEndPoint(Ip, Port);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int GetCurrentThreadId();
        static public void TraceCurrentThread(String Name, [System.Runtime.CompilerServices.CallerMemberName] string caller = null,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
#if DEBUG
            lock (pidHist)
            {
                // Thread.CurrentThread.Name = Name;
                int key = GetCurrentThreadId();
                Tuple<string, DateTime> val = new Tuple<string, DateTime>(String.Format("{2} [{0}:{1}]", caller, lineNumber, Name), DateTime.Now);
                pidHist[key] = val;

                /** Limpio los antiguos */
                var toRemove = (from item in pidHist
#if DEBUG
                                where item.Value.Item2 < DateTime.Now - TimeSpan.FromMinutes(3)
#else
                            where item.Value.Item2 < DateTime.Now - TimeSpan.FromHours(2)
#endif
                                select item).ToList();
                toRemove.ForEach(item =>
                {
                    pidHist.Remove(item.Key);
                });

                /** Salvo la tabla cada 10 minutos */
#if DEBUG
                if (lastSaveTime < DateTime.Now - TimeSpan.FromMinutes(1))
#else
            if (lastSaveTime < DateTime.Now - TimeSpan.FromMinutes(10))
#endif
                {
                    lastSaveTime = DateTime.Now;
                    NLog.LogManager.GetLogger("Thread").Trace("\n" + JsonConvert.SerializeObject(pidHist));
                }

                //NLog.LogManager.GetLogger("Thread").Trace("THREAD {4} [{0}:{1}] => TID: {2}, MTID: {3}", caller, lineNumber, 
                //    GetCurrentThreadId(),
                //    Thread.CurrentThread.ManagedThreadId,
                //    Name);
            }
#endif
        }
        static Dictionary<int, Tuple<string, DateTime>> pidHist = new Dictionary<int, Tuple<string, DateTime>>();
        static DateTime lastSaveTime = DateTime.MinValue;

    }

    public class GwMainStandbyCmd
    {
        [MarshalAs(UnmanagedType.I1)]
        char cmd = '2';
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        string name = "";
        [MarshalAs(UnmanagedType.I1)]
        char par1 = 'P';
        [MarshalAs(UnmanagedType.I1)]
        char par2 = 'R';
        
        public GwMainStandbyCmd(string Name)
        {
            name = Name;
        }
        public byte[] Data
        {
            get
            {
                return null;
            }
        }
    }
        
}
