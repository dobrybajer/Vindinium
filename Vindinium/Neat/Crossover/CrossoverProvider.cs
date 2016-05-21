using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vindinium.NEAT.Crossover
{
    public enum CrossoverMasterType
    {
        GenotypeOne,
        GenotypeTwo
    }
    public class CrossoverProvider : ICrossoverProvider
    {
        private CorrelationResults correlationResults;

        private readonly ICorrelationProvider correlationProvider;

        public CrossoverProvider(ICorrelationProvider correlationProvider)
        {
            this.correlationProvider = correlationProvider;
        }

        public Genotype CrossoverGenotype(Genotype genotype1, Genotype genotype2)
        {
            correlationResults = correlationProvider.CorrelateConnections(genotype1.GenomeConnection, genotype2.GenomeConnection);
            CrossoverMasterType crossoverMasterType;
            var crossoverMaster = ChooseCrossoverMaster(genotype1, genotype2, out crossoverMasterType);
            var crossoverSecond = ChooseCrossoverSecond(genotype1, genotype2, crossoverMasterType);
            
            var offspringGenome = new Genotype
            {
                GenomeConnection = new List<ConnectionGenesModel>(),
                NodeGens = GetNodesForOffspring(crossoverMaster, crossoverSecond),
                Value = crossoverMaster.Value
            };
            offspringGenome.GenomeConnection.AddRange(GetConnectionsForMatch(crossoverMasterType));
            offspringGenome.GenomeConnection.AddRange(GetConnectionsForDisjoint());
            offspringGenome.GenomeConnection.AddRange(GetConnectionsForExcess());
            return offspringGenome;
        }

        private Genotype ChooseCrossoverMaster(Genotype genotype1, Genotype genotype2, out CrossoverMasterType crossoverMasterType)
        {
            var random = new Random();
            if (genotype1.Value > genotype2.Value)
                crossoverMasterType = CrossoverMasterType.GenotypeOne;
            else if (genotype1.Value < genotype2.Value)
                crossoverMasterType = CrossoverMasterType.GenotypeTwo;
            else
                crossoverMasterType = (random.Next(0, 1) == 0) ? CrossoverMasterType.GenotypeOne : CrossoverMasterType.GenotypeTwo;
            return crossoverMasterType == CrossoverMasterType.GenotypeOne ? genotype1 : genotype2;
        }

        private Genotype ChooseCrossoverSecond(Genotype genotype1, Genotype genotype2,
            CrossoverMasterType crossoverMasterType)
        {
            if (crossoverMasterType == CrossoverMasterType.GenotypeOne)
                return genotype2;
            return genotype1;
        }

        private List<ConnectionGenesModel> GetConnectionsForMatch(CrossoverMasterType crossoverMasterType)
        {
            var connectionWithGivenType = new List<ConnectionGenesModel>();
            foreach (var correlationItem in correlationResults.CorrelationItems)
            {
                if (correlationItem.CorrelationItemType == CorrelationItemType.Match)
                {
                    connectionWithGivenType.Add(crossoverMasterType == CrossoverMasterType.GenotypeOne
                        ? correlationItem.ConnectionGene1
                        : correlationItem.ConnectionGene2);
                }
            }
            return connectionWithGivenType;
        }

        private List<ConnectionGenesModel> GetConnectionsForDisjoint()
        {
            var connectionWithGivenType = new List<ConnectionGenesModel>();
            foreach (var correlationItem in correlationResults.CorrelationItems)
            {
                if (correlationItem.CorrelationItemType == CorrelationItemType.Disjoint)
                {
                    connectionWithGivenType.Add(correlationItem.ConnectionGene1 ?? correlationItem.ConnectionGene2);
                }
            }
            return connectionWithGivenType;
        }

        private List<ConnectionGenesModel> GetConnectionsForExcess()
        {
            var connectionWithGivenType = new List<ConnectionGenesModel>();
            foreach (var correlationItem in correlationResults.CorrelationItems)
            {
                if (correlationItem.CorrelationItemType == CorrelationItemType.Excess)
                {
                    connectionWithGivenType.Add(correlationItem.ConnectionGene1 ?? correlationItem.ConnectionGene2);
                }
            }
            return connectionWithGivenType;
        }

        private List<NodeGenesModel> GetNodesForOffspring(Genotype master, Genotype second)
        {
            var nodesForCorrelationType = new List<NodeGenesModel>();
            foreach (var correlationItem in correlationResults.CorrelationItems)
            {
                switch (correlationItem.CorrelationItemType)
                {
                    case CorrelationItemType.Match:
                        nodesForCorrelationType.Add(master.NodeGens.Find(n => n.NodeNumber == correlationItem.ConnectionGene1.InNode));
                        nodesForCorrelationType.Add(master.NodeGens.Find(n => n.NodeNumber == correlationItem.ConnectionGene1.OutNode));
                        break;
                    case CorrelationItemType.Disjoint:
                    case CorrelationItemType.Excess:
                        if (correlationItem.ConnectionGene1 != null)
                        {
                            nodesForCorrelationType.Add(master.NodeGens.Find(n => n.NodeNumber == correlationItem.ConnectionGene1.InNode));
                            nodesForCorrelationType.Add(master.NodeGens.Find(n => n.NodeNumber == correlationItem.ConnectionGene1.OutNode));
                        }
                        else
                        {
                            nodesForCorrelationType.Add(second.NodeGens.Find(n => n.NodeNumber == correlationItem.ConnectionGene2.InNode));
                            nodesForCorrelationType.Add(second.NodeGens.Find(n => n.NodeNumber == correlationItem.ConnectionGene2.OutNode));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return nodesForCorrelationType;
        }
    }
}
