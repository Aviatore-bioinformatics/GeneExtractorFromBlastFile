using System;
using System.IO;
using System.Linq;
using GeneExtractorFromBlastFile.Models;
using GeneExtractorFromBlastFile.Services;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace GeneExtractorFromBlastFile
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            var commandLineOptions = CommandLine.Parser.Default.ParseArguments<CommandLineOptions>(args);

            if (commandLineOptions.Errors.Any())
            {
                return;
            }

            string exonsFilePath = Path.Combine(Directory.GetCurrentDirectory(), commandLineOptions.Value.GeneLengthFile);

            ExonReader exonReader = new ExonReader(logger, commandLineOptions.Value.LengthThreshold);
            exonReader.ReadExons(exonsFilePath);
            var validCds = exonReader.GetValidCds();
            /*foreach (var cds in validCds)
            {
                Console.Out.WriteLine($"Cds name: {cds.Key}");
                Console.Out.WriteLine("Cds exons:");
                foreach (var exon in cds.Value.Exons)
                {
                    Console.Out.WriteLine($"- {exon.Name} {exon.Length} bp");
                }
            }*/

            string blastFilePath = Path.Combine(Directory.GetCurrentDirectory(), commandLineOptions.Value.BlastOutputFile);
            BlastReader blastReader = new BlastReader(logger, validCds, commandLineOptions.Value.LengthThreshold);
            blastReader.ReadBlast(blastFilePath);

            foreach (var query in blastReader.Queries)
            {
                Console.Out.WriteLine($"Query name: {query.Name}, cds:");
                foreach (var cds in query.Cds)
                {
                    Console.Out.WriteLine($"- cds name: {cds.Name}, exons:");
                    foreach (var exon in cds.Exons)
                    {
                        Console.Out.WriteLine($" - exon name: {exon.Name}, length: {exon.Length}");
                    }
                }
            }
        }
    }
}