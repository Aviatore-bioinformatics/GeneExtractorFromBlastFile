using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GeneExtractorFromBlastFile.Models;
using Serilog.Core;

namespace GeneExtractorFromBlastFile.Services
{
    public class BlastReader
    {
        private readonly Logger _logger;
        private Dictionary<string, Cds> _validCds;
        private readonly float _lengthThreshold;
        public List<Query> Queries { get; }

        public BlastReader(Logger logger, Dictionary<string, Cds> validCds, float lengthThreshold)
        {
            _validCds = validCds;
            Queries = new List<Query>();
            _logger = logger;
            _lengthThreshold = lengthThreshold;
        }
        

        public void ReadBlast(string inputFilePath)
        {
            _logger.Debug("Start reading BLAST output file ...");
            
            StringBuilder line = new StringBuilder();

            using var sr = new StreamReader(inputFilePath);
            line.Append(sr.ReadLine());
                
            while (line?.Length > 0)
            {
                try
                {
                    BlastLine blastLine = new BlastLine(line.ToString());

                    if (Queries.Any(p => p.Name.Equals(blastLine.QueryName)))
                    {
                        var query = Queries.SingleOrDefault(p => p.Name.Equals(blastLine.QueryName) && p.Cds.Any(a => a.Name.Equals(blastLine.CdsName)));

                        if (query is not null)
                        {
                            var cds = query.Cds.Single(p => p.Name.Equals(blastLine.CdsName));

                            try
                            {
                                var i = cds.Exons
                                    .SingleOrDefault(l => l.Name.Equals(blastLine.SubjectName));

                                _logger.Debug(i is not null ? $"Exon {i.Name} of length {i.Length} bp present in query {query.Name} is present also in other part of the query" : $"{query.Name} {cds.Name} has no exons");

                                if (i is not null && i.Length < blastLine.MatchLength)
                                {
                                    _logger.Debug($"Exon {i.Name} of length {i.Length} bp present in query {query.Name} has longer alternative ({blastLine.MatchLength} bp). Replacing the shorter version with the longer one ...");
                                    
                                    cds.Exons.Remove(i);
                                    cds.Exons.Add(new Exon(_lengthThreshold)
                                    {
                                        Length = blastLine.MatchLength,
                                        Name = blastLine.SubjectName
                                    });
                                }
                                else if (i is null)
                                {
                                    cds.Exons.Add(new Exon(_lengthThreshold)
                                    {
                                        Length = blastLine.MatchLength,
                                        Name = blastLine.SubjectName
                                    });
                                }
                            }
                            catch (InvalidOperationException e)
                            {
                                var p = cds.Exons
                                    .Where(l => l.Name.Equals(blastLine.SubjectName));
                                _logger.Error($"CDS '{cds.Name}' has more than one exon of the same name: {blastLine.SubjectName}:");
                                foreach (var exon in p)
                                {
                                    _logger.Error($"- query: {query.Name}, CDS: {cds.Name}, exon: {exon.Name}");
                                }
                                throw;
                            }
                        }
                        else
                        {
                            Queries.Add(new Query()
                            {
                                Name = blastLine.QueryName,
                                Cds = new List<Cds>(){new Cds(_lengthThreshold)
                                    {
                                        Name = blastLine.CdsName,
                                        Exons = new List<Exon>()
                                        {
                                            new Exon(_lengthThreshold)
                                            {
                                                Length = blastLine.MatchLength,
                                                Name = blastLine.SubjectName
                                            }
                                        }
                                    }
                                }
                            });
                        }
                    }
                    else
                    {
                        Queries.Add(new Query()
                        {
                            Name = blastLine.QueryName,
                            Cds = new List<Cds>(){new Cds(_lengthThreshold)
                                {
                                    Name = blastLine.CdsName,
                                    Exons = new List<Exon>()
                                    {
                                        new Exon(_lengthThreshold)
                                        {
                                            Length = blastLine.MatchLength,
                                            Name = blastLine.SubjectName
                                        }
                                    }
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
            
            //FilterQueries();
            
            _logger.Debug("Reading BLAST output file finished");
        }

        public void FilterQueries()
        {
            for (int queryIndex = Queries.Count - 1; queryIndex >= 0; queryIndex--)
            {
                try
                {
                    for (int cdsIndex = Queries[queryIndex].Cds.Count - 1; cdsIndex >= 0; cdsIndex--)
                    {
                        var cdsName = Queries[queryIndex].Cds[cdsIndex].Name;
                        for (int exonIndex = Queries[queryIndex].Cds[cdsIndex].Exons.Count - 1;
                            exonIndex >= 0;
                            exonIndex--)
                        {
                            var exon = Queries[queryIndex].Cds[cdsIndex].Exons[exonIndex];
                            var validCds = _validCds[cdsName].Exons.Single(p => p.Name.Equals(exon.Name));
                            if (!validCds.Verify(exon))
                            {
                                _logger.Debug($"Query {Queries[queryIndex].Name} Exon {exon.Name} ({exon.Length}bp) is shorter than {_lengthThreshold}% of {validCds.Length}bp");
                                Queries[queryIndex].Cds[cdsIndex].Exons.RemoveAt(exonIndex);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            for (int queryIndex = Queries.Count - 1; queryIndex >= 0; queryIndex--)
            {
                if (!Queries[queryIndex].Cds.Any(p => p.Exons.Any()))
                {
                    Queries.RemoveAt(queryIndex);
                }
            }
        }
        public void FilterQueries_()
        {
            for (int queryIndex = Queries.Count - 1; queryIndex >= 0; queryIndex--)
            {
                try
                {
                    for (int index = Queries[queryIndex].Cds.Count - 1; index >= 0; index--)
                    {
                        if (!_validCds[Queries[queryIndex].Cds[index].Name].Verify(Queries[queryIndex].Cds[index]))
                        {
                            Queries[queryIndex].Cds.RemoveAt(index);
                        }
                    }

                    if (!Queries[queryIndex].Cds.Any())
                    {
                        Queries.RemoveAt(queryIndex);
                    }
                }
                catch (ArgumentException e)
                {
                    throw new ArgumentException($"Problem with {Queries[queryIndex].Name}. {e.Message}");
                    _logger.Error($"Problem with {Queries[queryIndex].Name}");
                    throw;
                }
            }
        }
    }
}