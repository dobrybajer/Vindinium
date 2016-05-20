using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redzen.Numerics;

namespace vindinium.NEAT.Mutation
{
    public class NodeGeneParameters
    {
        private double addNodeMutationProbability;

        public double AddNodeMutationProbability
        {
            get
            {
                return addNodeMutationProbability;
            }
            set
            {
                addNodeMutationProbability = value;
                CreateRouletteLayouts();
            }
        }

        private double addConnectionMutationProbability;
        public double AddConnectionMutationProbability
        {
            get
            {
                return addConnectionMutationProbability;
            }
            set
            {
                addConnectionMutationProbability = value;
                CreateRouletteLayouts();
            }
        }

        private double deleteConnectionMutationProbability;

        public double DeleteConnectionMutationProbability
        {
            get
            {
                return deleteConnectionMutationProbability;
            }
            set
            {
                deleteConnectionMutationProbability = value;
                CreateRouletteLayouts();
            }
        }

        private void CreateRouletteLayouts()
        {
            RouletteWheelLayout = CreateRouletteWheelLayout();
            RouletteWheelLayoutNonDestructive = CreateRouletteWheelLayoutNonDestructive();
        }

        public DiscreteDistribution RouletteWheelLayout { get; private set; }

        public DiscreteDistribution RouletteWheelLayoutNonDestructive { get; private set; }

        private DiscreteDistribution CreateRouletteWheelLayout()
        {
            var probabilities = GerProbabilities();
            return new DiscreteDistribution(probabilities);
        }

        private DiscreteDistribution CreateRouletteWheelLayoutNonDestructive()
        {
            var probabilities = GerProbabilities();
            return new DiscreteDistribution(probabilities);
        }

        private double[] GerProbabilities()
        {
            var probabilities = new[]
            {
                addNodeMutationProbability,
                addConnectionMutationProbability,
                deleteConnectionMutationProbability
            };
            return probabilities;
        }
    }
}
