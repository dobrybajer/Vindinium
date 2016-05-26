using System;
using System.Collections.Generic;
using System.Linq;

namespace vindinium.NEAT.Crossover
{
    public class CorrelationProvider : ICorrelationProvider
    {
        public CorrelationResults CorrelateConnections(List<ConnectionGenesModel> list1, List<ConnectionGenesModel> list2)
        {
            list1 = list1.OrderBy(c => c.Innovation).ToList();
            list2 = list2.OrderBy(c => c.Innovation).ToList();
            var crossoverResults = new CorrelationResults
            {
                CorrelationItems = new List<CorrelationItem>(),
                CorrelationStats = new CorrelationStats()
            };

            if (!list1.Any() && list2.Any())
                return crossoverResults;

            if (!list1.Any())
            {
                crossoverResults.CorrelationStats.ExcessConnectionGeneCount = list2.Count;
                foreach (var connectionGene in list2)
                    crossoverResults.CorrelationItems.Add(new CorrelationItem(CorrelationItemType.Excess, null, connectionGene));
                return crossoverResults;
            }

            if (!list2.Any())
            {   
                crossoverResults.CorrelationStats.ExcessConnectionGeneCount = list1.Count;
                foreach (var connectionGene in list1)
                    crossoverResults.CorrelationItems.Add(new CorrelationItem(CorrelationItemType.Excess, connectionGene, null));
                return crossoverResults;
            }

            var list1Idx = 0;
            var list2Idx = 0;
            var connectionGene1 = list1[list1Idx];
            var connectionGene2 = list2[list2Idx];
            for (;;)
            {
                if (connectionGene2.Innovation < connectionGene1.Innovation)
                {
                    crossoverResults.CorrelationItems.Add(new CorrelationItem(CorrelationItemType.Disjoint, null, connectionGene2));
                    crossoverResults.CorrelationStats.DisjointConnectionGeneCount++;

                    list2Idx++;
                }
                else if (connectionGene1.Innovation == connectionGene2.Innovation)
                {
                    crossoverResults.CorrelationItems.Add(new CorrelationItem(CorrelationItemType.Match, connectionGene1, connectionGene2));
                    crossoverResults.CorrelationStats.ConnectionWeightDelta += Math.Abs(connectionGene1.Weight - connectionGene2.Weight);
                    crossoverResults.CorrelationStats.MatchingGeneCount++;

                    list1Idx++;
                    list2Idx++;
                }
                else 
                {
                    crossoverResults.CorrelationItems.Add(new CorrelationItem(CorrelationItemType.Disjoint, connectionGene1, null));
                    crossoverResults.CorrelationStats.DisjointConnectionGeneCount++;

                    list1Idx++;
                }

                if (list1.Count == list1Idx)
                {
                    for (; list2Idx < list2.Count; list2Idx++)
                    {
                        crossoverResults.CorrelationItems.Add(new CorrelationItem(CorrelationItemType.Excess, null, list2[list2Idx]));
                        crossoverResults.CorrelationStats.ExcessConnectionGeneCount++;
                    }
                    return crossoverResults;
                }

                if (list2.Count == list2Idx)
                {
                    for (; list1Idx < list1.Count; list1Idx++)
                    {
                        crossoverResults.CorrelationItems.Add(new CorrelationItem(CorrelationItemType.Excess, list1[list1Idx], null));
                        crossoverResults.CorrelationStats.ExcessConnectionGeneCount++;
                    }
                    return crossoverResults;
                }

                connectionGene1 = list1[list1Idx];
                connectionGene2 = list2[list2Idx];
            }
        }
    }
}
