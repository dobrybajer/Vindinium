using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vindinium.NEAT.Extensions
{
    public static class NodeListExtensions
    {
        public static bool IsConnectionCyclic(this List<NodeGenesModel> nodeGenesModels, int sourceNodeId,
            int targetNodeId)
        {
            if (sourceNodeId == targetNodeId)
                return true;
            var sourceNode = nodeGenesModels.Find(n => n.NodeNumber == sourceNodeId);

            var visitedNeurons = new HashSet<int>();
            visitedNeurons.Add(sourceNodeId);

            var workStack = new Stack<int>();
            foreach (var neuronId in sourceNode.SourceNodes)
                workStack.Push(neuronId);

            while (workStack.Any())
            {
                var currentNodeId = workStack.Pop();
                if (visitedNeurons.Contains(currentNodeId))
                    continue;

                if (currentNodeId == targetNodeId)
                    return true;

                visitedNeurons.Add(currentNodeId);

                var currentNode = nodeGenesModels.Find(n => n.NodeNumber == currentNodeId);
                foreach (var nodeId in currentNode.SourceNodes)
                    workStack.Push(nodeId);
            }
            return false;
        }
    }
}
