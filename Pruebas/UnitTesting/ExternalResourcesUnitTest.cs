using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using U5kBaseDatos;
using U5kManServer;
using Utilities;

namespace UnitTesting
{

    [TestClass]
    public class ExternalResourcesUnitTest
    {
        List<BdtEuEq> equipos = new List<BdtEuEq>()
            {
                new BdtEuEq(){ Id="REQ-01",Ip="192.168.0.51", Ip2="192.168.1.51",  Tipo=2, IdDestino="", IdRecurso="RTX-01", RxOrTx=4, Modelo=1000, SipPort=5060},
                new BdtEuEq(){ Id="REQ-01",Ip="192.168.0.51", Ip2="192.168.1.51",  Tipo=2, IdDestino="", IdRecurso="RTX-02", RxOrTx=4, Modelo=1000, SipPort=5060},
                new BdtEuEq(){ Id="REQ-01",Ip="192.168.0.51", Ip2="192.168.1.51",  Tipo=2, IdDestino="", IdRecurso="RTX-03", RxOrTx=4, Modelo=1000, SipPort=5060},
                
                new BdtEuEq(){ Id="REQ-02",Ip="192.168.0.129", Ip2="192.168.0.129",  Tipo=2, IdDestino="", IdRecurso="RRX-01", RxOrTx=5, Modelo=1001, SipPort=5060},
                new BdtEuEq(){ Id="REQ-02",Ip="192.168.0.129", Ip2="192.168.0.129",  Tipo=2, IdDestino="", IdRecurso="RRX-02", RxOrTx=5, Modelo=1001, SipPort=5060},
                new BdtEuEq(){ Id="REQ-02",Ip="192.168.0.129", Ip2="192.168.0.129",  Tipo=2, IdDestino="", IdRecurso="RRX-03", RxOrTx=5, Modelo=1001, SipPort=5060},

                new BdtEuEq(){ Id="TEQ-01", Ip="192.168.0.155",Ip2="192.168.0.155", Tipo=3, IdDestino="", IdRecurso="BC_4",  RxOrTx=0, Modelo=1000, SipPort=5060},
                new BdtEuEq(){ Id="TEQ-01", Ip="192.168.0.155",Ip2="192.168.0.155", Tipo=3, IdDestino="", IdRecurso="BL_4",  RxOrTx=0, Modelo=1000, SipPort=5060},
                new BdtEuEq(){ Id="TEQ-01", Ip="192.168.0.155",Ip2="192.168.0.155", Tipo=3, IdDestino="", IdRecurso="AB_4",  RxOrTx=0, Modelo=1000, SipPort=5060},

                new BdtEuEq(){ Id="BC-1", Ip="192.168.0.52",Ip2="192.168.0.51", Tipo=3, IdDestino="", IdRecurso="BC-1",  RxOrTx=0, Modelo=1000, SipPort=5060},
                new BdtEuEq(){ Id="LC_22",Ip="192.168.0.53",Ip2="192.168.0.129", Tipo=3, IdDestino="", IdRecurso="LC_22", RxOrTx=0, Modelo=1000, SipPort=5060},

                new BdtEuEq(){ Id="REC_A",Ip="192.168.0.54",Ip2="192.168.0.129", Tipo=5, IdDestino="", IdRecurso="REC_A", RxOrTx=0, Modelo=1000, SipPort=5060},
                new BdtEuEq(){ Id="REC_B",Ip="192.168.0.55",Ip2="192.168.0.155", Tipo=5, IdDestino="", IdRecurso="REC_B", RxOrTx=0, Modelo=1000, SipPort=5060}
            };

        private SipSupervisor sips = new SipSupervisor(new SipUA() { user = "MTTO", ip = "127.0.0.1", port = 7060 });
        private List<string> AllowedSipResponses = new List<string>() { "404", "503" };

        [TestMethod]
        public void TestMethod1()
        {
            /** Genera la lista de Equipos */
            var lequipos = equipos
                .Select(equipo => new EquipoEurocae(null)
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

            /** Agruparlos por equipo */
            var grupos = lequipos.GroupBy(eq => eq.Ip1)
                .ToDictionary(grp => grp.Key, grp => grp.ToList());
            List<Task> tasks = new List<Task>();

            foreach (var grp in grupos)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Console.WriteLine($"Supervisando Equipo en {grp.Key}");
                        SupervisaEquipo(grp.Key, grp.Value);
                    }
                    catch(Exception x)
                    {
                        Console.WriteLine($"Supervisando Equipo en {grp.Key}. Excepcion => {x.Message}");
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray(), 9000);
        }

        protected void SupervisaEquipo(string ip, List<EquipoEurocae> recursos)
        {
            List<Task> stasks = new List<Task>();
            bool ping = U5kGenericos.Ping(ip, true);
            foreach(var recurso in recursos)
            {
                recurso.EstadoRed1 = recurso.EstadoRed2 =  ChangeStd(recurso, ping == true ? std.Ok : std.NoInfo); /** Provocará el histórico */
                Console.WriteLine($"Recurso {recurso.Id}, Estado Red => {recurso.EstadoRed1}");
                if (ping == true)
                {
                    /** Estado Agente SIP */
                    if (recurso.Tipo == 5)
                    {
                        /** Los Grabadores no tienen Agente SIP, Para que se muestre Ok, 
                            Ponemos que está bien */
                        recurso.EstadoSip = std.Ok;
                        Console.WriteLine($"Recurso Grabacion {recurso.Id} => {recurso.EstadoSip}");
                    }
                    else
                    {
                        stasks.Add(Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                Console.WriteLine($"Supervisando recurso {recurso.sip_user}");
                                SupervisaRecurso(recurso);
                            }
                            catch(Exception x)
                            {
                                Console.WriteLine($"Supervisando recurso {recurso.sip_user}. Excepcion => {x.Message}");
                            }
                        }));
                    }
                }
            }
            Task.WaitAll(stasks.ToArray(), 9000);
        }

        protected void SupervisaRecurso(EquipoEurocae recurso)
        {
            SipUA ua = new SipUA()
            {
                user = recurso.sip_user,
                ip = recurso.Ip1,
                port = recurso.sip_port,
                radio = (recurso.Tipo == 2)
            };
            
            bool sipp = sips?.SipPing(ua) ?? false;
            if (sipp == true)
            {
                if (ua.last_response != null)
                {
                    var allowedReponse = AllowedSipResponses.Contains(ua.last_response.Result);
                    recurso.EstadoSip = allowedReponse ? std.Ok : std.Aviso;
                    recurso.LastOptionsResponse = ua.last_response.Result;
                    Console.WriteLine($"Supervisando recurso {recurso.sip_user}. SipAgent response {recurso.LastOptionsResponse}, EstadoSip => {recurso.EstadoSip}");
                }
                else
                {
                    recurso.EstadoSip = std.Error;
                    recurso.LastOptionsResponse = "";
                    Console.WriteLine($"Supervisando recurso {recurso.sip_user}. SipAgent Respuesta NULA.");
                }

            }
            else
            {
                recurso.EstadoSip = std.Error;
                recurso.LastOptionsResponse = "";
                Console.WriteLine($"Supervisando recurso {recurso.sip_user}. SipAgent no contesta...");
            }
        }

        private std ChangeStd(EquipoEurocae equipo, std nuevo)
        {
            return nuevo;
        }


    }
}
