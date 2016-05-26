using System.Collections.Generic;
using System.Linq;
using Redzen.Numerics;
using vindinium.NEAT.Crossover;
using vindinium.NEAT.Mutation;
using vindinium.Singletons;

namespace vindinium.NEAT
{
    public class NeatGeneticAlgorithm
    {
        public NodeGeneParameters NodeGeneParameters { get; set; }
        private readonly ICrossoverProvider _crossoverProvider;
        private readonly IMutationProvider _mutationProvider;

        public NeatGeneticAlgorithm(ICrossoverProvider crossoverProvider, IMutationProvider mutationProvider)
        {
            _crossoverProvider = crossoverProvider;
            _mutationProvider = mutationProvider;
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

        public List<Genotype> CreateNewPopulationWithMutation(List<Genotype> genotypes, ref List<Innovations> innovationsList)
        {
            var outputPopulation = new List<Genotype>();
            var random = new XorShiftRandom();
            var maxValue = genotypes.Max(g => g.Value);
            var probabilities = new List<double>(genotypes.Count);
            probabilities.AddRange(genotypes.Select(genotype => genotype.Value / maxValue));

            var roulette = new DiscreteDistribution(probabilities.ToArray());

            var attemptsCount = genotypes.Count / Parameters.MutationWheelPart;
            var mutatedGenomesId = new List<int>();
            for (var i = 0; i < attemptsCount; i++)
            {
                var genomeId = DiscreteDistributionUtils.Sample(roulette, random);
                roulette = roulette.RemoveOutcome(genomeId);
                mutatedGenomesId.Add(genomeId);
                outputPopulation.Add(_mutationProvider.Mutate(genotypes[genomeId], NodeGeneParameters, ref innovationsList));
            }

            outputPopulation.AddRange(genotypes.Where((genotype, i) => !mutatedGenomesId.Contains(i)));

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
            for (var i = 0; i < attemptsCount; i++)
            {
                var parentOneId = DiscreteDistributionUtils.Sample(roulette, random);
                roulette = roulette.RemoveOutcome(parentOneId);
                var parentTwoId = DiscreteDistributionUtils.Sample(roulette, random);
                roulette = roulette.RemoveOutcome(parentTwoId);

                crossoveredGenomesId.Add(genotypes[parentOneId].Value < genotypes[parentTwoId].Value
                    ? parentOneId
                    : parentTwoId);
                outputPopulation.Add(_crossoverProvider.CrossoverGenotype(genotypes[parentOneId], genotypes[parentTwoId]));
            }

            outputPopulation.AddRange(genotypes.Where((genotype, i) => !crossoveredGenomesId.Contains(i)));

            return outputPopulation;
        }
    }
}
