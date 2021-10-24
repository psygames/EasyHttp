using EasyUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Test
{
    [TestClass]
    public class HttpFullTest
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            Http http = new Http();
            var json = await http.GetText("http://www.baidu.com");
            File.WriteAllText("a.json", json);
            Assert.IsTrue(!string.IsNullOrEmpty(json));
        }
    }
}
