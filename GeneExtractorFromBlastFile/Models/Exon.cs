using System;

namespace GeneExtractorFromBlastFile.Models
{
    public class Exon
    {
        private float _lengthThreshold;
        public string Name { get; set; }
        public int Length { get; set; }

        public Exon(float lengthThreshold)
        {
            _lengthThreshold = lengthThreshold;
        }
        public bool Verify(Exon exon)
        {
            if (!Name.Equals(exon.Name))
            {
                return false;
            }
            
            if (Length < exon.Length)
            {
                throw new ArgumentException(
                    $"Exon {exon.Name} of length {exon.Length} is longer than it should be, i.e. {Length}");
            }
            
            return Length > 0 && ((float)exon.Length / Length) * 100 >= _lengthThreshold;
        }
    }
}