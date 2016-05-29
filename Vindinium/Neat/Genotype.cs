using System.Collections.Generic;
using System.Linq;

namespace vindinium.NEAT
{
    public class Genotype
    {
        public double Value { get; set; }
        public List<ConnectionGenesModel> GenomeConnection { get; set; }
        public List<NodeGenesModel> NodeGens { get; set; }

        public Genotype(List<ConnectionGenesModel> genomeConnection, List<NodeGenesModel> nodeGens)
        {
            GenomeConnection = genomeConnection;
            NodeGens = nodeGens;
        }

        public Genotype()
        {
        }

        public Genotype DeepCopy()
        {
            return new Genotype
            {
                GenomeConnection = this.NodeGens != null
                    ? new List<ConnectionGenesModel>(this.GenomeConnection.Select(x => x.DeepCopy()).ToList())
                    : null,
                NodeGens = this.NodeGens != null
                    ? new List<NodeGenesModel>(this.NodeGens.Select(x => x.DeepCopy()).ToList())
                    : null

            };
        }

        public int GetCurrentInnovation() => GenomeConnection[GenomeConnection.Count - 1].Innovation;

        public NodeGenesModel GetNodeById(int id) => NodeGens.FirstOrDefault(n => n.NodeNumber == id);

        public ConnectionGenesModel GetEnabledConnectionByIds(int from, int to)
        {
            return GenomeConnection.First(c => c.InNode == from && c.OutNode == to && c.Status == ConnectionStatus.Enabled);
        }
    }
}
