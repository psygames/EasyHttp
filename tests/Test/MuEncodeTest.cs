using EasyUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Test
{
    [TestClass]
    public class MuEncodeTest
    {
        [TestMethod]
        public void EncodeDecode()
        {
            var src = "zip";
            var isEnc = MuEncode.EncodeAlphaNums(src, out var enc);
            Assert.IsTrue(isEnc);
            var isDec = MuEncode.DecodeAlphaNums(enc, out var dec);
            Assert.IsTrue(isDec);
            Assert.AreEqual(src, dec);
        }

        [TestMethod]
        public void EncodeDecodeX()
        {
            var check = new Dictionary<string, int>();
            var sameCount = 0;
            File.Delete("a.txt");
            var f = File.OpenWrite("a.txt");
            for (int i = 0; i < 1000000; i++)
            {

                var src = "测试aa文件.as";
                var isEnc = MuEncode.EncodeX(src, out var enc);
                var isDec = MuEncode.DecodeX(enc, out var dec);

                Assert.IsTrue(isEnc);
                Assert.IsTrue(isDec);
                Assert.AreEqual(src, dec);
                if (check.TryGetValue(enc, out var count))
                {
                    check[enc] = count + 1;
                    sameCount++;
                }
                else
                {
                    check[enc] = 1;
                }
                f.Write(Encoding.UTF8.GetBytes(src + " => " + enc + "\n"));
            }
            f.Write(Encoding.UTF8.GetBytes("Same Count: " + sameCount + "\n"));
            f.Close();
        }
    }
}
