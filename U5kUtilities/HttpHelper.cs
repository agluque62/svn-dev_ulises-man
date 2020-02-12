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
        public static string Get(string ip, string port, string localpath, TimeSpan timeout, string defaultReturn="{}")
        {
            string url = "http://" + ip + ":" + port + localpath;
            return Get(url, timeout, defaultReturn);
        }
        public static string Get(string url, TimeSpan timeout, string defaultReturn="{}")
        {
            try
            {
                // ... Use HttpClient.
                using (var client = new HttpClient() { Timeout = timeout })
                using (var result = client.GetAsync(url).Result)
                using (var content = result.IsSuccessStatusCode ? result.Content : null)
                {
                    if (content != null)
                    {
                        // Respuestas 20x
                        string data = content.ReadAsStringAsync().Result;
                        return data;
                    }
                    else
                    {
                        // Otras Respuestas
                    }
                }
            }
            catch (Exception )
            {
                // Timeouts...
            }
            return defaultReturn;
        }
        public static void Get(string url, TimeSpan timeout, Action<bool, string> Notify, string defaultReturn = "{}")
        {
            try
            {
                // ... Use HttpClient.
                using (var client = new HttpClient() { Timeout = timeout })
                using (var result = client.GetAsync(url).Result)
                using (var content = result.IsSuccessStatusCode ? result.Content : null)
                {
                    if (content != null)
                    {
                        // Respuestas 20x
                        Notify?.Invoke(true, content.ReadAsStringAsync().Result);
                    }
                    else
                    {
                        // Otras Respuestas
                        Notify?.Invoke(false, $"{result.StatusCode}: {result.ReasonPhrase}");
                    }
                    return;
                }
            }
            catch (Exception)
            {                // Timeouts...
               Notify?.Invoke(false, defaultReturn);
            }
        }
    }
}
