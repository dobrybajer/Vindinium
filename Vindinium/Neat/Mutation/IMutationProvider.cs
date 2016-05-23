using System.Collections.Generic;

namespace vindinium.NEAT.Mutation
{
    public interface IMutationProvider
    {
        Genotype Mutate(Genotype genotype, NodeGeneParameters nodeGeneParameters,ref List<Innovations> innovations);
    }
}