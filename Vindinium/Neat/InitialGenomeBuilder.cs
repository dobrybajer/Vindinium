using System;
using System.Collections.Generic;

namespace vindinium.NEAT
{
    public class InitialGenomeBuilder
    {
        public Genotype CreateInitialGenotype(int inputNodesCount, int outputNodesCount)
        {
            var random = new Random();
            var initialNodes = new List<NodeGenesModel>();
            var connections = new List<ConnectionGenesModel>();
            for (var i = 0; i < inputNodesCount; i++)
            {
                var sourceNodes = new HashSet<int>();
                for (var j = inputNodesCount; j < inputNodesCount + outputNodesCount; j++)
                {
                    connections.Add(new ConnectionGenesModel {InNode = i, OutNode = j, Weight = (double)random.Next(0, 100) / 100 });
                    sourceNodes.Add(j);
                }
                var node = new NodeGenesModel {NodeNumber = i, Type = NodeType.Input, SourceNodes = sourceNodes};
                initialNodes.Add(node);
            }
            for (var i = inputNodesCount; i < outputNodesCount + inputNodesCount; i++)
            {
                var node = new NodeGenesModel {NodeNumber = i, Type = NodeType.Output, TargetNodes = new HashSet<int>() };
                initialNodes.Add(node);
            }

            var initialGenotype = new Genotype
            {
                NodeGens = initialNodes,
                GenomeConnection = connections
            };
            return initialGenotype;
        }
    }
}