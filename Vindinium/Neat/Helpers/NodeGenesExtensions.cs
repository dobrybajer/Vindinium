namespace vindinium.NEAT.Helpers
{
    internal static class NodeGenesExtensions
    {
        public static bool IsNodeRedundant(this NodeGenesModel neuronGene)
        {
            return neuronGene.Type == NodeType.Hidden && 0 == neuronGene.SourceNodes.Count + neuronGene.TargetNodes.Count;
        }
    }
}
