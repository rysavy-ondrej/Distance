using Distance.Engine.Runner;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Distance.Diagnostics.Lan.Tests
{
    [TestFixture("TestData/Pcaps/lan-invalid_gateway.pcapng")]
    [TestFixture("TestData/Pcaps/lan-invalid_mask.pcapng")]
    [TestFixture("TestData/Pcaps/lan-ip_conflict.pcapng")]
    [TestFixture("TestData/Pcaps/lan-ip_mismatch.pcapng")]
    public class TestLan
    {
        private CaptureAnalyzer analyzer;
        private string sourceFilePath;
        private string outputFolderPath;

        public TestLan(string testDataPath)
        {
            var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory)))));
            sourceFilePath = Path.Combine(solutionDir, testDataPath);
            outputFolderPath = Path.Combine(solutionDir, "TestData/Results");
        }

        [SetUp]
        public void Setup()
        {
            analyzer = new CaptureAnalyzer(Assembly.GetAssembly(typeof(IpPacket)));

        }

        [Test]
        public async Task Test()
        {
            await analyzer.AnalyzeCaptureFile(sourceFilePath, outputFolderPath);
            Assert.Pass();
        }
    }
}