using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vindinium.NEAT
{
    public class Innovations
    {
        public int InnovationNumber { get; set; }
        public int  InNode { get; set; }
        public int OutNode { get; set; }

        public Innovations()
        {

        }

        public Innovations(int innovationNumber, int inNode, int outNode)
        {
           InnovationNumber = innovationNumber;
           InNode = inNode;
           OutNode = outNode;

        }
            
    }
}
