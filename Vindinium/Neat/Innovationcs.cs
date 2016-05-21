using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vindinium.NEAT
{
    class Innovationcs
    {
        public int InnovationNumber { get; set; }
        public InnovationcsType Type { get; set; }
        public int  InNode { get; set; }
        public int OutNode { get; set; }

        public Innovationcs()
        {

        }

        public Innovationcs(int innovationNumber, InnovationcsType type, int inNode, int outNode)
        {
           InnovationNumber = innovationNumber;
           Type = type;
           InNode = inNode;
           OutNode = outNode;

        }
            
    }
}
