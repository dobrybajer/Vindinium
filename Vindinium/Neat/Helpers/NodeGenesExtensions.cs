namespace vindinium.NEAT.Helpers
{
    public static class NodeGenesExtensions
    {
        public static bool IsNodeRedundant(this NodeGenesModel neuronGene)
        {
            return neuronGene.Type == NodeType.Hidden && 0 == neuronGene.SourceNodes.Count + neuronGene.TargetNodes.Count;
        }
    }
}
