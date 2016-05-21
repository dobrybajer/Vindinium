using System.Collections.Generic;

namespace vindinium.NEAT
{
    internal class NodeGenesModel
    {
        public int NodeNumber { get; set; }

        public NodeType Type { get; set; }

        public int Level { get; set; }

        public double FeedForwardValue { get; set; }

        public HashSet<int> SourceNodes { get; set; }
            
        public HashSet<int> TargetNodes { get; set; }

        public NodeGenesModel() { }

        public NodeGenesModel(int nodeNumber, NodeType type, int level, double feedForwardVelue)
        {
            NodeNumber = nodeNumber;
            Type = type;
            Level = level;
            FeedForwardValue = feedForwardVelue;
        }
    }
}
