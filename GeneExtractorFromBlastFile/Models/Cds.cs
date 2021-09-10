using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GeneExtractorFromBlastFile.Models
{
    public class Cds
    {
        private float _lengthThreshold;
        public string Name { get; set; }
        public List<Exon> Exons { get; set; }

        public Cds(float lengthThreshold)
        {
            _lengthThreshold = lengthThreshold;
        }
        
        
        public bool Verify(Cds cds)
        {
            int validLength = 0;

            foreach (var exon in Exons)
            {
                validLength += exon.Length;
            }

            int cdsLength = 0;

            foreach (var exon in cds.Exons)
            {
                cdsLength += exon.Length;
            }

            return validLength > 0 && ((float)cdsLength / validLength) * 100 >= _lengthThreshold;
        }
    }
}