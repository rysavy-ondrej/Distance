using Distance.Engine.Runner;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Distance.Diagnostics.Lan.Tests
{
    [TestFixture("TestData/Pcaps/lan-invalid_gateway.pcap")]
    [TestFixture("TestData/Pcaps/lan-invalid_mask.pcap")]
    [TestFixture("TestData/Pcaps/lan-ip_conflict.pcap")]
    [TestFixture("TestData/Pcaps/lan-ip_mismatch.pcap")]
    public class TestLan
    {
        private CaptureAnalyzer analyzer;
        private string sourceFilePath;

        public TestLan(string testDataPath)
        {
            var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory)))));
            this.sourceFilePath = Path.Combine(solutionDir, testDataPath);

        }

        [SetUp]
        public void Setup()
        {
            analyzer = new CaptureAnalyzer(Assembly.GetAssembly(typeof(IpPacket)));

        }

        [Test]
        public async Task Test()
        {
            await analyzer.AnalyzeCaptureFile(sourceFilePath);
            Assert.Pass();
        }
    }
}