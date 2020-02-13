using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using Utilities;

namespace UnitTesting
{
    [TestClass]
    public class GenericTests
    {
        [TestMethod]
        public void HttpClientTests()
        {               
            Debug.WriteLine($"{DateTime.Now.ToLongTimeString()}: Test START");

            HttpHelper.GetSync("http://192.168.1.121/pepe", TimeSpan.FromSeconds(5), (succes, data) =>
            {
                Debug.WriteLine($"{DateTime.Now.ToLongTimeString()}: GetSync. Res {succes}, data: {data}");
            });

            HttpHelper.GetSync("http://192.168.0.212:1234/pepe", TimeSpan.FromSeconds(5), (succes, data) =>
            {
                Debug.WriteLine($"{DateTime.Now.ToLongTimeString()}: GetSync. Res {succes}, data: {data}");
            });

            HttpHelper.GetSync("http://192.168.0.212/pepe", TimeSpan.FromSeconds(5), (succes, data) =>
            {
                Debug.WriteLine($"{DateTime.Now.ToLongTimeString()}: GetSync. Res {succes}, data: {data}");
            });

            HttpHelper.GetSync("http://192.168.0.50:8080/test", TimeSpan.FromSeconds(5), (succes, data) =>
            {
                Debug.WriteLine($"{DateTime.Now.ToLongTimeString()}: GetSync. Res {succes}, data: {data}");
            });

            HttpHelper.GetSync("http://192.168.0.223:8080/test", TimeSpan.FromSeconds(5), (succes, data) =>
            {
                Debug.WriteLine($"{DateTime.Now.ToLongTimeString()}: GetSync. Res {succes}, data: {data}");
            });

            Debug.WriteLine($"{DateTime.Now.ToLongTimeString()}: Test END");
        }
    }
}
