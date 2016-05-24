using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redzen.Numerics;
using vindinium.NEAT.Crossover;
using vindinium.NEAT.Mutation;
using vindinium.Singletons;

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
            CreateNodeGeneParameters();
        }

        private void CreateNodeGeneParameters()
        {
            NodeGeneParameters = new NodeGeneParameters
            {
                AddConnectionMutationProbability = Parameters.AddConnectionMutationProbablity,
                AddNodeMutationProbability = Parameters.AddNodeMutationProbablity,
                DeleteConnectionMutationProbability = Parameters.DeleteConnectionMutationProbablity,
                ConnectionWeightMutationProbability = Parameters.ConnectionWeightMutationProbablity
            };
        }

        public List<Genotype> CreateNewPopulationWithMutation(List<Genotype> genotypes,ref List<Innovations> innovationsList)
        {
            var outputPopulation = new List<Genotype>();
            var random = new XorShiftRandom();
            var maxValue = genotypes.Max(g => g.Value);
            var probabilities = new List<double>(genotypes.Count);
            probabilities.AddRange(genotypes.Select(genotype => genotype.Value/maxValue));

            var roulette = new DiscreteDistribution(probabilities.ToArray());

            var attemptsCount = genotypes.Count / Parameters.MutationWheelPart;
            var mutatedGenomesId = new List<int>();
            for (int i = 0; i < attemptsCount; i++)
            {
                var genomeId = DiscreteDistributionUtils.Sample(roulette, random);
                roulette.RemoveOutcome(genomeId);
                mutatedGenomesId.Add(genomeId);
                outputPopulation.Add(mutationProvider.Mutate(genotypes[genomeId], NodeGeneParameters,ref innovationsList));
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

            var attemptsCount = genotypes.Count / Parameters.CrossoverWheelPart;
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
