namespace vindinium.NEAT.Crossover
{
    public interface ICrossoverProvider
    {
        Genotype CrossoverGenotype(Genotype genotype1, Genotype genotype2);
    }
}