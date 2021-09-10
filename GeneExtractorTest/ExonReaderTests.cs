using GeneExtractorFromBlastFile.Services;
using Moq;
using NUnit.Framework;
using Serilog.Core;

namespace GeneExtractorTest
{
    [TestFixture]
    public class ExonReaderTests
    {
        private Mock<Logger> _logger;

        [OneTimeSetUp]
        public void Setup()
        {
            _logger = new Mock<Logger>();
            _logger.Setup(p => p.Error("Error"));
        }

        [Test]
        public void ExonReaderSingleExonLineCount()
        {
            ExonReader exonReader = new ExonReader(_logger.Object, 70);
            exonReader.ReadExons();
        }
    }
}