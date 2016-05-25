using System;
using Redzen.Numerics;
using System.Collections.Generic;
using System.Linq;

namespace vindinium.NEAT.Mutation
{
    public class MutationProvider : IMutationProvider
    {
        public IRandomGenerator RandomGenerator { get; set; } = new RandomGenerator();

        public Genotype Mutate(Genotype genotype, NodeGeneParameters nodeGeneParameters, ref List<Innovations> innovations)
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
                        mutatedGenotype = MutateAddNode(genotype, ref innovations); 
                        break;
                    case 1:
                        mutatedGenotype = MutateAddConnection(genotype, ref innovations); 
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
                if (mutatedGenotype == null && OnMutationFailed(rouletteWheelLayoutCurrent, outcome))
                    throw new ArgumentException($"Empty mutated genotype (null) from case: {outcome}");
            }
            return mutatedGenotype;
        }

        private bool OnMutationFailed(DiscreteDistribution rouletteWheelLayoutCurrent, int outcome)
        {
            rouletteWheelLayoutCurrent = rouletteWheelLayoutCurrent.RemoveOutcome(outcome);
            return 0 == rouletteWheelLayoutCurrent.Probabilities.Length;
        }

        public Genotype MutateAddConnection(Genotype genotype, ref List<Innovations> innovation)
        {
            var nodeNumber = genotype.NodeGens.Count;

            var inNode = RandomGenerator.Next(1, nodeNumber);
            var isInNodeInput = genotype.NodeGens[inNode - 1].Type == NodeType.Input;
            var stop = false;
            var outNode = 0;
            while (!stop)
            {
                outNode = RandomGenerator.Next(1, nodeNumber);
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

            var isConnection = IsConnectionInGenotype(inNode, outNode, genotype);

            if (isConnection) return genotype;

            //var currentInnovaton = innovation.Count == 0 ? 0 : innovation[innovation.Count - 1].InnovationNumber + 1;
            var currentInnovaton = innovation.LastOrDefault()?.InnovationNumber + 1 ?? 1;
            foreach (var el in innovation)
                if (el.InNode == inNode && el.OutNode == outNode || el.InNode == outNode && el.OutNode == inNode)
                    currentInnovaton = el.InnovationNumber;

            if (currentInnovaton == 1 || currentInnovaton == innovation[innovation.Count - 1].InnovationNumber + 1)
            {
                innovation.Add(new Innovations
                {
                    InnovationNumber = currentInnovaton,
                    InNode = inNode,
                    OutNode = outNode
                });
            }
            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = inNode,
                OutNode = outNode,
                Weight = (double)RandomGenerator.Next(0, 100) / 100,
                Status = ConnectionStatus.Enabled,
                Innovation = currentInnovaton,
            });

            //var sourceNode = genotype.NodeGens.Where(node => node.NodeNumber == inNode).Select(s => s.NodeNumber).ToList();
            //var targetNode = genotype.NodeGens.Where(node => node.NodeNumber == outNode).Select(s => s.NodeNumber).ToList();
            genotype.NodeGens[inNode].TargetNodes.Add(outNode);
            genotype.NodeGens[outNode].SourceNodes.Add(inNode);
            //genotype.NodeGens[sourceNode[0]].TargetNodes.Add(outNode);
            //genotype.NodeGens[targetNode[0]].SourceNodes.Add(inNode);

            return genotype;
        }

        public Genotype MutateAddNode(Genotype genotype, ref List<Innovations> innovation)
        {
            var connectionNumber = genotype.GenomeConnection.Count - 1;

            var random = new Random();
            var stop = false;
            var chooseConnection = 0;
            while (!stop)
            {
                chooseConnection = random.Next(0, connectionNumber);
                if (genotype.GenomeConnection[chooseConnection].Status == ConnectionStatus.Enabled)
                    stop = true;
            }            

            genotype.GenomeConnection[chooseConnection].Status = ConnectionStatus.Disabled;

            var inNodeIdx = genotype.GenomeConnection[chooseConnection].InNode;
            var outNodeIdx = genotype.GenomeConnection[chooseConnection].OutNode;

            //var sourceNode = genotype.NodeGens.Where(node => node.NodeNumber == inNodeIdx).Select(s => s.NodeNumber).ToList();
            //var targetNode = genotype.NodeGens.Where(node => node.NodeNumber == outNodeIdx).Select(s => s.NodeNumber).ToList();

            //genotype.NodeGens[sourceNode[0]].TargetNodes.Remove(outNodeIdx);
            //genotype.NodeGens[targetNode[0]].SourceNodes.Remove(inNodeIdx);
            genotype.NodeGens[inNodeIdx].TargetNodes.Remove(outNodeIdx);
            genotype.NodeGens[outNodeIdx].SourceNodes.Remove(inNodeIdx);

            var newNodeGen = new NodeGenesModel
            {
                NodeNumber = genotype.NodeGens.Count,
                Type = NodeType.Hidden,
                TargetNodes = new HashSet<int> {outNodeIdx},
                SourceNodes = new HashSet<int> {inNodeIdx},
            };

            genotype.NodeGens.Add(newNodeGen);

            //var currentInnovaton = innovation.Count == 0 ? 1 : innovation[innovation.Count - 1].InnovationNumber + 1;
            var currentInnovaton = innovation.LastOrDefault()?.InnovationNumber + 1 ?? 1;

            foreach (var el in innovation)
                if (el.InNode == inNodeIdx && el.OutNode == newNodeGen.NodeNumber || el.InNode == newNodeGen.NodeNumber && el.OutNode == inNodeIdx)
                    currentInnovaton = el.InnovationNumber;

            if (currentInnovaton == 1 || currentInnovaton == innovation[innovation.Count - 1].InnovationNumber + 1)
            {
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
            }

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
                InNode = newNodeGen.NodeNumber,
                OutNode = outNodeIdx,
                Weight = (double)random.Next(0, 100) / 100,
                Status = ConnectionStatus.Enabled,
                Innovation = currentInnovaton + 1,
            });

            genotype.NodeGens[inNodeIdx].TargetNodes.Add(newNodeGen.NodeNumber);
            genotype.NodeGens[outNodeIdx].SourceNodes.Add(newNodeGen.NodeNumber);
            //var tmp = newNodeGen.NodeNumber;
            //genotype.NodeGens[tmp].TargetNodes.Add(outNodeIdx);
            //genotype.NodeGens[tmp].SourceNodes.Add(inNodeIdx);

            return genotype;
        }

        private Genotype MutateDeleteConnection(Genotype genotype)
        {
            var connectionNumber = genotype.GenomeConnection.Count;
            var random = new Random();
            var choosenConnectionId = random.Next(1, connectionNumber);

            genotype.GenomeConnection[choosenConnectionId - 1].Status = ConnectionStatus.Disabled;

            var inNode = genotype.GenomeConnection[choosenConnectionId].InNode;
            var outNode = genotype.GenomeConnection[choosenConnectionId].OutNode;

            //var sourceNode = genotype.NodeGens.Where(node => node.NodeNumber == inNode).Select(s => s.NodeNumber).ToList();
            //var targetNode = genotype.NodeGens.Where(node => node.NodeNumber == outNode).Select(s => s.NodeNumber).ToList();

            genotype.NodeGens[inNode].TargetNodes.Remove(outNode);
            genotype.NodeGens[outNode].SourceNodes.Remove(inNode);
            //genotype.NodeGens[sourceNode[0]].TargetNodes.Remove(outNode);
            //genotype.NodeGens[targetNode[0]].SourceNodes.Remove(inNode);

            return genotype;
        }

        private Genotype ChangeWeight(Genotype genotype)
        {
            var connectionNumber = genotype.GenomeConnection.Count;
            var random = new Random();
            var choosenConnectionId = random.Next(1, connectionNumber);

            genotype.GenomeConnection[choosenConnectionId - 1].Weight = (double)random.Next(0, 100) / 100;
            return genotype;
        }

        private bool IsConnectionInGenotype(int inNode, int outNode, Genotype genotype)
        {
            foreach (var el in genotype.GenomeConnection)
                if (el.InNode == inNode && el.OutNode == outNode || el.InNode == outNode && el.OutNode == inNode)
                    return true;
            return false;
        }


    }
}
