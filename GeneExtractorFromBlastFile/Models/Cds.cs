using System;
using System.Collections.Generic;
using System.Linq;
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
                var validExonLength = Exons.Single(p => p.Name.Equals(exon.Name)).Length;
                if (validExonLength < exon.Length)
                {
                    throw new ArgumentException(
                        $"Exon {exon.Name} of length {exon.Length} is longer than it should be, i.e. {validExonLength}");
                }
                cdsLength += exon.Length;
            }

            return validLength > 0 && ((float)cdsLength / validLength) * 100 >= _lengthThreshold;
        }
    }
}