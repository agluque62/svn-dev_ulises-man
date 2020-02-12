using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Utilities
{
    class SimpleSipAgent
    {
    }
    /// <summary>
    /// 
    /// </summary>
    public class SipSupervisor : IDisposable
    {
        public event Action<SipUA, Exception> NotifyException=null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="UA"></param>
        /// <param name="Timeout"></param>
        public SipSupervisor(SipUA UA, int Timeout = 500)
        {
            local_ua = UA;
            timeout = Timeout;
            cseq = 1;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            NotifyException = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="remote_ua"></param>
        /// <returns></returns>
        public bool SipPing(SipUA remote_ua)
        {
            using (UdpClient udpClient = new UdpClient(/*local_ua.port ?? 5060*/0))    
            {
                // Actualizo el puerto local al seleccionado por la pila....
                SipUA working_local_ua = new SipUA(local_ua);
                working_local_ua.port = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;

                udpClient.Client.ReceiveTimeout = timeout;

                var to = new IPEndPoint(IPAddress.Parse(remote_ua.ip), remote_ua.port ?? 5060);
                byte[] data = Encoding.ASCII.GetBytes(
                    (new SipOptionsRequest()
                    {
                        from = working_local_ua,
                        to = remote_ua,
                        CSeq = cseq.ToString("00000000")
                    }).Message);
                cseq++;
                try
                {
                    IPEndPoint remoteEP = null;
                    udpClient.Client.ReceiveTimeout = timeout;

                    while (udpClient.Available > 0)
                        udpClient.Receive(ref remoteEP);

                    udpClient.Send(data, data.Length, to);
                    byte[] receivedData = udpClient.Receive(ref remoteEP);
                    remote_ua.last_response = new SipResponse(receivedData);
                    return true;
                }
                catch (SocketException x)
                {
                    NotifyException?.Invoke(remote_ua, x);
                    remote_ua.last_response = new SipResponse(null);
                    return x.SocketErrorCode == SocketError.TimedOut ? false : true;
                }
                catch (Exception x)
                {
                    NotifyException?.Invoke(remote_ua, x);
                    remote_ua.last_response = new SipResponse(null);
                }
            }
            return true;
        }

        private SipUA local_ua = null;
        //private UdpClient udpClient = null;
        private int timeout;
        private Decimal cseq;
    }
    /// <summary>
    /// 
    /// </summary>
    public class SipOptionsRequest 
    {
        public SipUA from { get; set; }
        public SipUA to { get; set; }
        public string CSeq { get; set; }

        public string Message
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(String.Format("OPTIONS {0} SIP/2.0", to.uri));
                sb.AppendLine(String.Format("Via: SIP/2.0/UDP {0};branch={1}", from.via, Branch));
                sb.AppendLine(String.Format("Max-Forwards: 70"));
                sb.AppendLine(String.Format("To: <{0}>", to.uri));
                sb.AppendLine(String.Format("From: <{0}>;tag={1}", from.uri, Tag));
                sb.AppendLine(String.Format("Call-ID: {0}", CallId));
                sb.AppendLine(String.Format("CSeq: {0} OPTIONS", CSeq));
                sb.AppendLine(String.Format("Contact: <{0}>", from.uri));
                sb.AppendLine(String.Format("User-Agent: {0}", user_agent));
                sb.AppendLine(String.Format("WG67-Version: {0}", wg67_version));
                sb.AppendLine(String.Format("Accept: application/sdp"));
                sb.AppendLine(String.Format("Content-Length: 0"));
                sb.AppendLine();
                return sb.ToString();
            }
        }

        protected string Branch
        {
            get
            {
#if _FIXED_
                return "z9hG4bKPj70c19aaffa4f49008dc4d5aa7cee7aad";
#else
                // The value of the branch parameter MUST start with the magic cookie "z9hG4bK".
                return "z9hG4bK-" + Guid.NewGuid().ToString().Replace("-", "");
#endif
            }
        }

        protected string Tag
        {
            get
            {
#if _FIXED_
                return "1928301774";
#else
                // Random string
                return Guid.NewGuid().ToString().Replace("-", "");
#endif
            }
        }

        protected string CallId
        {
            get
            {
#if _FIXED_
                return "a84b4c76e66710";
#else
                // Random string
                return Guid.NewGuid().ToString().Replace("-", "");
#endif
            }
        }

        protected string user_agent
        {
            get
            {
                return "uv5ki-man/2.5.x";
            }
        }

        protected string wg67_version
        {
            get
            {
                return to.radio == null || to.radio == false ? "phone.01" : "radio.01";
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class SipResponse 
    {
        public string Result { get; set; }
        public string Text { get; set; }

        public SipResponse(byte[] data)
        {
            Text = data == null ? "" : Encoding.Default.GetString(data);
            using (StringReader reader = new StringReader(Text))
            {
                string line;
                line = reader.ReadLine();                   // Primera Linea.
                if (line != null)
                {
                    string[] campos = line.Split(' ');
                    if (campos.Length > 2 && campos[0].ToUpper() == "SIP/2.0")
                    {
                        Result = campos[1];
                    }
                    else
                    {
                        Result = "Error";
                    }
                }
                else
                {
                    Result = "Error";
                }

                // Resto de Líneas.
                while ((line = reader.ReadLine()) != null)
                {
                    // Do something with the line
                }
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class SipUA 
    {
        public string user { get; set; }
        public string ip { get; set; }
        public int? port { get; set; }
        public bool? radio { get; set; }

        public string uri
        {
            get
            {
                return "sip:" + user + "@" + (port == null ? ip : (ip + ":" + port.ToString()));
            }
        }

        public string via
        {
            get
            {
                return ip + (port == null ? "" : ":" + port.ToString());
            }
        }

        public SipResponse last_response { get; set; }

        public SipUA() { }

        public SipUA(SipUA o)
        {
            user = o.user;
            ip = o.ip;
            port = o.port;
            radio = o.radio;
        }

    }
}
