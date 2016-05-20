using vindinium.NEAT.Mutation;

namespace vindinium.NEAT
{
    internal class Neat
    {
        public IMutationProvider Provider { get; }

        public int CurrentInnovation { get; set; }

        public Neat(IMutationProvider mutationProvider)
        {
            Provider = mutationProvider;
            CurrentInnovation = 0;
        }
    }
}
