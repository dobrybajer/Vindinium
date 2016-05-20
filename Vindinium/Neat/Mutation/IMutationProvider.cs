namespace vindinium.NEAT.Mutation
{
    internal interface IMutationProvider
    {
        Genotype Mutate(Genotype genotype, NodeGeneParameters nodeGeneParameters);
    }
}