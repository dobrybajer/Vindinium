using System;
using vindinium.NEAT.Helpers;
using vindinium.NEAT.Mutation;

namespace vindinium.NEAT
{
    internal class Neat
    {
        private readonly IMutationProvider mutationProvider;
        public int CurrentInnovation { get; set; }

        public Neat(IMutationProvider mutationProvider)
        {
            this.mutationProvider = mutationProvider;
            CurrentInnovation = 0;
        }
    }
}
