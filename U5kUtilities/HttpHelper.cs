using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;


namespace Utilities
{
    public class HttpHelper
    {
        public static string Get(string ip, string port, string localpath, string defaultReturn="{}")
        {
            try
            {
                string url = "http://" + ip + ":" + port + localpath;

                // ... Use HttpClient.
                using (var client = new HttpClient())
                using (var result = client.GetAsync(url).Result)
                using (var content = result.IsSuccessStatusCode ? result.Content : null)
                {
                    // ... Read the string.
                    if (content != null)
                    {
                        string data = content.ReadAsStringAsync().Result;
                        return data;
                    }
                }
            }
            catch (Exception )
            {
            }
            return defaultReturn;
        }
    }
}
