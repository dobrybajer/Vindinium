using vindinium.NEAT.Mutation;

namespace vindinium.NEAT
{
    public class Neat
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
