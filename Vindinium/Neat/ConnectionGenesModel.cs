namespace vindinium.NEAT
{
    public class ConnectionGenesModel
    {
        public int InNode { get; set; }

        public int OutNode { get; set; }

        public double Weight { get; set; }

        public ConnectionStatus Status { get; set; }

        public int Innovation { get; set; }

        public bool IsMutated { get; set; }

        public ConnectionGenesModel()
        {

        }

        public ConnectionGenesModel DeepCopy()
        {
            return new ConnectionGenesModel
            {
                InNode = this.InNode,
                OutNode = this.OutNode,
                Weight = this.Weight,
                Status = this.Status,
                Innovation = this.Innovation,
                IsMutated = this.IsMutated
            };
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
