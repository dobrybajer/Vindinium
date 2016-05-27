using System;
using System.Collections.Generic;
using System.Linq;
using vindinium.NEAT.Extensions;

namespace vindinium.NEAT.Crossover
{
    public enum CrossoverMasterType
    {
        GenotypeOne,
        GenotypeTwo
    }
    public class CrossoverProvider : ICrossoverProvider
    {
        private CorrelationResults _correlationResults;
        private readonly ICorrelationProvider _correlationProvider;
        private Genotype _offspringGenome;
        private HashSet<int> _addedNodes;
        private CrossoverMasterType _crossoverMasterType;
        private Genotype _crossoverMaster;

        public CrossoverProvider(ICorrelationProvider correlationProvider)
        {
            _correlationProvider = correlationProvider;
        }

        public Genotype CrossoverGenotype(Genotype genotype1, Genotype genotype2)
        {
            _correlationResults = _correlationProvider.CorrelateConnections(genotype1.GenomeConnection, genotype2.GenomeConnection);
            _crossoverMaster = ChooseCrossoverMaster(genotype1, genotype2);

            _offspringGenome = new Genotype
            {
                GenomeConnection = new List<ConnectionGenesModel>(),
                NodeGens = GetNodesForOffspring(genotype1, genotype2),
                Value = _crossoverMaster.Value
            };

            _offspringGenome.GenomeConnection.AddRange(GetConnectionsForMatch());
            RebuildNodesInfo();
            _offspringGenome.GenomeConnection.AddRange(GetConnectionsForDisjointOrExcess(CorrelationItemType.Disjoint));
            _offspringGenome.GenomeConnection.AddRange(GetConnectionsForDisjointOrExcess(CorrelationItemType.Excess));
            UpdateNodesInfo();
            return _offspringGenome;
        }

        private Genotype ChooseCrossoverMaster(Genotype genotype1, Genotype genotype2)
        {
            var random = new Random();
            if (genotype1.Value > genotype2.Value) _crossoverMasterType = CrossoverMasterType.GenotypeOne;
            else if (genotype1.Value < genotype2.Value) _crossoverMasterType = CrossoverMasterType.GenotypeTwo;
            else _crossoverMasterType = (random.Next(0, 2) == 0) ? CrossoverMasterType.GenotypeOne : CrossoverMasterType.GenotypeTwo;
            return _crossoverMasterType == CrossoverMasterType.GenotypeOne ? genotype1 : genotype2;
        }

        private IEnumerable<ConnectionGenesModel> GetConnectionsForMatch()
        {
            var connectionWithGivenType = new List<ConnectionGenesModel>();
            foreach (var correlationItem in _correlationResults.CorrelationItems)
            {
                if (correlationItem.CorrelationItemType == CorrelationItemType.Match)
                {
                    var connection = _crossoverMasterType == CrossoverMasterType.GenotypeOne
                        ? correlationItem.ConnectionGene1
                        : correlationItem.ConnectionGene2;
                    connectionWithGivenType.Add(connection);
                }
            }
            return connectionWithGivenType;
        }

        private IEnumerable<ConnectionGenesModel> GetConnectionsForDisjointOrExcess(CorrelationItemType correlationItemType)
        {
            if (correlationItemType == CorrelationItemType.Match)
                throw new ArgumentException("Match type is not allowed here");
            var connectionWithGivenType = new List<ConnectionGenesModel>();
            foreach (var correlationItem in _correlationResults.CorrelationItems)
            {
                if (correlationItem.CorrelationItemType == correlationItemType)
                {
                    var connection = correlationItem.ConnectionGene1 ?? correlationItem.ConnectionGene2;
                    if (!_offspringGenome.NodeGens.IsConnectionCyclic(connection.InNode, connection.OutNode))
                        connectionWithGivenType.Add(connection);
                }
            }
            return connectionWithGivenType;
        }

        private List<NodeGenesModel> GetNodesForOffspring(Genotype genotype1, Genotype genotype2)
        {
            var nodesForCorrelationType = new List<NodeGenesModel>();
            _addedNodes = new HashSet<int>();
            foreach (var correlationItem in _correlationResults.CorrelationItems)
            {
                switch (correlationItem.CorrelationItemType)
                {
                    case CorrelationItemType.Match:
                        nodesForCorrelationType.AddRange(SelectNodesFromConnection(_crossoverMaster, correlationItem.ConnectionGene1));
                        break;
                    case CorrelationItemType.Disjoint:
                    case CorrelationItemType.Excess:
                        nodesForCorrelationType.AddRange(correlationItem.ConnectionGene1 != null
                            ? SelectNodesFromConnection(genotype1, correlationItem.ConnectionGene1)
                            : SelectNodesFromConnection(genotype2, correlationItem.ConnectionGene2));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return nodesForCorrelationType.OrderBy(n => n.NodeNumber).ToList();
        }

        private List<NodeGenesModel> SelectNodesFromConnection(Genotype genotype, ConnectionGenesModel connectionGenesModel)
        {
            var nodeGenesModelsToAdd = new List<NodeGenesModel>();

            if (!_addedNodes.Contains(connectionGenesModel.InNode))
            {
                nodeGenesModelsToAdd.Add(genotype.NodeGens.Find(n => n.NodeNumber == connectionGenesModel.InNode));
                _addedNodes.Add(connectionGenesModel.InNode);
            }
            if (!_addedNodes.Contains(connectionGenesModel.OutNode))
            {
                nodeGenesModelsToAdd.Add(genotype.NodeGens.Find(n => n.NodeNumber == connectionGenesModel.OutNode));
                _addedNodes.Add(connectionGenesModel.OutNode);
            }
            return nodeGenesModelsToAdd;
        }

        private void RebuildNodesInfo()
        {
            ClearSourceandTargetNodes();
            UpdateNodesInfo();
        }

        private void UpdateNodesInfo()
        {
            foreach (var connection in _offspringGenome.GenomeConnection)
            {
                if (connection.Status == ConnectionStatus.Disabled) continue;
                var sourceNode = _offspringGenome.NodeGens.First(node => node.NodeNumber == connection.InNode);
                var targetNode = _offspringGenome.NodeGens.First(node => node.NodeNumber == connection.OutNode);
                sourceNode.TargetNodes.Add(connection.OutNode);
                targetNode.SourceNodes.Add(connection.InNode);
            }
        }

        private void ClearSourceandTargetNodes()
        {
            foreach (var node in _offspringGenome.NodeGens)
            {
                if (node.SourceNodes != null && node.SourceNodes.Count != 0) node.SourceNodes.Clear();
                if (node.TargetNodes != null && node.TargetNodes.Count != 0) node.TargetNodes.Clear();
            }
        }
    }
}
