using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vindinium.Neat
{
    class Neat
    {
        public int CurrentInnovation { get; set; }

        public Neat()
        {
            CurrentInnovation = 0;
        }

        public Genotype MutateAddConnection(Genotype genotype)
        {
            var nodeNumber = genotype.NodeGens.Count;

            Random random = new Random();
            var inNode = random.Next(1, nodeNumber-1);//Dopuszczamy połączenia z samym sobą?
            var outNode = random.Next(1, nodeNumber);

            if(inNode< outNode)
            {
                genotype.GenomeConnection.Add(new ConnectionGenesModel()
                {
                    InNode = inNode,
                    OutNode = outNode,
                    Weight = random.Next(0, 100) / 100,
                    Status = ConnectionStatus.Enabled,
                    Innovation = genotype.GetCurrentInnovation() + 1,
                });
            }
            else
            {
                genotype.GenomeConnection.Add(new ConnectionGenesModel()
                {
                    InNode = outNode,
                    OutNode = inNode,
                    Weight = random.Next(0, 100) / 100,
                    Status = ConnectionStatus.Enabled,
                    Innovation = genotype.GetCurrentInnovation() + 1,
                });
            }

            return genotype;
            
        }

        public Genotype MutateAddNode(Genotype genotype)
        {
            var connectionNumber = genotype.GenomeConnection.Count;

            Random random = new Random();
            var chooseConnection = random.Next(1, connectionNumber );
            genotype.GenomeConnection[chooseConnection].Status= ConnectionStatus.Disabled;
            genotype.NodeGens.Add(new NodeGenesModel()
            {
                NodeNumber = genotype.NodeGens.Count + 1,
                Type = NodeType.Hidden
            });
            genotype.GenomeConnection.Add(new ConnectionGenesModel()
            {
                InNode = genotype.GenomeConnection[chooseConnection].InNode,
                OutNode = genotype.NodeGens.Count,
                Weight = random.Next(0, 100) / 100,
                Status = ConnectionStatus.Enabled,
                Innovation = genotype.GetCurrentInnovation() + 1,
            });
            genotype.GenomeConnection.Add(new ConnectionGenesModel()
            {
                InNode = genotype.NodeGens.Count,
                OutNode = genotype.GenomeConnection[chooseConnection].OutNode,
                Weight = random.Next(0, 100) / 100,
                Status = ConnectionStatus.Enabled,
                Innovation = genotype.GetCurrentInnovation() + 1,
            });

            return genotype;
        }

        public void MatchingGenomes(Genotype genotype1, Genotype genotype2)
        {
            throw new Exception();
        }
    }
}
