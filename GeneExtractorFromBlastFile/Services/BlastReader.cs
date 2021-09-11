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

                                if (i is not null)
                                {
                                    Console.Out.WriteLine($"{i.Name} {i.Length}");
                                }
                                else
                                {
                                    Console.Out.WriteLine($"{cds.Name} has no exons");
                                }
                                    
                                if (i is not null && i.Length < blastLine.MatchLength)
                                {
                                    Console.Out.WriteLine($"removing {i.Name}");
                                    cds.Exons.Remove(i);
                                    cds.Exons.Add(new Exon()
                                    {
                                        Length = blastLine.MatchLength,
                                        Name = blastLine.SubjectName
                                    });
                                }
                                else if (i is null)
                                {
                                    cds.Exons.Add(new Exon()
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
                                foreach (var exon in p)
                                {
                                    Console.Out.WriteLine($"{query.Name} name: {exon.Name}");
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
                                            new Exon()
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
                                        new Exon()
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
        }

        public void FilterQueries()
        {
            for (int queryIndex = Queries.Count - 1; queryIndex >= 0; queryIndex--)
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
        }
    }
}