using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vindinium.NEAT.Helpers
{
    internal static class NodeGenesExtensions
    {
        public static bool IsNodeRedundant(this NodeGenesModel neuronGene)
        {
            if (neuronGene.Type != NodeType.Hidden)
                return false;
            return (0 == (neuronGene.SourceNodes.Count + neuronGene.TargetNodes.Count));
        }
    }
}
