using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vocabulary;

namespace TestUnitario
{
    [TestClass]
    public class UnitTestMsg
    {
        [TestMethod]
        public void CodecTest()
        {
            BinaryMessageCodec codec = new BinaryMessageCodec();

            Message msg = new Message(1, 5);
            String received = codec.Decode(codec.Encode(msg));

            Assert.AreEqual(received,msg.packet);
        }

        [TestMethod]
        public void MessageTestConstructor1()
        {
            Message msg = new Message(1, 5);
            Assert.AreEqual(5, msg.num);
        }

        [TestMethod]
        public void MessageTestConstructor2()
        {
            Message msg = new Message(3);
            Assert.AreEqual("3", msg.packet);
        }
    }
}
