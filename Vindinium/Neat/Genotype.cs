using System.Collections.Generic;
using System.Linq;

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

        public NodeGenesModel GetNodeById(int id)
        {
            return NodeGens.FirstOrDefault(n => n.NodeNumber == id);
        }

        public ConnectionGenesModel GetEnabledConnectionByIds(int from, int to)
        {
            return GenomeConnection.First(c => c.InNode == from && c.OutNode == to && c.Status == ConnectionStatus.Enabled);
        }
    }
}
