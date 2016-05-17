using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vindinium.Neat
{
    class Neat
    {
        public int CurrentInnovation { get; set; }

        public Neat()
        {
            CurrentInnovation = 0;
        }

        public void MutateAddConnection(Genotype genotype)
        {
            throw new Exception();
        }

        public void MutateAddNode(Genotype genotype)
        {
            throw new Exception();
        }

        public void MatchingGenomes(Genotype genotype1, Genotype genotype2)
        {
            throw new Exception();
        }
    }
}
