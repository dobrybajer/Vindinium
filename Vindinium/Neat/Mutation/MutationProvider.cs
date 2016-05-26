﻿using System;
using Redzen.Numerics;
using System.Collections.Generic;
using System.Linq;
using vindinium.NEAT.Extensions;

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

            var sourceNode = RandomGenerator.Next(1, nodeNumber);
            var isInNodeInput = genotype.NodeGens[sourceNode - 1].Type == NodeType.Input;
            var stop = false;
            var targetNode = 0;
            while (!stop)
            {
                targetNode = RandomGenerator.Next(1, nodeNumber);
                if (isInNodeInput && genotype.NodeGens[targetNode - 1].Type != NodeType.Input && targetNode != sourceNode)
                    stop = true;
                else if (!isInNodeInput && targetNode != sourceNode)
                    stop = true;
            }

            if (genotype.NodeGens[sourceNode - 1].Type == NodeType.Output || genotype.NodeGens[targetNode - 1].Type == NodeType.Input)
            {
                var tmp = sourceNode;
                sourceNode = targetNode;
                targetNode = tmp;
            }

            var isConnection = IsConnectionInGenotype(sourceNode, targetNode, genotype);
            var isCycle = genotype.NodeGens.IsConnectionCyclic(sourceNode, targetNode);

            if (isConnection || isCycle) return genotype;

            var currentInnovation = innovation.LastOrDefault()?.InnovationNumber + 1 ?? 1;
            foreach (var el in innovation)
                if (el.InNode == sourceNode && el.OutNode == targetNode || el.InNode == targetNode && el.OutNode == sourceNode)
                    currentInnovation = el.InnovationNumber;

            if (currentInnovation == 1 || currentInnovation == innovation[innovation.Count - 1].InnovationNumber + 1)
            {
                innovation.Add(new Innovations
                {
                    InnovationNumber = currentInnovation,
                    InNode = sourceNode,
                    OutNode = targetNode
                });
            }
            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = sourceNode,
                OutNode = targetNode,
                Weight = (double)RandomGenerator.Next(0, 100) / 100,
                Status = ConnectionStatus.Enabled,
                Innovation = currentInnovation,
            });

            genotype.NodeGens[sourceNode].TargetNodes.Add(targetNode);
            genotype.NodeGens[targetNode].SourceNodes.Add(sourceNode);

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

            genotype.NodeGens[inNodeIdx].TargetNodes.Remove(outNodeIdx);
            genotype.NodeGens[outNodeIdx].SourceNodes.Remove(inNodeIdx);
            
            var newNodeGen = new NodeGenesModel
            {
                NodeNumber = genotype.NodeGens.Count,
                Type = NodeType.Hidden,
                TargetNodes = new HashSet<int> {outNodeIdx},
                SourceNodes = new HashSet<int> {inNodeIdx},
            };

            genotype.NodeGens[inNodeIdx].TargetNodes.Add(newNodeGen.NodeNumber);
            genotype.NodeGens[outNodeIdx].SourceNodes.Add(newNodeGen.NodeNumber);

            genotype.NodeGens.Add(newNodeGen);

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
            }

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = inNodeIdx,
                OutNode = newNodeGen.NodeNumber,
                Weight = (double)random.Next(0, 100) / 100,
                Status = ConnectionStatus.Enabled,
                Innovation = currentInnovaton,
            });

            currentInnovaton = innovation.LastOrDefault()?.InnovationNumber + 1 ?? 1;

            foreach (var el in innovation)
                if (el.InNode == newNodeGen.NodeNumber && el.OutNode == outNodeIdx || el.InNode == outNodeIdx && el.OutNode == newNodeGen.NodeNumber)
                    currentInnovaton = el.InnovationNumber;

            if (currentInnovaton == 1 || currentInnovaton == innovation[innovation.Count - 1].InnovationNumber + 1)
            {
                innovation.Add(new Innovations
                {
                    InnovationNumber = currentInnovaton,
                    InNode = newNodeGen.NodeNumber,
                    OutNode = outNodeIdx
                });
            }

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = newNodeGen.NodeNumber,
                OutNode = outNodeIdx,
                Weight = (double)random.Next(0, 100) / 100,
                Status = ConnectionStatus.Enabled,
                Innovation = currentInnovaton,
            });

            return genotype;
        }

        public Genotype MutateDeleteConnection(Genotype genotype)
        {
            var connectionNumber = genotype.GenomeConnection.Count;
            var random = new Random();
            var choosenConnectionId = random.Next(0, connectionNumber-1);

            genotype.GenomeConnection[choosenConnectionId].Status = ConnectionStatus.Disabled;

            var inNode = genotype.GenomeConnection[choosenConnectionId].InNode;
            var outNode = genotype.GenomeConnection[choosenConnectionId].OutNode;

            genotype.NodeGens[inNode].TargetNodes.Remove(outNode);
            genotype.NodeGens[outNode].SourceNodes.Remove(inNode);

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

        private static bool IsConnectionInGenotype(int inNode, int outNode, Genotype genotype)
        {
            return genotype.GenomeConnection.Any(el => el.InNode == inNode && el.OutNode == outNode || el.InNode == outNode && el.OutNode == inNode);
        }
    }
}
