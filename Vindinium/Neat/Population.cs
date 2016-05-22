using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vindinium.NEAT
{
    class Population
    {
        public List<Genotype> PopulationList { get; set; }
        public List<Innovations> Innovation { get; set; }

        public Population()
        {

        }

        public Population(List<Genotype> populationList, List<Innovations> innovation)
        {
            PopulationList = populationList;
            Innovation = innovation;
        }


    }
}
