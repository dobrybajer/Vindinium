using System;
using System.Diagnostics;
using Redzen.Numerics;
using vindinium.NEAT.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace vindinium.NEAT.Mutation
{
    public class MutationProvider : IMutationProvider
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
                        mutatedGenotype = MutateAddNode(genotype, null); //zmienić
                        break;
                    case 1:
                        mutatedGenotype = MutateAddConnection(genotype,null);//Zmienić 
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

        private Genotype MutateAddConnection(Genotype genotype, List<Innovationcs> innovation)
        {
            var nodeNumber = genotype.NodeGens.Count;

            var random = new Random();
            var inNode = random.Next(1, nodeNumber + 1); 
            var outNode = random.Next(1, nodeNumber + 1);

            ChangeLevel(inNode, outNode, genotype);

            var isConnection = isConnectionInGenotype(inNode,outNode,genotype);
            var currentInnovaton = 0;

            if (!isConnection)
                foreach (var el in innovation)
                    if (el.InNode == inNode && el.OutNode == outNode || el.InNode == outNode && el.OutNode == inNode)
                        currentInnovaton = el.InnovationNumber;
            

            if (!isConnection && currentInnovaton!=0)
            {
                genotype.GenomeConnection.Add(new ConnectionGenesModel
                {
                    InNode = inNode,
                    OutNode = outNode,
                    Weight = (double)random.Next(0, 100) / 100,
                    Status = ConnectionStatus.Enabled,
                    Innovation = currentInnovaton,
                });
            }
            else
            {
                innovation.Add(new Innovationcs
                {
                    InnovationNumber = innovation[innovation.Count - 1].InnovationNumber + 1,
                    InNode =inNode,
                    OutNode =outNode
                });

                genotype.GenomeConnection.Add(new ConnectionGenesModel
                {
                    InNode = inNode,
                    OutNode = outNode,
                    Weight = (double)random.Next(0, 100) / 100,
                    Status = ConnectionStatus.Enabled,
                    Innovation = innovation[innovation.Count-1].InnovationNumber,
                });
            }
                   
            return genotype;

        }

        private Genotype MutateAddNode(Genotype genotype, List<Innovationcs> innovation)
        {
            var connectionNumber = genotype.GenomeConnection.Count;

            var random = new Random();
            var chooseConnection = random.Next(1, connectionNumber);
            genotype.GenomeConnection[chooseConnection].Status = ConnectionStatus.Disabled;

            var inNodeIdx = genotype.GenomeConnection[chooseConnection].InNode;
            var outNodeIdx = genotype.GenomeConnection[chooseConnection].OutNode;
            var lowerLevel = Math.Min(genotype.NodeGens[inNodeIdx].Level, genotype.NodeGens[outNodeIdx].Level);

            var newNodeGen = new NodeGenesModel
            {
                NodeNumber = genotype.NodeGens.Count + 1,
                Type = NodeType.Hidden,
                Level = lowerLevel + 1
            };

            if (newNodeGen.Level == genotype.NodeGens[outNodeIdx].Level)
                genotype.NodeGens[outNodeIdx].Level++;

            genotype.NodeGens.Add(newNodeGen);

            innovation.Add(new Innovationcs
            {
                InnovationNumber = innovation[innovation.Count - 1].InnovationNumber + 1,
                InNode = inNodeIdx,
                OutNode = newNodeGen.NodeNumber
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = inNodeIdx,
                OutNode = newNodeGen.NodeNumber,
                Weight = (double)random.Next(0, 100) / 100,
                Status = ConnectionStatus.Enabled,
                Innovation = genotype.GetCurrentInnovation() + 1,
            });

            innovation.Add(new Innovationcs
            {
                InnovationNumber = innovation[innovation.Count - 1].InnovationNumber + 1,
                InNode = newNodeGen.NodeNumber,
                OutNode = outNodeIdx
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
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

            var outNode = genotype.GenomeConnection[choosenConnectionId].OutNode;
            var currLevel = genotype.NodeGens.Where(w => w.Level - 1 == genotype.NodeGens[outNode].Level).Select(s=> s.NodeNumber);

            var isPossible = false;

            for (int i = 0; i < genotype.GenomeConnection.Count; i++)
                if (genotype.GenomeConnection[i].OutNode == outNode && currLevel.Contains(genotype.GenomeConnection[i].InNode) &&
                    choosenConnectionId != i)
                    isPossible = true;
            

            if(isPossible)
                genotype.GenomeConnection[choosenConnectionId].Status = ConnectionStatus.Disabled;
            return genotype;
        }

        public void MatchingGenomes(Genotype genotype1, Genotype genotype2)
        {
             //Dopisać funkcje zmieniającą pojedyncze wagi krawidzi
            throw new Exception();
        }

        public bool isConnectionInGenotype(int inNode,int outNode, Genotype genotype)
        {
            var isConnection = false;
            foreach (var el in genotype.GenomeConnection)
                if (el.InNode == inNode && el.OutNode == outNode || el.InNode == outNode && el.OutNode == inNode)
                     return isConnection = true;
            return isConnection;
        }

        public void ChangeLevel(int inNode, int outNode, Genotype genotype)
        {
            var random = new Random();
            if (genotype.NodeGens[inNode].Level == genotype.NodeGens[outNode].Level)
            {
                if (random.Next(0, 2) == 0)
                    genotype.NodeGens[inNode].Level++;
                else
                    genotype.NodeGens[outNode].Level++;
            }
        }
    }
}
