using System.Collections.Generic;

namespace vindinium.NEAT
{
    public class NodeGenesModel
    {
        public int NodeNumber { get; set; }

        public NodeType Type { get; set; }

        public double FeedForwardValue { get; set; }

        public int FeedForwardCount { get; set; }

        public HashSet<int> SourceNodes { get; set; }
            
        public HashSet<int> TargetNodes { get; set; }

        public NodeGenesModel() { }

        public NodeGenesModel DeepCopy()
        {
            return new NodeGenesModel
            {
                NodeNumber = this.NodeNumber,
                Type = this.Type,
                FeedForwardValue = this.FeedForwardValue,
                FeedForwardCount = this.FeedForwardCount,
                SourceNodes = this.SourceNodes != null ? new HashSet<int>(this.SourceNodes) : null,
                TargetNodes = this.TargetNodes != null ? new HashSet<int>(this.TargetNodes) : null
            };
        }

        public NodeGenesModel(int nodeNumber, NodeType type, double feedForwardVelue)
        {
            NodeNumber = nodeNumber;
            Type = type;
            FeedForwardValue = feedForwardVelue;
        }
    }
}
