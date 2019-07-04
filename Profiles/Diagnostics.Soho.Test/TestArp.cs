using Distance.Engine.Runner;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Distance.Diagnostics.Arp.Tests
{
    [TestFixture("TestData/Pcaps/arp-badpadding.pcapng")]
    [TestFixture("TestData/Pcaps/arp-bootup1.pcapng")]
    [TestFixture("TestData/Pcaps/arp-bootup2.pcapng")]
    [TestFixture("TestData/Pcaps/arp-iphonestartup.pcapng")]
    [TestFixture("TestData/Pcaps/arp-ping.pcapng")]
    [TestFixture("TestData/Pcaps/arp-poison.pcapng")]
    [TestFixture("TestData/Pcaps/arp-scan.pcapng")]
    [TestFixture("TestData/Pcaps/arp-sweep.pcapng")]
    public class TestArp
    {
        private CaptureAnalyzer analyzer;
        private string sourceFilePath;
        private string outputFolderPath;

        public TestArp(string testDataPath)
        {
            var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory)))));
            this.sourceFilePath = Path.Combine(solutionDir, testDataPath);
            outputFolderPath = Path.Combine(solutionDir, "TestData/Results");
        }

        [SetUp]
        public void Setup()
        {
            analyzer = new CaptureAnalyzer(Assembly.GetAssembly(typeof(ArpPacket)));
            
        }

        [Test]
        public async Task Test()
        {
            await analyzer.AnalyzeCaptureFile(sourceFilePath, outputFolderPath);
            Assert.Pass();
        }
    }
}