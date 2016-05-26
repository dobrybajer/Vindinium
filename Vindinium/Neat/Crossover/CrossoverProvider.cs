﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private CorrelationResults correlationResults;

        private readonly ICorrelationProvider correlationProvider;
        private Genotype offspringGenome;
        private HashSet<int> addedNodes;
        private CrossoverMasterType crossoverMasterType;
        private Genotype crossoverMaster;
        private Genotype crossoverSecond;

        public CrossoverProvider(ICorrelationProvider correlationProvider)
        {
            this.correlationProvider = correlationProvider;
        }

        public Genotype CrossoverGenotype(Genotype genotype1, Genotype genotype2)
        {
            correlationResults = correlationProvider.CorrelateConnections(genotype1.GenomeConnection, genotype2.GenomeConnection);
            crossoverMaster = ChooseCrossoverMaster(genotype1, genotype2);
            crossoverSecond = ChooseCrossoverSecond(genotype1, genotype2);

            offspringGenome = new Genotype
            {
                GenomeConnection = new List<ConnectionGenesModel>(),
                NodeGens = GetNodesForOffspring(genotype1, genotype2),
                Value = crossoverMaster.Value
            };
            offspringGenome.GenomeConnection.AddRange(GetConnectionsForMatch());
            RebuildNodesInfo();
            offspringGenome.GenomeConnection.AddRange(GetConnectionsForDisjoint());
            offspringGenome.GenomeConnection.AddRange(GetConnectionsForExcess());
            UpdateNodesInfo();
            return offspringGenome;
        }

        private Genotype ChooseCrossoverMaster(Genotype genotype1, Genotype genotype2)
        {
            var random = new Random();
            if (genotype1.Value > genotype2.Value)
                crossoverMasterType = CrossoverMasterType.GenotypeOne;
            else if (genotype1.Value < genotype2.Value)
                crossoverMasterType = CrossoverMasterType.GenotypeTwo;
            else
                crossoverMasterType = (random.Next(0, 2) == 0) ? CrossoverMasterType.GenotypeOne : CrossoverMasterType.GenotypeTwo;
            return crossoverMasterType == CrossoverMasterType.GenotypeOne ? genotype1 : genotype2;
        }

        private Genotype ChooseCrossoverSecond(Genotype genotype1, Genotype genotype2)
        {
            if (crossoverMasterType == CrossoverMasterType.GenotypeOne)
                return genotype2;
            return genotype1;
        }

        private List<ConnectionGenesModel> GetConnectionsForMatch()
        {
            var connectionWithGivenType = new List<ConnectionGenesModel>();
            foreach (var correlationItem in correlationResults.CorrelationItems)
            {
                if (correlationItem.CorrelationItemType == CorrelationItemType.Match)
                {
                    var connection = crossoverMasterType == CrossoverMasterType.GenotypeOne
                        ? correlationItem.ConnectionGene1
                        : correlationItem.ConnectionGene2;
                    connectionWithGivenType.Add(connection);
                }
            }
            return connectionWithGivenType;
        }

        private List<ConnectionGenesModel> GetConnectionsForDisjoint()
        {
            var connectionWithGivenType = new List<ConnectionGenesModel>();
            foreach (var correlationItem in correlationResults.CorrelationItems)
            {
                if (correlationItem.CorrelationItemType == CorrelationItemType.Disjoint)
                {
                    var connection = correlationItem.ConnectionGene1 ?? correlationItem.ConnectionGene2;
                    if (!offspringGenome.NodeGens.IsConnectionCyclic(connection.InNode, connection.OutNode))
                        connectionWithGivenType.Add(connection);
                }
            }
            return connectionWithGivenType;
        }

        private List<ConnectionGenesModel> GetConnectionsForExcess()
        {
            var connectionWithGivenType = new List<ConnectionGenesModel>();
            foreach (var correlationItem in correlationResults.CorrelationItems)
            {
                if (correlationItem.CorrelationItemType == CorrelationItemType.Excess)
                {
                    var connection = correlationItem.ConnectionGene1 ?? correlationItem.ConnectionGene2;
                    if (!offspringGenome.NodeGens.IsConnectionCyclic(connection.InNode, connection.OutNode))
                        connectionWithGivenType.Add(connection);
                }
            }
            return connectionWithGivenType;
        }

        private List<NodeGenesModel> GetNodesForOffspring(Genotype genotype1, Genotype genotype2)
        {
            var nodesForCorrelationType = new List<NodeGenesModel>();
            addedNodes = new HashSet<int>();
            foreach (var correlationItem in correlationResults.CorrelationItems)
            {
                switch (correlationItem.CorrelationItemType)
                {
                    case CorrelationItemType.Match:
                        nodesForCorrelationType.AddRange(SelectNodesFromConnection(crossoverMaster, correlationItem.ConnectionGene1));
                        break;
                    case CorrelationItemType.Disjoint:
                    case CorrelationItemType.Excess:
                        if (correlationItem.ConnectionGene1 != null)
                            nodesForCorrelationType.AddRange(SelectNodesFromConnection(genotype1, correlationItem.ConnectionGene1));
                        else
                            nodesForCorrelationType.AddRange(SelectNodesFromConnection(genotype2, correlationItem.ConnectionGene2));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return nodesForCorrelationType;
        }

        private List<NodeGenesModel> SelectNodesFromConnection(Genotype genotype, ConnectionGenesModel connectionGenesModel)
        {
            var nodeGenesModelsToAdd = new List<NodeGenesModel>();

            if (!addedNodes.Contains(connectionGenesModel.InNode))
            {
                nodeGenesModelsToAdd.Add(
                    genotype.NodeGens.Find(n => n.NodeNumber == connectionGenesModel.InNode));
                addedNodes.Add(connectionGenesModel.InNode);
            }
            if (!addedNodes.Contains(connectionGenesModel.OutNode))
            {
                nodeGenesModelsToAdd.Add(
                    genotype.NodeGens.Find(n => n.NodeNumber == connectionGenesModel.OutNode));
                addedNodes.Add(connectionGenesModel.OutNode);
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
            foreach (var connection in offspringGenome.GenomeConnection)
            {
                if (connection.Status == ConnectionStatus.Disabled) continue;
                var sourceNode = offspringGenome.NodeGens.First(node => node.NodeNumber == connection.InNode);
                var targetNode = offspringGenome.NodeGens.First(node => node.NodeNumber == connection.OutNode);
                sourceNode.TargetNodes.Add(connection.OutNode);
                targetNode.SourceNodes.Add(connection.InNode);
            }
        }

        private void ClearSourceandTargetNodes()
        {
            foreach (var node in offspringGenome.NodeGens)
            {
                if (node.SourceNodes != null && node.SourceNodes.Count != 0)
                    node.SourceNodes.Clear();
                if (node.TargetNodes != null && node.TargetNodes.Count != 0)
                    node.TargetNodes.Clear();
            }
        }
    }
}
