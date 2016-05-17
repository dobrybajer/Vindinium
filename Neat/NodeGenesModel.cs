using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vindinium.Neat
{
    class NodeGenesModel
    {
        public int NodeNumber { get; set; }
        public NodeType Type { get; set; }

        public NodeGenesModel(int nodeNumber, NodeType type)
        {
            NodeNumber = nodeNumber;
            Type = type;
        }
    }
}
