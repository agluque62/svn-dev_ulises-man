using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using System.Runtime.Serialization;
using System.Xml;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

using U5kManServer;
using U5kBaseDatos;

namespace SerializeTest
{
    public enum trc { rcRadio = 2, rcLCE = 3, rcTLF = 4, rcATS = 5, rcNotipo = -1 }

    [DataContract]
    class TestMember
    {
        [DataMember]
        public string member { get; set; }
        [DataMember]
        public trc data = trc.rcATS;
    }

    [DataContract]
    class TestClass
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public TestMember member { get; set; }
        [DataMember]
        public Dictionary<int, string> dic = new Dictionary<int, string>() { { 0, "Cero" }, { 1, "Uno" } };

    }

    class Program
    {
        static U5kManStdData uv5ki_data = new U5kManStdData();

        static void Main(string[] args)
        {
            string data = WriteObject<TestClass>(new TestClass() { name = "Hola Mundo....", member = new TestMember() { member = "Miembro de..." } });

            LoadUv5kiData();
            data = WriteObject<U5kManStdData>(uv5ki_data);

            Console.WriteLine(data);
            Console.WriteLine("XML String {0} bytes...", data.Count());

            U5kManStdData leido = new U5kManStdData();
            ReadObject<U5kManStdData>(data, ref leido);
            Console.WriteLine("XML Leido = Original ?: {0}", leido.Equals(uv5ki_data));
            Console.ReadLine();

            data = JsonConvert.SerializeObject(uv5ki_data);
            Console.WriteLine(data);
            Console.WriteLine("JSON String {0} bytes...", data.Count());

            leido = JsonConvert.DeserializeObject<U5kManStdData>(data);
            Console.WriteLine("JSON Leido = Original ?: {0}", leido.Equals(uv5ki_data));

            Console.ReadLine();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerialize"></param>
        /// <returns></returns>
        public static string WriteObject<T>(T objectToSerialize)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(objectToSerialize.GetType());
                serializer.WriteObject(memoryStream, objectToSerialize);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strdata"></param>
        /// <param name="obj"></param>
        public static void ReadObject<T>(string strdata, ref T obj)
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

        public static void LoadUv5kiData()
        {
            U5kBdtService Database = null;
            try
            {
                Database = new U5kBdtService(CultureInfo.CurrentCulture, eBdt.bdtMySql, "192.168.0.212", "root");

                /** Operadores */
                List<BdtTop> tops = Database.GetListaTop("departamento");
                uv5ki_data.CFGTOPS = tops.Select(top => new stdPos() { name = top.Id, ip = top.Ip }).ToList();

                /** Pasarelas */
                List<BdtGw> lgw = Database.GetListaGw("departamento");
                uv5ki_data.CFGGWS = lgw.Select(gw => new stdGw(null)
                {
                    name = gw.Id,
                    ip = gw.Ip,
                    Dual = gw.Dual,
                    gwA = new stdPhGw()
                    {
                        name = gw.Id + "-A",
                        ip = gw.Ip1,
                        snmpport = gw.SnmpPortA
                    },
                    gwB = new stdPhGw()
                    {
                        name = gw.Id + "-B",
                        ip = gw.Ip2,
                        snmpport = gw.SnmpPortB
                    }
                }).ToList();

                /** */
                List<BdtEuEq> equipos = Database.ListaEquiposEurocae("departamento");
                /** Filtro de equipo valido */
                Func<BdtEuEq, bool> filtro = delegate(BdtEuEq equipo)
                {
                    /** Filtro el Equipo Correspondiente a la PABX */
                    //if (equipo.Ip == Properties.u5kManServer.Default.PabxIp ||
                    //    equipo.Ip2 == Properties.u5kManServer.Default.PabxIp)
                    //    return false;

                    /** Filtro Equipos No-Asignados (Sin SIP)*/
                    if (true)
                    {
                        if (equipo.Tipo != 5 && equipo.IdRecurso == null)
                            return false;
                    }
                    return true;
                };

                /** Configuro la lista de equipos */
                uv5ki_data.CFGEQS = equipos.Where(equipo => filtro(equipo)).
                    Select(equipo => new U5kManStdEquiposEurocae.EquipoEurocae(null)
                    {
                        Id = equipo.Id,
                        Ip1 = equipo.Ip,
                        Ip2 = equipo.Ip2,
                        Tipo = equipo.Tipo,
                        Modelo = equipo.Modelo,
                        RxTx = equipo.RxOrTx,
                        fid = equipo.IdDestino,
                        sip_user = equipo.IdRecurso,
                        sip_port = equipo.SipPort
                    }).ToList();

                List<BdtPabxDest> destinos = Database.ListaDestinosPABX("departamento");
                uv5ki_data.CFGPBXS = destinos.Select(d => new Uv5kManDestinosPabx.DestinoPabx() { Id = d.Id }).ToList();

            }
            catch (Exception x)
            {
                Console.WriteLine(x.Message);
            }
            finally
            {
                if (Database != null)
                    Database.dbClose();
            }
        }
    }
}
