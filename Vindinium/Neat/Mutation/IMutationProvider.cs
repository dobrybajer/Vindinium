using System.Collections.Generic;

namespace vindinium.NEAT.Mutation
{
    public interface IMutationProvider
    {
        Genotype Mutate(Genotype genotype, NodeGeneParameters nodeGeneParameters,List<Innovations> innovations);
    }
}