using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

using RestSharp;

using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Objects;
using Lextm.SharpSnmpLib.Pipeline;


namespace SnmpTest
{
    class GwText
    {
        public static string SlotType(int tp)
        {
            return tp == 0 ? "DES" : tp == 1 ? "IA4" : tp == 2 ? "IQ1" : tp.ToString() + "_?";
        }
        public static string SlotStatus(int st)
        {
            return st == 0 ? "DES" : st == 1 ? "CON" : st.ToString() + "_?";
        }
        public static string CanalStatus(int st)
        {
            return st == 0 ? "NPR" : st == 1 ? "PRE" : st.ToString() + "_?";
        }
        public static string ResType(int tp)
        {
            return tp == 0 ? "RD" : tp == 1 ? "LC" : tp == 2 ? "BC" : tp == 3 ? "BL" : tp == 4 ? "AB" : tp == 5 ? "R2" : tp == 6 ? "N5" : tp == 7 ? "QS" : tp == 9 ? "--" : tp.ToString() + "_?";
        }
        public static string ResSthw(int st)
        {
            return st == 0 ? "NP" : st == 1 ? "OK" : st == 2 ? "FA" : st.ToString() + "_?";
        }
        public static string ResStOpe(int st)
        {
            return st == 0 ? "NP" : st == 1 ? "OK" : st == 2 ? "FA" : st == 3 ? "DE" : st == -1 ? "--" : st.ToString() + "_?";
        }
    }

    class Program
    {
        static IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.0.41"), 161);
        static OctetString community = new OctetString("public");

        /** */
        static void Main(string[] args)
        {
            List<Variable> tarjetas = new List<Variable>();
            List<Variable> interfaces = new List<Variable>();
            IPAddress ip = IPAddress.Parse("192.168.0.41");

            int ntarjetas, ninterfaces;

            ConsoleKeyInfo result;
            do
            {
                result = Console.ReadKey(true);
                try
                {
                    switch (result.Key)
                    {
                        case ConsoleKey.A:
                            tarjetas.Clear();
                            ntarjetas = Messenger.Walk(VersionCode.V2, new IPEndPoint(ip, 161), new OctetString("public"),
                                new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.3.2"), tarjetas, 10000, WalkMode.WithinSubtree);
                            Console.WriteLine("Leidos {0} registros organizados en {1} filas", tarjetas.Count, ntarjetas);
                            break;
                        case ConsoleKey.B:
                            interfaces.Clear();
                            ninterfaces = Messenger.Walk(VersionCode.V2, new IPEndPoint(ip, 161), new OctetString("public"),
                                new ObjectIdentifier(".1.3.6.1.4.1.7916.8.3.1.4.2"), interfaces, 20000, WalkMode.WithinSubtree);
                            Console.WriteLine("Leidos {0} registros organizados en {1} filas", interfaces.Count, ninterfaces);
                            break;

                        case ConsoleKey.D1:
                            SnmpScanSlot(0);
                            break;
                        case ConsoleKey.D2:
                            SnmpScanSlot(1);
                            break;
                        case ConsoleKey.D3:
                            SnmpScanSlot(2);
                            break;
                        case ConsoleKey.D4:
                            SnmpScanSlot(3);
                            break;

                        case ConsoleKey.R:
                            RestSharpTest();
                            break;
                    }
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                }
            } while (result.Key != ConsoleKey.Escape);
        }

        /** */
        static void SnmpScanSlot(int nslot)
        {
            string oidbase = ".1.3.6.1.4.1.7916.8.3.1.3.2.1.";
            List<Variable> vIn = new List<Variable>()
            {
                new Variable(new ObjectIdentifier(oidbase+"2."+(nslot+1).ToString())),   // Tipo
                new Variable(new ObjectIdentifier(oidbase+"3."+(nslot+1).ToString())),   // Status,
                new Variable(new ObjectIdentifier(oidbase+"4."+(nslot+1).ToString())),   // Canal-0
                new Variable(new ObjectIdentifier(oidbase+"5."+(nslot+1).ToString())),   // Canal-1
                new Variable(new ObjectIdentifier(oidbase+"6."+(nslot+1).ToString())),   // Canal-2
                new Variable(new ObjectIdentifier(oidbase+"7."+(nslot+1).ToString()))    // Canal-3
            };
            IList<Variable > vOut = Messenger.Get(VersionCode.V2, ep, community, vIn, 2000);
            Console.WriteLine("Slot {0} Scanned: {1},{2},{3},{4},{5},{6}", nslot, 
                GwText.SlotType(((Integer32 )vOut[0].Data).ToInt32()), 
                GwText.SlotStatus(((Integer32)vOut[1].Data).ToInt32()), 
                GwText.CanalStatus(((Integer32)vOut[2].Data).ToInt32()), 
                GwText.CanalStatus(((Integer32)vOut[3].Data).ToInt32()), 
                GwText.CanalStatus(((Integer32)vOut[4].Data).ToInt32()), 
                GwText.CanalStatus(((Integer32)vOut[5].Data).ToInt32()));
            for (int canal = 0; canal < 4; canal++)
                SnmpScanResource(nslot, canal);
        }

        /** */
        static void SnmpScanResource(int nslot, int ncanal)
        {
            string oidbase = ".1.3.6.1.4.1.7916.8.3.1.4.2.1.";
            int nres = nslot * 4 + ncanal;
            List<Variable> vIn = new List<Variable>()
            {
                new Variable(new ObjectIdentifier(oidbase+"3."+(nres+1).ToString())),   // Tipo
                new Variable(new ObjectIdentifier(oidbase+"6."+(nres+1).ToString())),   // Status Hardware,
                new Variable(new ObjectIdentifier(oidbase+"15."+(nres+1).ToString())),  // Status Interfaz.
            };
            IList<Variable> vOut = Messenger.Get(VersionCode.V2, ep, community, vIn, 2000);
            Console.WriteLine("   Resource {0,2} Scanned: {1},{2},{3}", nres,
                GwText.ResType(((Integer32)vOut[0].Data).ToInt32()),
                GwText.ResSthw(((Integer32)vOut[1].Data).ToInt32()),
                GwText.ResStOpe(((Integer32)vOut[2].Data).ToInt32())
                );
        }

        /** */
        static void RestSharpTest()
        {
            var client = new RestClient("http://192.168.0.41:8080");
            // client.Authenticator = new HttpBasicAuthenticator(username, password);

            var request = new RestRequest("tses", Method.POST);
            //request.AddParameter("name", "value"); // adds to POST or URL querystring based on Method
            //request.AddUrlSegment("id", "123"); // replaces matching token in request.Resource

            // easily add HTTP Headers
            //request.AddHeader("header", "value");

            // add files to upload (works with compatible verbs)
            //request.AddFile(path);

            // execute the request
            IRestResponse response = client.Execute(request);
            var content = response.Content; // raw content as string

            // or automatically deserialize result
            // return content type is sniffed but can be explicitly set via RestClient.AddHandler();
            //RestResponse<Person> response2 = client.Execute<Person>(request);
            //var name = response2.Data.Name;

            // easy async support
            //client.ExecuteAsync(request, response => {
            //    Console.WriteLine(response.Content);
            //});

            // async with deserialization
            //var asyncHandle = client.ExecuteAsync<Person>(request, response => {
            //    Console.WriteLine(response.Data.Name);
            //});

            // abort the request on demand
            //asyncHandle.Abort();
        }
    }
}
