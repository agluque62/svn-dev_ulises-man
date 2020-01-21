using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using U5kBaseDatos;

namespace UnitTesting
{
    [TestClass]
    public class LocalConfigInDbUnitTest
    {
        U5kBdtService Database = new U5kBdtService(System.Threading.Thread.CurrentThread.CurrentUICulture,
            eBdt.bdtMySql,
            "192.168.0.212",
            "root",
            "cd40");

        [TestMethod]
        public void CreateAndDelete()
        {
            using (U5kiLocalConfigInDb lct = new U5kiLocalConfigInDb(Database, true))
            {
                Assert.AreEqual(lct.Idioma, "es");
                Assert.AreEqual(lct.ServidorDual, true);
                Assert.AreEqual(lct.HayReloj, true);
                Assert.AreEqual(lct.HaySacta, false);
                Assert.AreEqual(lct.HayAltavozHF, false);
                Assert.AreEqual(lct.SonidoAlarmas, false);
                Assert.AreEqual(lct.Historico_PttSqhOnBdt, false);
                Assert.AreEqual(lct.GenerarHistoricos, true);
                Assert.AreEqual(lct.DiasEnHistorico, 180);
                Assert.AreEqual(lct.LineasIncidencias, 32);
            }
        }

        [TestMethod]
        public void ChangingProperties()
        {
            using (U5kiLocalConfigInDb lct = new U5kiLocalConfigInDb(Database))
            {
                lct.Idioma = "fr";
                lct.ServidorDual = false;
                lct.HayReloj = false;
                lct.HaySacta = true;
                lct.HayAltavozHF = true;
                lct.SonidoAlarmas = true;
                lct.Historico_PttSqhOnBdt = true;
                lct.GenerarHistoricos = false;
                lct.DiasEnHistorico = 30;
                lct.LineasIncidencias = 16;

                lct.Consolidate();
            }

            using (U5kiLocalConfigInDb lct = new U5kiLocalConfigInDb(Database))
            {
                Assert.AreEqual(lct.Idioma, "fr");
                Assert.AreEqual(lct.ServidorDual, false);
                Assert.AreEqual(lct.HayReloj, false);
                Assert.AreEqual(lct.HaySacta, true);
                Assert.AreEqual(lct.HayAltavozHF, true);
                Assert.AreEqual(lct.SonidoAlarmas, true);
                Assert.AreEqual(lct.Historico_PttSqhOnBdt, true);
                Assert.AreEqual(lct.GenerarHistoricos, false);
                Assert.AreEqual(lct.DiasEnHistorico, 30);
                Assert.AreEqual(lct.LineasIncidencias, 16);
            }

            CreateAndDelete();
        }
    }
}
