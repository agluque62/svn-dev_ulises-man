using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using U5kBaseDatos;
using U5kManServer;
using U5kManServer.WebAppServer;

namespace UnitTesting
{
    [TestClass]
    public class CfgGwsUnitTest
    {
        protected void PrepareTest()
        {
            U5kManService.GlobalData = new U5kManStdData();
        }

        protected List<BdtGw> TestingBdtGwGet(int nRes)
        {
            var lres = new List<BdtGwRes>();
            for (int res = 0; res < nRes; res++)
            {
                lres.Add(new BdtGwRes()
                {
                    Id = $"RES{res}",
                    Slot = res / 4,
                    Pos = res % 4,
                    Tpo = 1,            // Telefonia.
                    Itf = 2,            // BC
                    Stpo = 0
                });
            }
            var gws = new List<BdtGw>();
            gws.Add(new BdtGw()
            {
                Id = "GWTEST",
                Ip = "1.1.1.1",
                Ip1 = "1.1.1.2",
                Ip2 = "1.1.1.3",
                Dual = true,
                SnmpPortA = 161,
                SnmpPortB = 161,
                Res = lres
            });
            return gws;
        }
        protected stdGw Bdt2Mem(BdtGw bgw)
        {
            stdGw gw = new stdGw(null)
            {
                name = bgw.Id,
                ip = bgw.Ip,
                Dual = bgw.Dual
            };
            gw.gwA = new stdPhGw()
            {
                ip = bgw.Ip1,
                snmpport = bgw.SnmpPortA,
                name = bgw.Id + "-A",
            };
            gw.gwB = new stdPhGw()
            {
                ip = bgw.Ip2,
                snmpport = bgw.SnmpPortB,
                name = bgw.Id + "-B",
            };

            // Mapea los Recursos en las Pasarelas...
            gw.gwA.RecInit();
            gw.gwB.RecInit();

            foreach (BdtGwRes res in bgw.Res)
            {
                if (res.Slot < 4 && res.Pos < 4)
                {
                    /** Configuracion de Recursos */
                    gw.gwA.slots[res.Slot].std_cfg = gw.gwB.slots[res.Slot].std_cfg = std.Ok;
                    gw.gwA.slots[res.Slot].rec[res.Pos].name = gw.gwB.slots[res.Slot].rec[res.Pos].name = res.Id;
                    gw.gwA.slots[res.Slot].rec[res.Pos].bdt_name = gw.gwB.slots[res.Slot].rec[res.Pos].bdt_name = res.Id;
                    gw.gwA.slots[res.Slot].rec[res.Pos].tipo = gw.gwB.slots[res.Slot].rec[res.Pos].tipo = U5kManService.Bdt2Rct(res.Tpo, res.Itf);
                    gw.gwA.slots[res.Slot].rec[res.Pos].bdt = gw.gwB.slots[res.Slot].rec[res.Pos].bdt = true;
                    gw.gwA.slots[res.Slot].rec[res.Pos].Stpo = gw.gwB.slots[res.Slot].rec[res.Pos].Stpo = res.Stpo;
                    // 
                    gw.gwA.slots[res.Slot].rec[res.Pos].snmp_port = gw.gwB.slots[res.Slot].rec[res.Pos].snmp_port = 161 * 100 + (((res.Slot + 1) * 10) + res.Pos + 1); ;
                    gw.gwA.slots[res.Slot].rec[res.Pos].snmp_trap_port = gw.gwA.slots[res.Slot].rec[res.Pos].snmp_trap_port = 162 * 100 + (((res.Slot + 1) * 10) + res.Pos + 1); ;

                    /** Estado de Recursos */
                    gw.gwA.slots[res.Slot].rec[res.Pos].presente = false;
                    gw.gwB.slots[res.Slot].rec[res.Pos].presente = false;
                    gw.gwA.slots[res.Slot].rec[res.Pos].std_online = std.NoInfo;
                    gw.gwB.slots[res.Slot].rec[res.Pos].std_online = std.NoInfo;
                    gw.gwA.slots[res.Slot].rec[res.Pos].tipo_online = trc.rcNotipo;
                    gw.gwB.slots[res.Slot].rec[res.Pos].tipo_online = trc.rcNotipo;

                    /** Referencia a su IP Virtual */
                    gw.gwA.slots[res.Slot].rec[res.Pos].VirtualIp = gw.ip;
                    gw.gwB.slots[res.Slot].rec[res.Pos].VirtualIp = gw.ip;
                }
            }
            return gw;
        }

        [TestMethod]
        public void DeleteResourceTestMethod()
        {
            PrepareTest();

            var cfg = TestingBdtGwGet(2);
            var lgw = new List<stdGw>();
            cfg.ForEach(bgw =>
            {
                lgw.Add(Bdt2Mem(bgw));
            });
            U5kManService.GlobalData.CFGGWS = lgw;

            lgw.Clear();
            cfg = TestingBdtGwGet(1);
            cfg.ForEach(bgw =>
            {
                lgw.Add(Bdt2Mem(bgw));
            });
            U5kManService.GlobalData.CFGGWS = lgw;
        }
    }
}
