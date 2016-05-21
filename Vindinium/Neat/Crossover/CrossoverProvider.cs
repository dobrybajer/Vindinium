using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vindinium.NEAT.Crossover
{
    public enum CrossoverMaster
    {
        GenotypeOne,
        GenotypeTwo
    }
    public class CrossoverProvider
    {
        private CorrelationResults correlationResults;

        private readonly ICorrelationProvider correlationProvider;

        public CrossoverProvider(ICorrelationProvider correlationProvider)
        {
            this.correlationProvider = correlationProvider;
        }


        public Genotype CrossoverGenotype(Genotype genotype1, Genotype genotype2)
        {
            var random = new Random();
            correlationResults = correlationProvider.CorrelateConnections(genotype1.GenomeConnection,
                genotype2.GenomeConnection);
            CrossoverMaster crossoverMaster;
            if (genotype1.Value > genotype2.Value)
                crossoverMaster = CrossoverMaster.GenotypeOne;
            else if (genotype1.Value < genotype2.Value)
                crossoverMaster = CrossoverMaster.GenotypeTwo;
            else
                crossoverMaster = (random.Next(0, 1) == 0) ? CrossoverMaster.GenotypeOne : CrossoverMaster.GenotypeTwo;

            var offspringGenome = new Genotype { GenomeConnection = new List<ConnectionGenesModel>() };
            offspringGenome.GenomeConnection.AddRange(GetConnectionsForCorrelationType(crossoverMaster, CorrelationItemType.Match));
            offspringGenome.GenomeConnection.AddRange(GetConnectionsForCorrelationType(crossoverMaster, CorrelationItemType.Disjoint));
            offspringGenome.GenomeConnection.AddRange(GetConnectionsForCorrelationType(crossoverMaster, CorrelationItemType.Excess));
            return offspringGenome;
        }

        private List<ConnectionGenesModel> GetConnectionsForCorrelationType(CrossoverMaster crossoverMaster, CorrelationItemType correlationItemType)
        {
            var connectionWithGivenType = new List<ConnectionGenesModel>();
            foreach (var correlationItem in correlationResults.CorrelationItems)
            {
                if (correlationItem.CorrelationItemType == correlationItemType)
                {
                    connectionWithGivenType.Add(crossoverMaster == CrossoverMaster.GenotypeOne
                        ? correlationItem.ConnectionGene1
                        : correlationItem.ConnectionGene2);
                }
            }
            return connectionWithGivenType;
        }
    }
}
