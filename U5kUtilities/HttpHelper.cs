using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.Http;


namespace Utilities
{
    public static class HttpRequestExtensions
    {
        private static string TimeoutPropertyKey = "RequestTimeout";

        public static void SetTimeout(this HttpRequestMessage request, TimeSpan? timeout)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            request.Properties[TimeoutPropertyKey] = timeout;
        }

        public static TimeSpan? GetTimeout(this HttpRequestMessage request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Properties.TryGetValue(TimeoutPropertyKey, out var value) && value is TimeSpan timeout)
                return timeout;
            return null;
        }
    }

    public class TimeoutHandler : DelegatingHandler
    {
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(100);

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (var cts = GetCancellationTokenSource(request, cancellationToken))
            {
                try
                {
                    return await base.SendAsync(request, cts?.Token ?? cancellationToken);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException();
                }
            }
        }

        private CancellationTokenSource GetCancellationTokenSource(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var timeout = request.GetTimeout() ?? DefaultTimeout;
            if (timeout == Timeout.InfiniteTimeSpan)
            {
                // No need to create a CTS if there's no timeout
                return null;
            }
            else
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeout);
                return cts;
            }
        }
    }

    public class HttpHelper
    {
        public static string URL(string ip, string port, string localpath)
        {
            return "http://" + ip + ":" + port + localpath;
        }
        public static string GetSync(string ip, string port, string localpath, TimeSpan timeout, string defaultReturn="{}")
        {
            string ret = defaultReturn;
            GetSync(URL(ip, port, localpath), timeout, (success, data) =>
            {
                if (success)
                    ret = data;
            }, defaultReturn);
            return ret;
        }
        public static void GetSync(string url, TimeSpan timeout, Action<bool, string> Notify, string defaultReturn = "{}")
        {
            HttpWebRequest request = WebRequest.CreateHttp(url);
            request.Timeout = (int)timeout.TotalMilliseconds;
            request.Method = "GET";
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    // Do something with the response
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            // Get a reader capable of reading the response stream
                            using (StreamReader myStreamReader = new StreamReader(responseStream, Encoding.UTF8))
                            {
                                // Read stream content as string
                                string strData = myStreamReader.ReadToEnd();
                                Notify?.Invoke(true, strData);
                            }
                        }
                    }
                    else
                    {
                        var msg = $"Error: {response.StatusCode}";
                        Notify?.Invoke(false, msg);
                    }
                }
            }
            catch (Exception x)
            {
                Notify?.Invoke(false, x.Message);
            }
        }

        //public static string GetAsync(string url, string defaultReturn="{}")
        //{
        //    try
        //    {
        //        // ... Use HttpClient.
        //        using (var client = new HttpClient())
        //        using (var result = client.GetAsync(url).Result)
        //        using (var content = result.IsSuccessStatusCode ? result.Content : null)
        //        {
        //            if (content != null)
        //            {
        //                // Respuestas 20x
        //                string data = content.ReadAsStringAsync().Result;
        //                return data;
        //            }
        //            else
        //            {
        //                // Otras Respuestas
        //            }
        //        }
        //    }
        //    catch (Exception )
        //    {
        //        // Timeouts...
        //    }
        //    return defaultReturn;
        //}
        //public static void GetAsync(string url, Action<bool, string> Notify, string defaultReturn = "{}")
        //{
        //    try
        //    {
        //        // ... Use HttpClient.
        //        using (var client = new HttpClient())
        //        using (var result = client.GetAsync(url).Result)
        //        using (var content = result.IsSuccessStatusCode ? result.Content : null)
        //        {
        //            if (content != null)
        //            {
        //                // Respuestas 20x
        //                Notify?.Invoke(true, content.ReadAsStringAsync().Result);
        //            }
        //            else
        //            {
        //                // Otras Respuestas
        //                Notify?.Invoke(false, $"{result.StatusCode}: {result.ReasonPhrase}");
        //            }
        //            return;
        //        }
        //    }
        //    catch (Exception x)
        //    {
        //        // Errores
        //        var msg = x.Message;
        //        if (x is AggregateException)
        //        {
        //            foreach(var excep in (x as AggregateException).InnerExceptions)
        //            {
        //                msg += ("\n\t" + excep.Message);
        //            }
        //        }
        //        Notify?.Invoke(false, msg);
        //    }
        //}

        public static async void GetAsync(string url, TimeSpan timeout, Action<bool, string> Notify, string defaultReturn = "{}")
        {
            try
            {
                var handler = new TimeoutHandler
                {
                    DefaultTimeout = timeout,
                    InnerHandler = new HttpClientHandler()
                };

                using (var cts = new CancellationTokenSource())
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = Timeout.InfiniteTimeSpan;

                    var request = new HttpRequestMessage(HttpMethod.Get, url);

                    // Uncomment to test per-request timeout
                    //request.SetTimeout(TimeSpan.FromSeconds(5));

                    // Uncomment to test that cancellation still works properly
                    //cts.CancelAfter(TimeSpan.FromSeconds(2));

                    using (var response = await client.SendAsync(request, cts.Token))
                    {
                        var content = response.Content;
                        if (response.IsSuccessStatusCode)
                        {
                            Notify?.Invoke(true, content.ReadAsStringAsync().Result);
                        }
                        else
                        {
                            Notify?.Invoke(false, $"{response.StatusCode}: {response.ReasonPhrase}");
                        }
                    }
                }

                // ... Use HttpClient.
                //using (var client = new HttpClient() { Timeout = timeout })
                //using (var result = client.GetAsync(url).Result)
                //using (var content = result.IsSuccessStatusCode ? result.Content : null)
                //{
                //    if (content != null)
                //    {
                //        // Respuestas 20x
                //        Notify?.Invoke(true, content.ReadAsStringAsync().Result);
                //    }
                //    else
                //    {
                //        // Otras Respuestas
                //        Notify?.Invoke(false, $"{result.StatusCode}: {result.ReasonPhrase}");
                //    }
                //    return;
                //}
            }
            catch (Exception x)
            {
                // Errores
                var msg = x.Message;
                if (x is AggregateException)
                {
                    foreach (var excep in (x as AggregateException).InnerExceptions)
                    {
                        msg += ("\n\t" + excep.Message);
                    }
                }
                Notify?.Invoke(false, msg);
            }
        }

    }
}
