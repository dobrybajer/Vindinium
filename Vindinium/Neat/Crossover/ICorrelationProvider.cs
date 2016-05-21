using System.Collections.Generic;

namespace vindinium.NEAT.Crossover
{
    public interface ICorrelationProvider
    {
        CorrelationResults CorrelateConnections(List<ConnectionGenesModel> list1, List<ConnectionGenesModel> list2);
    }
}