using Distance.Diagnostics.Tls;
using Distance.Engine.Runner;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Distance.Context.Tests
{
    [TestFixture("TestData/Pcaps/testbed-16.pcap")]
    public class TestTls
    {
        private CaptureAnalyzer analyzer;
        private string sourceFilePath;
        private string outputFolderPath;

        public TestTls(string testDataPath)
        {
            var solutionDir = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory)))));
            sourceFilePath = Path.Combine(solutionDir, testDataPath);
            outputFolderPath = Path.Combine(solutionDir, "TestData/Results");
        }

        [SetUp]
        public void Setup()
        {
            analyzer = new CaptureAnalyzer(Assembly.GetAssembly(typeof(TlsCLientHello)));

        }

        [Test]
        public async Task Test()
        {
            await analyzer.AnalyzeCaptureFile(sourceFilePath, outputFolderPath);
            Assert.Pass();
        }
    }
}