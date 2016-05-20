using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vindinium.Neat
{
    class Genotype
    {
        public List<ConnectionGenesModel> GenomeConnection;
        public List<NodeGenesModel> NodeGens;

        public Genotype(List<ConnectionGenesModel> genomeConnection, List<NodeGenesModel> nodeGens)
        {
            GenomeConnection = genomeConnection;
            NodeGens = nodeGens;
        }

        public int GetCurrentInnovation()
        {
            return GenomeConnection[GenomeConnection.Count - 1].Innovation;
        }
    }
}
