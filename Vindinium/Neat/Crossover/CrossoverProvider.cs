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
            
            var offspringGenome = new Genotype
            {
                GenomeConnection = new List<ConnectionGenesModel>(),
                NodeGens = new List<NodeGenesModel>(crossoverMaster.NodeGens),
                Value = crossoverMaster.Value
            };
            offspringGenome.GenomeConnection.AddRange(GetConnectionsForCorrelationType(crossoverMasterType, CorrelationItemType.Match));
            offspringGenome.GenomeConnection.AddRange(GetConnectionsForCorrelationType(crossoverMasterType, CorrelationItemType.Disjoint));
            offspringGenome.GenomeConnection.AddRange(GetConnectionsForCorrelationType(crossoverMasterType, CorrelationItemType.Excess));
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

        private List<ConnectionGenesModel> GetConnectionsForCorrelationType(CrossoverMasterType crossoverMasterType, CorrelationItemType correlationItemType)
        {
            var connectionWithGivenType = new List<ConnectionGenesModel>();
            foreach (var correlationItem in correlationResults.CorrelationItems)
            {
                if (correlationItem.CorrelationItemType == correlationItemType)
                {
                    connectionWithGivenType.Add(crossoverMasterType == CrossoverMasterType.GenotypeOne
                        ? correlationItem.ConnectionGene1
                        : correlationItem.ConnectionGene2);
                }
            }
            return connectionWithGivenType;
        }
    }
}
