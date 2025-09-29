using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    [TestClass]
    public class SecretTests : BaseTests
    {

        [TestMethod]
        public async Task GetValue()
        {
            string testKey1 = await SecretSvc.GetValue("TestKey");
            string screenName = await SecretSvc.GetValue("TwitterScreenName");

            Assert.AreEqual("TestValue", testKey1);
            Assert.AreEqual("optioncharts", screenName);
        }
    }
}
