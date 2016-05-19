using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vindinium.Neat
{
    class ConnectionGenesModel
    {
        public int InNode { get; set; }
        public int OutNode { get; set; }
        public double Weight { get; set; }
        public ConnectionStatus Status { get; set; }
        public int Innovation { get; set; }

        public ConnectionGenesModel()
        {

        }

        public ConnectionGenesModel(int inNode, int outNode, double weight, ConnectionStatus status, int innovation)
        {
            InNode = inNode;
            OutNode = outNode;
            Weight = weight;
            Status = status;
            Innovation = innovation;
        }

        
    }

}
