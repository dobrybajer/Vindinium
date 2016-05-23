using System;
using System.Collections.Generic;
using System.Linq;

namespace vindinium.NEAT
{
    public class InitialGenomeBuilder
    {
        public Genotype CreateInitialGenotype(int inputNodesCount, int outputNodesCount)
        {
            var random = new Random();
            var inputNodes = new List<NodeGenesModel>();
            var outputNodes = new List<NodeGenesModel>();
            var connections = new List<ConnectionGenesModel>();

            for (var i = 0; i < inputNodesCount; i++)
            {
                var targetNodes = new HashSet<int>();
                for (var j = inputNodesCount; j < inputNodesCount + outputNodesCount; j++)
                {
                    connections.Add(new ConnectionGenesModel
                    {
                        InNode = i,
                        OutNode = j,
                        IsMutated = false,
                        Status = ConnectionStatus.Enabled,
                        Weight = (double) random.Next(0, 100)/100
                    });
                    targetNodes.Add(j);
                }

                var node = new NodeGenesModel
                {
                    NodeNumber = i,
                    Type = NodeType.Input,
                    SourceNodes = new HashSet<int>(),
                    TargetNodes = targetNodes
                };
                inputNodes.Add(node);
            }

            for (var i = inputNodesCount; i < outputNodesCount + inputNodesCount; i++)
            {
                var node = new NodeGenesModel
                {
                    NodeNumber = i,
                    Type = NodeType.Output,
                    SourceNodes = new HashSet<int>(inputNodes.Select(n => n.NodeNumber)),
                    TargetNodes = new HashSet<int>()
                };
                outputNodes.Add(node);
            }

            inputNodes.AddRange(outputNodes);

            var initialGenotype = new Genotype
            {
                NodeGens = inputNodes,
                GenomeConnection = connections
            };

            return initialGenotype;
        }

        public List<Innovations> InitInnovationList(int inputNodesCount, int outputNodesCount)
        {
            var innovationList = new List<Innovations>();
            var innovationCout = 1;

            for (int i = 0; i < inputNodesCount ; i++)
            {
                for (int j = inputNodesCount; j < outputNodesCount+ inputNodesCount; j++)
                {
                    innovationList.Add(
                            new Innovations()
                            {
                                InnovationNumber = innovationCout,
                                InNode=i,
                                OutNode=j
                            });
                    innovationCout++;
                }
            }

            return innovationList;
        }
    }
}