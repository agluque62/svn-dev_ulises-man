using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Win32;

namespace Utilities
{
    public class NtpClientStatus : IDisposable
    {
        #region Public
        protected enum NtpClient { Windows = 0, Meinberg = 1, Unknow = -1 }
        public NtpClientStatus(int client = 0)
        {
            Cliente = client == 0 ? NtpClient.Windows : client == 1 ? NtpClient.Meinberg : NtpClient.Unknow;
        }
        public void Dispose()
        {
        }
        public List<string> Status
        {
            get
            {
                try
                {
                    switch (Cliente)
                    {
                        case NtpClient.Windows:
                            return W32tmStatus;
                        case NtpClient.Meinberg:
                            return MeinbergNtpClientStatus;
                        default:
                            return ErrorStatus("Not implemented");
                    }
                }
                catch (Exception x)
                {
                    return ErrorStatus(x.Message);
                }
            }
        }
        public string UrlServer(string client_response = "")
        {
            switch (Cliente)
            {
                case NtpClient.Windows:
                    return WindowsNtpServerUrl;
                case NtpClient.Meinberg:
                    return MeinbergNtpServerUrl(client_response);
                default:
                    return String.Format("Ntp Client {0} unknown", (int)Cliente);
            }
        }

        #endregion

        #region Private
        private NtpClient Cliente { get; set; }
        private List<string> W32tmStatus
        {
            get
            {
                List<string> status = new List<string>();
                ProcessStartInfo psi = new ProcessStartInfo("w32tm", " /query /peers")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                var proc = Process.Start(psi);
                string Text = proc.StandardOutput.ReadToEnd();

                using (StringReader reader = new StringReader(Text))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line != String.Empty)
                            status.Add(Normalizar(line));
                    }
                }

                return status;
            }
        }
        private List<string> MeinbergNtpClientStatus
        {
            get
            {
                List<string> status = new List<string>();
                try
                {
                    string Text = ExecuteMeinberCommand();

                    using (StringReader reader = new StringReader(Text))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line != String.Empty)
                                status.Add(Normalizar(line));
                        }
                    }
                }
                catch (Exception x)
                {
                    return ErrorStatus(x.Message);
                }

                return status;
            }
        }
        private String WindowsNtpServerUrl
        {
            get
            {
                RegistryKey pRegKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\W32Time\Parameters");
                Object obj = pRegKey.GetValue("NtpServer");
                String[] ntpSrvs = ((string)obj).Split(',');
                return ntpSrvs[0];
            }
        }
        private String MeinbergNtpServerUrl(string client_response)
        {
            try
            {
                string Text = client_response == "" ? ExecuteMeinberCommand() : client_response;
                List<string> Servidores = new List<string>();

                using (StringReader reader = new StringReader(Text))
                {
                    string line;
                    int nline = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (nline > 1 && line != String.Empty)
                        {
                            bool active = line[0] == '*';
                            string server = line.Substring(1).Split(new char[] { ' ' }).ToList().ElementAt(0);
                            Servidores.Add((active ? "a" : "i") + server);
                        }
                        nline++;
                    }
                }
                Servidores = Servidores.OrderBy(item => item).ToList();
                return Servidores.Count == 0 ? "No Meinberg Server" : Servidores.ElementAt(0).Substring(1);

            }
            catch (Exception )
            {
                return String.Format("No Meinberg Client");
            }
        }
        private List<string> ErrorStatus(string error)
        {
            List<string> status = new List<string>()
                {
                    String.Format("ntpc {1} Error: {0}", error, Cliente) 
                };
            return status;
        }
        private String Normalizar(String inputString)
        {
            // var inputString = "Mañana será otro día";
            var normalizedString = inputString.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            for (int i = 0; i < normalizedString.Length; i++)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(normalizedString[i]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(normalizedString[i]);
                }
            }
            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }
        private String ExecuteMeinberCommand()
        {
#if DEBUG
            string Text = "     remote           refid      st t when poll reach   delay   offset  jitter\r\n"
                          + "==============================================================================\r\n"
                          + " 192.168.0.129 ( 172.24.90.12     6 u  234  256  377    0.977   82.219  26.097\r\n"
                          + "*DF1501  ( 172.24.90.12     6 u  234  256  377    0.977   82.219  26.097\r\n";
            return Text;
#else
            ProcessStartInfo psi = new ProcessStartInfo("ntpq", " -p")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var proc = Process.Start(psi);
            return proc.StandardOutput.ReadToEnd();
#endif
        }
        #endregion
    }
}
