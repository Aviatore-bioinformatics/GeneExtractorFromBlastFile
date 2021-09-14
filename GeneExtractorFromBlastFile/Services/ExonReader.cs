using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GeneExtractorFromBlastFile.Models;
using Serilog.Core;

namespace GeneExtractorFromBlastFile.Services
{
    public class ExonReader
    {
        private readonly Logger _logger;
        private readonly Dictionary<string, Cds> _validCds;
        private readonly float _lengthThreshold;

        public ExonReader(Logger logger, float lengthThreshold)
        {
            _logger = logger;
            _validCds = new Dictionary<string, Cds>();
            _lengthThreshold = lengthThreshold;
        }

        public Dictionary<string, Cds> GetValidCds() => _validCds;

        public void ReadExons(string inputFilePath)
        {
            StringBuilder line = new StringBuilder();

            using var sr = new StreamReader(inputFilePath);
            line.Append(sr.ReadLine());

            while (line.Length > 0)
            {
                try
                {
                    var exonLine = new ExonLine(line.ToString());

                    if (_validCds.ContainsKey(exonLine.CdsName))
                    {
                        if (_validCds[exonLine.CdsName].Exons.Any(p => p.Name.Equals(exonLine.ExonName)))
                        {
                            _logger.Error($"Duplicated gene name: {exonLine.ExonName}");
                            throw new ArgumentException($"Duplicated gene name: {exonLine.ExonName}");
                        }
                            
                        _validCds[exonLine.CdsName].Exons.Add(new Exon(_lengthThreshold)
                        {
                            Name = exonLine.ExonName,
                            Length = exonLine.ExonLength
                        });
                    }
                    else
                    {
                        _validCds.Add(exonLine.CdsName, new Cds()
                        {
                            Name = exonLine.CdsName,
                            Exons = new List<Exon>(){new Exon(_lengthThreshold)
                                {
                                    Name = exonLine.ExonName,
                                    Length = exonLine.ExonLength
                                }
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);
                    throw;
                }
                    
                line.Clear();
                line.Append(sr.ReadLine());
            }
        }
    }
}