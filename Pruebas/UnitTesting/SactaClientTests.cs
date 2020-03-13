using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using U5kSacta;

namespace UnitTesting
{
    [TestClass]
    public class SactaClientTests
    {
        [TestMethod]
        public void SactaConfigTest()
        {
            object Config = null;
            SactaConfig.GetConfig((cfg, error) =>
            {
                Config = cfg;
                SactaConfig.SetConfig(Config, (err) => { });
            });
        }
    }
}
