using System;
using System.Text.RegularExpressions;

namespace GeneExtractorFromBlastFile.Models
{
    public class BlastLine
    {
        private Regex _re;
        private Regex _reCdsName;
        public string QueryName { get; }
        public int QueryStart { get; }
        public int QueryStop { get; }
        public string SubjectName { get; }
        public int SubjectStart { get; }
        public int SubjectStop { get; }
        public int MatchLength { get; }
        public int FullLength { get; }
        public bool IsExon { get; }
        public string CdsName { get; set; }

        public BlastLine(string line)
        {
            _re = new Regex(@"-ex\d+$");
            _reCdsName = new Regex(@"^[^\s]+(?=-)");
            
            var splittedLine = line.Split('\t');

            if (splittedLine.Length != 10)
            {
                throw new Exception($"Input line contains {splittedLine.Length} instead of 10 fields");
            }

            QueryName = splittedLine[0];
            
            if (!Int32.TryParse(splittedLine[1], out int queryStart))
            {
                throw new Exception("QueryStart field is not a number");
            }
            QueryStart = queryStart;
            
            if (!Int32.TryParse(splittedLine[2], out int queryStop))
            {
                throw new Exception("QueryStop field is not a number");
            }
            QueryStop = queryStop;
            
            SubjectName = splittedLine[3];
            IsExon = _re.IsMatch(SubjectName);
            if (IsExon)
            {
                CdsName = _reCdsName.Match(SubjectName).Value;
            }
            else
            {
                CdsName = SubjectName;
            }
            
            if (!Int32.TryParse(splittedLine[4], out int subjectStart))
            {
                throw new Exception("SubjectStart field is not a number");
            }
            SubjectStart = subjectStart;
            
            if (!Int32.TryParse(splittedLine[5], out int subjectStop))
            {
                throw new Exception("SubjectStop field is not a number");
            }
            SubjectStop = subjectStop;
            
            if (!Int32.TryParse(splittedLine[6], out int matchLength))
            {
                throw new Exception("MatchLength field is not a number");
            }
            MatchLength = matchLength;
            
            if (!Int32.TryParse(splittedLine[7], out int fullLength))
            {
                throw new Exception("FullLength field is not a number");
            }
            FullLength = fullLength;
        }
    }
}