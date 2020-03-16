using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using U5kSacta;

namespace UnitTesting
{
    [TestClass]
    public class SactaClientTests
    {
        [TestMethod]
        public void SactaConfigTest1()
        {
            SactaConfig Config = null;
            SactaConfig.GetConfig((cfg, error) =>
            {
                Config = cfg;
                SactaConfig.SetConfig(Config, (err) => { });
            });
        }

        [TestMethod]
        public void SactaConfigTest2()
        {
            var sacta = new SactaClientService();
            var config = sacta.Config;
            sacta.Config = config;
        }
    }
}
