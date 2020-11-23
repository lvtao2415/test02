using NUnit.Framework;
using Telerik.JustMock;
using testDto;

namespace testNUnit
{
    public class Tests
    {
        private IClass1 cs01;
        private IClass1 cs02;

        [SetUp]
        public void Setup()
        {
            cs01 = new Class1();
            cs02 = Mock.Create<IClass1>();
        }

        [Test]
        public void Test01()
        {
            Assert.IsTrue(cs01.test01());

            Mock.Arrange(() => cs02.test01()).Returns(false);
            Assert.IsFalse(cs02.test01());
        }

        [Test]
        [TestCase()]
        public void Test02()
        {
            Assert.IsTrue(cs01.test02(Arg.AnyBool));

            Mock.Arrange(() => cs02.test02(Arg.AnyBool)).Returns(false);
            Assert.IsFalse(cs02.test02(Arg.AnyBool));
        }
    }
}