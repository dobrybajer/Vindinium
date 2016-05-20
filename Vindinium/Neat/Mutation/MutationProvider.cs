using System;
using vindinium.NEAT.Helpers;

namespace vindinium.NEAT.Mutation
{
    internal class MutationProvider : IMutationProvider
    {
        public void Mutate()
        {

        }

        public Genotype MutateAddConnection(Genotype genotype)
        {
            var nodeNumber = genotype.NodeGens.Count;

            var random = new Random();
            var inNode = random.Next(1, nodeNumber - 1); //Dopuszczamy połączenia z samym sobą?
            var outNode = random.Next(1, nodeNumber);

            if (inNode < outNode)
            {
                genotype.GenomeConnection.Add(new ConnectionGenesModel()
                {
                    InNode = inNode,
                    OutNode = outNode,
                    Weight = (double)random.Next(0, 100) / 100,
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
                    Weight = (double)random.Next(0, 100) / 100,
                    Status = ConnectionStatus.Enabled,
                    Innovation = genotype.GetCurrentInnovation() + 1,
                });
            }

            return genotype;

        }

        public Genotype MutateAddNode(Genotype genotype)
        {
            var connectionNumber = genotype.GenomeConnection.Count;

            var random = new Random();
            var chooseConnection = random.Next(1, connectionNumber);
            genotype.GenomeConnection[chooseConnection].Status = ConnectionStatus.Disabled;
            genotype.NodeGens.Add(new NodeGenesModel()
            {
                NodeNumber = genotype.NodeGens.Count + 1,
                Type = NodeType.Hidden
            });
            genotype.GenomeConnection.Add(new ConnectionGenesModel()
            {
                InNode = genotype.GenomeConnection[chooseConnection].InNode,
                OutNode = genotype.NodeGens.Count,
                Weight = (double)random.Next(0, 100) / 100,
                Status = ConnectionStatus.Enabled,
                Innovation = genotype.GetCurrentInnovation() + 1,
            });
            genotype.GenomeConnection.Add(new ConnectionGenesModel()
            {
                InNode = genotype.NodeGens.Count,
                OutNode = genotype.GenomeConnection[chooseConnection].OutNode,
                Weight = (double)random.Next(0, 100) / 100,
                Status = ConnectionStatus.Enabled,
                Innovation = genotype.GetCurrentInnovation() + 1,
            });

            return genotype;
        }


        private ConnectionGenesModel MutateDeleteConnection(Genotype genotype)
        {
            var connectionNumber = genotype.GenomeConnection.Count;
            var random = new Random();
            var choosenConnectionId = random.Next(1, connectionNumber);
            var connectionToDelete = genotype.GenomeConnection[choosenConnectionId];

            genotype.GenomeConnection.RemoveAt(choosenConnectionId);

            var srcNeuronIdx = genotype.NodeGens[connectionToDelete.InNode].NodeNumber;
            var srcNeuronGene = genotype.NodeGens[srcNeuronIdx];
            srcNeuronGene.TargetNodes.Remove(connectionToDelete.OutNode);

            if (srcNeuronGene.IsNodeRedundant())
                genotype.NodeGens.RemoveAt(srcNeuronIdx);

            var tgtNeuronIdx = genotype.NodeGens[connectionToDelete.OutNode].NodeNumber;
            var tgtNeuronGene = genotype.NodeGens[tgtNeuronIdx];
            tgtNeuronGene.SourceNodes.Remove(connectionToDelete.InNode);

            if (srcNeuronGene != tgtNeuronGene && tgtNeuronGene.IsNodeRedundant())
                genotype.NodeGens.RemoveAt(tgtNeuronIdx);

            return connectionToDelete;
        }

        public void MatchingGenomes(Genotype genotype1, Genotype genotype2)
        {
            throw new Exception();
        }
    }
}
