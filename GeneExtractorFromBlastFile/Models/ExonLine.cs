using System;
using System.Text.RegularExpressions;

namespace GeneExtractorFromBlastFile.Models
{
    public class ExonLine
    {
        private Regex _cdsName;

        public string CdsName { get; set; }
        public string ExonName { get; set; }
        public int ExonLength { get; set; }

        public ExonLine(string line)
        {
            _cdsName = new Regex(@"^[^\s]+(?=-ex)");
            
            var splittedLine = line.Split('\t');
            
            if (splittedLine.Length != 2)
            {
                throw new Exception($"Input line contains {splittedLine.Length} instead of 2 fields");
            }

            if (_cdsName.IsMatch(splittedLine[0]))
            {
                CdsName = _cdsName.Match(splittedLine[0]).Value;
            }
            else
            {
                CdsName = splittedLine[0];
            }
            
            ExonName = splittedLine[0];
            
            if (!Int32.TryParse(splittedLine[1], out int exonLength))
            {
                throw new Exception("ExonLength field is not a number");
            }
            ExonLength = exonLength;
        }
    }
}