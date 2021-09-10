using System.Collections.Generic;

namespace GeneExtractorFromBlastFile.Models
{
    public class Query
    {
        public string Name { get; set; }
        public List<Cds> Cds { get; set; }
    }
}