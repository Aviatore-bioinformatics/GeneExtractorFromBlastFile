using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using GeneExtractorFromBlastFile.Models;
using GeneExtractorFromBlastFile.Services;

namespace GeneExtractorFromBlastFile.View
{
    public class TablePrinter
    {
        private readonly BlastReader _blastReader;
        private readonly string _outputFilePath;
        private readonly Dictionary<string, Cds> _validCds;

        public TablePrinter(BlastReader blastReader, Dictionary<string, Cds> validCds, string outputFilePath)
        {
            _blastReader = blastReader;
            _outputFilePath = Path.Combine(Directory.GetCurrentDirectory(), outputFilePath);
            _validCds = validCds;
        }

        public void ToTable()
        {
            Console.Out.WriteLine(_outputFilePath);
            using var sw = new StreamWriter(_outputFilePath);
            StringBuilder sb = new StringBuilder();

            sb.Append('\t');
            foreach (var cds in _validCds.OrderBy(p => p.Key))
            {
                foreach (var exon in _validCds[cds.Key].Exons.OrderBy(p => p.Name))
                {
                    sb.Append(exon.Name + '\t');
                }
            }

            //sb.Append('\n');
                
            sw.WriteLine(sb.ToString());

            sb.Clear();

            string previousQueryName = null;
            Dictionary<string, string> outputLine = new Dictionary<string, string>();
            foreach (var query in _blastReader.Queries)
            {
                if (!query.Name.Equals(previousQueryName))
                {
                    if (outputLine.Count > 0)
                    {
                        foreach (var exonName in outputLine.Keys.OrderBy(p => p))
                        {
                            sb.Append(outputLine[exonName] + '\t');
                        }

                        //sb.Append('\n');

                        sw.WriteLine(sb.ToString());

                        sb.Clear();
                        
                        outputLine.Clear();
                    }
                    
                    sb.Append(query.Name + '\t');
                    previousQueryName = query.Name;
                }
                
                foreach (var cds in _validCds)
                {
                    foreach (var exon in _validCds[cds.Key].Exons)
                    {
                        if (query.Cds.Any(p => p.Exons.Any(p => p.Name.Equals(exon.Name))))
                        {
                            if (outputLine.ContainsKey(exon.Name))
                            {
                                outputLine[exon.Name] = exon.Name;
                            }
                            else
                            {
                                outputLine.Add(exon.Name, exon.Name);
                            }
                            //sb.Append(exon.Name);
                        }
                        else
                        {
                            if (!outputLine.ContainsKey(exon.Name))
                            {
                                outputLine[exon.Name] = "";
                            }
                        }
                    
                        //sb.Append('\t');
                    }
                }
            }
            
            foreach (var exonName in outputLine.Keys.OrderBy(p => p))
            {
                sb.Append(outputLine[exonName] + '\t');
            }

            //sb.Append('\n');

            sw.WriteLine(sb.ToString());

            sb.Clear();
                        
            outputLine.Clear();
        }
    }
}