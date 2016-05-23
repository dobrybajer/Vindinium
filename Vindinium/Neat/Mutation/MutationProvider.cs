using System;
using Redzen.Numerics;
using System.Collections.Generic;


namespace vindinium.NEAT.Mutation
{
    public class MutationProvider : IMutationProvider
    {
        public Genotype Mutate(Genotype genotype, NodeGeneParameters nodeGeneParameters,List<Innovations> innovations)
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
                        mutatedGenotype = MutateAddNode(genotype, innovations); //zmienić
                        break;
                    case 1:
                        mutatedGenotype = MutateAddConnection(genotype, innovations);//Zmienić 
                        break;
                    case 2:
                        mutatedGenotype = MutateDeleteConnection(genotype);
                        break;
                    case 3:
                        mutatedGenotype = ChangeWeight(genotype);
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

        public Genotype MutateAddConnection(Genotype genotype, List<Innovations> innovation)
        {
            var nodeNumber = genotype.NodeGens.Count;

            var random = new Random();
            var inNode = random.Next(1, nodeNumber);
            var isInNodeInput = genotype.NodeGens[inNode - 1].Type == NodeType.Input ? true : false;
            var stop = false;
            var outNode = 0;
            while (!stop)
            {
                outNode = random.Next(1, nodeNumber + 1);
                if (isInNodeInput && genotype.NodeGens[outNode - 1].Type != NodeType.Input && outNode != inNode)
                    stop = true;
                else if (!isInNodeInput && outNode != inNode)
                    stop = true;
            }

            if (inNode > outNode)
            {
                var tmp = inNode;
                inNode = outNode;
                outNode = tmp;
            }
            var isConnection = isConnectionInGenotype(inNode, outNode, genotype);
            var currentInnovaton = 0;

            if (!isConnection)
            {
                foreach (var el in innovation)
                    if (el.InNode == inNode && el.OutNode == outNode || el.InNode == outNode && el.OutNode == inNode)
                        currentInnovaton = el.InnovationNumber;

                currentInnovaton = innovation.Count == 0 ? 0 : innovation[innovation.Count - 1].InnovationNumber;

               
                innovation.Add(new Innovations
                {
                    InnovationNumber = currentInnovaton + 1,
                    InNode = inNode,
                    OutNode = outNode
                });

                genotype.GenomeConnection.Add(new ConnectionGenesModel
                {
                    InNode = inNode,
                    OutNode = outNode,
                    Weight = (double)random.Next(0, 100) / 100,
                    Status = ConnectionStatus.Enabled,
                    Innovation = currentInnovaton + 1,
                });

                genotype.NodeGens[inNode-1].TargetNodes.Add(outNode-1);
                genotype.NodeGens[outNode-1].SourceNodes.Add(inNode-1);
            }

            return genotype;

        }

        public Genotype MutateAddNode(Genotype genotype, List<Innovations> innovation)
        {
            var connectionNumber = genotype.GenomeConnection.Count;

            var random = new Random();
            var stop = false;
            var chooseConnection = 0;
            while (!stop)
            {
                chooseConnection = random.Next(1, connectionNumber);
                if (genotype.GenomeConnection[chooseConnection].Status == ConnectionStatus.Enabled)
                    stop = true;
            }
            genotype.GenomeConnection[chooseConnection].Status = ConnectionStatus.Disabled;

            var inNodeIdx = genotype.GenomeConnection[chooseConnection].InNode;
            var outNodeIdx = genotype.GenomeConnection[chooseConnection].OutNode;

            var newNodeGen = new NodeGenesModel
            {
                NodeNumber = genotype.NodeGens.Count + 1,
                Type = NodeType.Hidden,
                TargetNodes = new HashSet<int>(),
                SourceNodes = new HashSet<int>(),
            };

            genotype.NodeGens.Add(newNodeGen);

            var currentInnovaton = 0;

            foreach (var el in innovation)
                if (el.InNode == inNodeIdx && el.OutNode == newNodeGen.NodeNumber || el.InNode == newNodeGen.NodeNumber && el.OutNode == inNodeIdx)
                    currentInnovaton = el.InnovationNumber;


            currentInnovaton = innovation.Count == 0 ? 1 : innovation[innovation.Count - 1].InnovationNumber + 1;

            innovation.Add(new Innovations
            {
                InnovationNumber = currentInnovaton,
                InNode = inNodeIdx,
                OutNode = newNodeGen.NodeNumber
            });
            innovation.Add(new Innovations
            {
                InnovationNumber = currentInnovaton + 1,
                InNode = newNodeGen.NodeNumber,
                OutNode = outNodeIdx
            });


            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = inNodeIdx,
                OutNode = newNodeGen.NodeNumber,
                Weight = (double)random.Next(0, 100) / 100,
                Status = ConnectionStatus.Enabled,
                Innovation = currentInnovaton,
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = genotype.NodeGens.Count,
                OutNode = genotype.GenomeConnection[chooseConnection].OutNode,
                Weight = (double)random.Next(0, 100) / 100,
                Status = ConnectionStatus.Enabled,
                Innovation = currentInnovaton + 1,
            });

            genotype.NodeGens[inNodeIdx-1].TargetNodes.Add(newNodeGen.NodeNumber-1);
            genotype.NodeGens[outNodeIdx-1].SourceNodes.Add(newNodeGen.NodeNumber-1);
            var tmp = newNodeGen.NodeNumber - 1;
            genotype.NodeGens[tmp].TargetNodes.Add(outNodeIdx-1);
            genotype.NodeGens[tmp].SourceNodes.Add(inNodeIdx-1);

            return genotype;
        }


        private Genotype MutateDeleteConnection(Genotype genotype)
        {
            var connectionNumber = genotype.GenomeConnection.Count;
            var random = new Random();
            var choosenConnectionId = random.Next(1, connectionNumber);

            genotype.GenomeConnection[choosenConnectionId].Status = ConnectionStatus.Disabled;

            var inNode = genotype.GenomeConnection[choosenConnectionId].InNode;
            var outNode = genotype.GenomeConnection[choosenConnectionId].InNode;

            genotype.NodeGens[inNode-1].TargetNodes.Remove(outNode-1);
            genotype.NodeGens[outNode-1].SourceNodes.Remove(inNode-1);
            
            return genotype;
        }

        private Genotype ChangeWeight(Genotype genotype)
        {
            var connectionNumber = genotype.GenomeConnection.Count;
            var random = new Random();
            var choosenConnectionId = random.Next(1, connectionNumber);

            genotype.GenomeConnection[choosenConnectionId].Weight = (double)random.Next(0, 100) / 100;
            return genotype;
        }

        public bool isConnectionInGenotype(int inNode,int outNode, Genotype genotype)
        {
            var isConnection = false;
            foreach (var el in genotype.GenomeConnection)
                if (el.InNode == inNode && el.OutNode == outNode || el.InNode == outNode && el.OutNode == inNode)
                     return isConnection = true;
            return isConnection;
        }

      
    }
}
