using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace GeneExtractorFromBlastFile.Models
{
    public class Cds
    {
        public string Name { get; init; }
        public List<Exon> Exons { get; init; }
    }
}