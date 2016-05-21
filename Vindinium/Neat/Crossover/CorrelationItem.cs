namespace vindinium.NEAT.Crossover
{
    public class CorrelationItem
    {
        public CorrelationItem(CorrelationItemType correlationItemType, ConnectionGenesModel connectionGene1, ConnectionGenesModel connectionGene2)
        {
            CorrelationItemType = correlationItemType;
            ConnectionGene1 = connectionGene1;
            ConnectionGene2 = connectionGene2;
        }

        public CorrelationItemType CorrelationItemType { get; }

        public ConnectionGenesModel ConnectionGene1 { get; }

        public ConnectionGenesModel ConnectionGene2 { get; }
    }
}