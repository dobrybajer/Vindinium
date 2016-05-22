using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redzen.Numerics;
using vindinium.NEAT.Crossover;
using vindinium.NEAT.Mutation;

namespace vindinium.NEAT
{
    public class NeatGeneticAlgorithm
    {
        public NodeGeneParameters NodeGeneParameters { get; set; }
        private readonly ICrossoverProvider crossoverProvider;
        private readonly IMutationProvider mutationProvider;

        public NeatGeneticAlgorithm(ICrossoverProvider crossoverProvider, IMutationProvider mutationProvider)
        {
            this.crossoverProvider = crossoverProvider;
            this.mutationProvider = mutationProvider;
        }

        public List<Genotype> CreateNewPopulationWithMutation(List<Genotype> genotypes)
        {
            var outputPopulation = new List<Genotype>();
            var random = new XorShiftRandom();
            var maxValue = genotypes.Max(g => g.Value);
            var probabilities = new List<double>(genotypes.Count);
            probabilities.AddRange(genotypes.Select(genotype => genotype.Value/maxValue));

            var roulette = new DiscreteDistribution(probabilities.ToArray());

            var attemptsCount = genotypes.Count / 2;
            var mutatedGenomesId = new List<int>();
            for (int i = 0; i < attemptsCount; i++)
            {
                var genomeId = DiscreteDistributionUtils.Sample(roulette, random);
                roulette.RemoveOutcome(genomeId);
                mutatedGenomesId.Add(genomeId);
                //outputPopulation.Add(mutationProvider.Mutate(genotypes[genomeId], NodeGeneParameters));
            }

            for (int i = 0; i < genotypes.Count; i++)
            {
                var genotype = genotypes[i];
                if (!mutatedGenomesId.Contains(i))
                    outputPopulation.Add(genotype);
            }

            return outputPopulation;
        }

        public List<Genotype> CreateNewPopulationWithCrossover(List<Genotype> genotypes)
        {
            var outputPopulation = new List<Genotype>();
            var random = new XorShiftRandom();
            var maxValue = genotypes.Max(g => g.Value);
            var probabilities = new List<double>(genotypes.Count);
            probabilities.AddRange(genotypes.Select(genotype => genotype.Value / maxValue));

            var roulette = new DiscreteDistribution(probabilities.ToArray());

            var attemptsCount = genotypes.Count / 4;
            var crossoveredGenomesId = new List<int>();
            for (int i = 0; i < attemptsCount; i++)
            {
                var parentOneId = DiscreteDistributionUtils.Sample(roulette, random);
                roulette.RemoveOutcome(parentOneId);
                var parentTwoId = DiscreteDistributionUtils.Sample(roulette, random);
                roulette.RemoveOutcome(parentTwoId);
                crossoveredGenomesId.Add(parentOneId);
                crossoveredGenomesId.Add(parentTwoId);
                outputPopulation.Add(crossoverProvider.CrossoverGenotype(genotypes[parentOneId], genotypes[parentTwoId]));
            }

            for (int i = 0; i < genotypes.Count; i++)
            {
                var genotype = genotypes[i];
                if (!crossoveredGenomesId.Contains(i))
                    outputPopulation.Add(genotype);
            }

            return outputPopulation;
        }
    }
}
