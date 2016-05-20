namespace vindinium.NEAT.Crossover
{
    internal class CorrelationStats
    {
        public int MatchingGeneCount { get; set; }
        public int DisjointConnectionGeneCount { get; set; }
        public int ExcessConnectionGeneCount { get; set; }
        public double ConnectionWeightDelta { get; set; }
    }
}