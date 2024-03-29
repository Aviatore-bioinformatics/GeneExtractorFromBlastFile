﻿using System;
using System.IO;
using System.Linq;
using GeneExtractorFromBlastFile.Models;
using GeneExtractorFromBlastFile.Services;
using GeneExtractorFromBlastFile.View;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

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

            Logger logger = new LoggerConfiguration()
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

            //PrintQueries(blastReader, logger);

            try
            {
                blastReader.FilterQueries();
                
                //PrintQueries(blastReader, logger);
                
                TablePrinter tablePrinter =
                    new TablePrinter(blastReader, validCds, commandLineOptions.Value.OutputFileName);
                tablePrinter.ToTable();
            }
            catch (ArgumentException e)
            {
                logger.Error(e.Message);
            }
        }

        static void PrintQueries(BlastReader blastReader, Logger logger)
        {
            logger.Debug("Printing queries' CDS ...");
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