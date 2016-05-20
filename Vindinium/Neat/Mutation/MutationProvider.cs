using System;
using System.Diagnostics;
using Redzen.Numerics;
using vindinium.NEAT.Helpers;

namespace vindinium.NEAT.Mutation
{
    internal class MutationProvider : IMutationProvider
    {
        public Genotype Mutate(Genotype genotype, NodeGeneParameters nodeGeneParameters)
        {
            var xorRandom = new XorShiftRandom();
            var rouletteWheelLayoutInitial = (genotype.GenomeConnection.Count < 2) ?
                  nodeGeneParameters.RouletteWheelLayoutNonDestructive
                : nodeGeneParameters.RouletteWheelLayout;

            var rouletteWheelLayoutCurrent = rouletteWheelLayoutInitial;
            Genotype mutatedGenotype = null;
            while (mutatedGenotype == null)
            {
                var outcome = DiscreteDistributionUtils.Sample(rouletteWheelLayoutCurrent, xorRandom);
                switch (outcome)
                {
                    case 0:
                        mutatedGenotype = MutateAddNode(genotype);
                        break;
                    case 1:
                        mutatedGenotype = MutateAddConnection(genotype);
                        break;
                    case 2:
                        mutatedGenotype = MutateDeleteConnection(genotype);
                        break;
                    default:
                        throw new ArgumentException(nameof(outcome));
                }
                if(mutatedGenotype == null && OnMutationFailed(rouletteWheelLayoutCurrent, outcome)) return null;
            }
            return mutatedGenotype;
        }

        private bool OnMutationFailed(DiscreteDistribution rouletteWheelLayoutCurrent, int outcome)
        {
            rouletteWheelLayoutCurrent = rouletteWheelLayoutCurrent.RemoveOutcome(outcome);
            if (0 == rouletteWheelLayoutCurrent.Probabilities.Length)
                return true;
            return false;
        }

        private Genotype MutateAddConnection(Genotype genotype)
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

        private Genotype MutateAddNode(Genotype genotype)
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


        private Genotype MutateDeleteConnection(Genotype genotype)
        {
            var connectionNumber = genotype.GenomeConnection.Count;
            var random = new Random();
            var choosenConnectionId = random.Next(1, connectionNumber);

            genotype.GenomeConnection[choosenConnectionId].Status = ConnectionStatus.Disabled;
            return genotype;
        }

        public void MatchingGenomes(Genotype genotype1, Genotype genotype2)
        {
            throw new Exception();
        }
    }
}
