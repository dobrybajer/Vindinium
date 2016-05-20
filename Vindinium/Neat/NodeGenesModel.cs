using System.Collections.Generic;

namespace vindinium.NEAT
{
    internal class NodeGenesModel
    {
        public int NodeNumber { get; set; }

        public NodeType Type { get; set; }

        public HashSet<int> SourceNodes { get; set; }
            
        public HashSet<int> TargetNodes { get; set; }

        public NodeGenesModel() { }

        public NodeGenesModel(int nodeNumber, NodeType type)
        {
            NodeNumber = nodeNumber;
            Type = type;
        }
    }
}
