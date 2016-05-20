using System.Collections.Generic;

namespace vindinium.NEAT
{
    internal class Genotype
    {
        public double Value;
        public List<ConnectionGenesModel> GenomeConnection;
        public List<NodeGenesModel> NodeGens;

        public Genotype(List<ConnectionGenesModel> genomeConnection, List<NodeGenesModel> nodeGens)
        {
            GenomeConnection = genomeConnection;
            NodeGens = nodeGens;
        }

        public Genotype()
        {
        }

        public int GetCurrentInnovation()
        {
            return GenomeConnection[GenomeConnection.Count - 1].Innovation;
        }
    }
}
