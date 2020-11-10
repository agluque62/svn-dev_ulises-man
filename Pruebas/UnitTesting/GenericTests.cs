using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

using U5kBaseDatos;
using U5kManServer;
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

        [TestMethod]
        public void HttpPostTests()
        {
            Debug.WriteLine($"{DateTime.Now.ToLongTimeString()}: Test START");

            HttpHelper.PostSync(HttpHelper.URL("10.12.60.130","1023","/rd11"), new { id = "test " }, TimeSpan.FromSeconds(5), (success, data) =>
            {            
                Debug.WriteLine($"{DateTime.Now.ToLongTimeString()}: PostSync. Res {success}, data: {data}");
            });

            HttpHelper.PostSync(HttpHelper.URL("10.12.60.130", "1023", "/rdhf"), new { id = "test " }, TimeSpan.FromSeconds(5), (success, data) =>
            {
                Debug.WriteLine($"{DateTime.Now.ToLongTimeString()}: PostSync. Res {success}, data: {data}");
            });

            HttpHelper.PostSync(HttpHelper.URL("10.12.60.130", "1023", "/rdhfhf"), new { id = "test " }, TimeSpan.FromSeconds(5), (success, data) =>
            {
                Debug.WriteLine($"{DateTime.Now.ToLongTimeString()}: PostSync. Res {success}, data: {data}");
            });

            Debug.WriteLine($"{DateTime.Now.ToLongTimeString()}: Test END");
        }

        [TestMethod]
        public void InciFilterTest()
        {
            Debug.WriteLine("Starting Test");
            var t1 = Task.Factory.StartNew(() =>
            {
                var inci = new U5kIncidencia()
                {
                    id = 1000,
                    idhw = "IDW1",
                    desc = "Hola que tal"
                };
                ToDo("TASK-1", inci, TimeSpan.FromSeconds(3), TimeSpan.FromMinutes(45));
            });

            var t2 = Task.Factory.StartNew(() =>
            {
                var inci = new U5kIncidencia()
                {
                    id = 1001,
                    idhw = "IDW2",
                    desc = "Hola que tal"
                };
                ToDo("TASK-2", inci, TimeSpan.FromSeconds(4), TimeSpan.FromMinutes(45));
            });

            Task.WaitAll(new Task[] { t1, t2 });
            Debug.WriteLine($"End Test. Almacenadas {Almacenadas.Count}");
        
        }

        Queue<U5kIncidencia> Almacenadas = new Queue<U5kIncidencia>();
        HistThread.StoreFilterControl filter = new HistThread.StoreFilterControl();
        public void ToDo(string id, U5kIncidencia inci, TimeSpan periodo, TimeSpan limite)
        {
            var startTest = DateTime.Now;
            do
            {
                inci.fecha = DateTime.Now;
                if (filter.ToStore(inci, 60) == true)
                {
                    Almacenadas.Enqueue(inci);
                }
                Debug.WriteLine($"{DateTime.Now}: {id}: Almacenadas {Almacenadas.Count}");
                Task.Delay(periodo).Wait();

            } while ((DateTime.Now - startTest) < limite);

        }
    }
}
