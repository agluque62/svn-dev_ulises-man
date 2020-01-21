using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

using U5kManServer;

namespace UnitTesting
{
    [TestClass]
    public class PresenceDataServiceUnitTest
    {
        [TestMethod]
        public void PresenceDataServieTestMethod01()
        {
            PresenceDataService pds = new PresenceDataService() { TickInSeconds = 20, Testing=true };
            pds.Start();

            MessageBox.Show("Pulse para Finalizar Prueba");

            pds.Dispose();
        }
    }

}
