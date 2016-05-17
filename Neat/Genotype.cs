using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vindinium.Neat
{
    class Genotype
    {
        public List<ConnectionGenesModel> Genome;

        public Genotype(List<ConnectionGenesModel> genome)
        {
            Genome = genome;
        }

    }
}
