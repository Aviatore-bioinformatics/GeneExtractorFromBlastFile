using CommandLine;

namespace GeneExtractorFromBlastFile.Models
{
    public class CommandLineOptions
    {
        [Option('b', "binput", Required = true, HelpText = "BLAST output file")]
        public string BlastOutputFile { get; set; }

        [Option('g', "ginput", Required = true, HelpText = "File containing gene lengths")]
        public string GeneLengthFile { get; set; }

        [Option('o', "output", Required = false, Default = "table.txt", HelpText = "Output file name")]
        public string OutputFileName { get; set; }

        [Option('t', "threshold", Required = false, Default = 70, HelpText = "Percentage of homology")]
        public float LengthThreshold { get; set; }
    }
}