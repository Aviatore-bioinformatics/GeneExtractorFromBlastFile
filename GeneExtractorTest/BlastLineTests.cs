using GeneExtractorFromBlastFile.Models;
using NUnit.Framework;

namespace GeneExtractorTest
{
    [TestFixture]
    public class BlastLineTests
    {
        [Test]
        public void BlastLineSubjectNameSingleExon()
        {
            string line = "scf7180000105533	76342	77421	rps2	1083	1	1068	1083	1/-1	minus";
            var blastLine = new BlastLine(line);
            
            Assert.That(blastLine.SubjectName, Is.EqualTo("rps2"));
        }
        
        [Test]
        public void BlastLineSubjectNameMultipleExons()
        {
            string line = "scf7180000105533	62195	62878	rps3-ex2	929	1612	677	684	1/1	plus";
            var blastLine = new BlastLine(line);
            
            Assert.That(blastLine.SubjectName, Is.EqualTo("rps3-ex2"));
        }
        
        [Test]
        public void BlastLineCdsNameMultipleExons()
        {
            string line = "scf7180000105533	62195	62878	rps3-ex2	929	1612	677	684	1/1	plus";
            var blastLine = new BlastLine(line);
            
            Assert.That(blastLine.CdsName, Is.EqualTo("rps3"));
        }
        
        [Test]
        public void BlastLineIsExonTrue()
        {
            string line = "scf7180000105533	62195	62878	rps3-ex2	929	1612	677	684	1/1	plus";
            var blastLine = new BlastLine(line);
            
            Assert.That(blastLine.IsExon, Is.True);
        }
        
        [Test]
        public void BlastLineIsExonFalse()
        {
            string line = "scf7180000105533	76342	77421	rps2	1083	1	1068	1083	1/-1	minus";
            var blastLine = new BlastLine(line);
            
            Assert.That(blastLine.IsExon, Is.False);
        }
    }
}