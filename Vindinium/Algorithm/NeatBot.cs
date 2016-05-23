using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using vindinium.NEAT;
using vindinium.NEAT.Crossover;
using vindinium.NEAT.Mutation;
using vindinium.Singletons;

namespace vindinium.Algorithm
{
    public class NeatBot : Bot
    {
        #region Private Fields

        private Genotype CurrentModel { get; set; }

        private readonly InitialGenomeBuilder _initialGenomeBuilder;

        private readonly NeatGeneticAlgorithm _neatGeneticAlgorithm = new NeatGeneticAlgorithm(new CrossoverProvider(new CorrelationProvider()), new MutationProvider());

        #endregion

        #region Constructor

        public NeatBot() : base("Neat") { _initialGenomeBuilder = new InitialGenomeBuilder(); }

        #endregion

        #region Main functions

        public void Play()
        {
            ServerStuff = new ServerStuff(Parameters.ServerSecretKey, false, 0, Parameters.ServerUrl, "");
            CurrentModel = ObjectManager.ReadFromJsonFile<Genotype>(Parameters.TrainedModel);
            Run();
        }

        protected override string GetDirection()
        {
            return Compute();
            //return Direction.GetRandomDirection();
        }

        #endregion

        #region Neural Network computation

        private static double ActivationFunction(double input)
        {
            switch (Parameters.ActivationFunction)
            {
                case Algorithm.ActivationFunction.Linear: // result from 0 to 1 (increasing value)
                    return input;
                case Algorithm.ActivationFunction.HiperbolicTangens: // result from 0 to 0.99 (increasing value)
                    return Math.Tanh(input) * 1.3;
                case Algorithm.ActivationFunction.Unipolar: // result 0 or 1 (increasing value)
                    return input < 0.5 ? 0 : 1;
                case Algorithm.ActivationFunction.Sigmoid: // result from 0.02 to 0.98 (increasing value)
                    return 1 / (1 + Math.Pow(Math.E, -8 * (input - 0.5)));
                case Algorithm.ActivationFunction.ArcTangens: // result from 0 to 0.98 (increasing value)
                    return Math.Atan(input) * 1.25;
                case Algorithm.ActivationFunction.Gaussian: // result from 0.05 to 1 (decreasing value)
                    return Math.Pow(Math.E, -3 * Math.Pow(input, 2));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string Compute()
        {
            var input = MapBoardToNeuralNetworskInput();

            var nextLevelNodes = new List<Tuple<int, int, double>>();
            var inputNodes = CurrentModel.NodeGens.Where(n => n.Type == NodeType.Input).ToList();

            foreach (var sn in inputNodes)
            {
                sn.FeedForwardValue = input[sn.NodeNumber];
                nextLevelNodes.AddRange(sn.TargetNodes.Select(en => new Tuple<int, int, double>(sn.NodeNumber, en, sn.FeedForwardValue)));
            }

            while (nextLevelNodes.Any())
            {
                var currentLevelNodes = new List<Tuple<int, int, double>>(nextLevelNodes);
                nextLevelNodes.Clear();

                foreach (var en in currentLevelNodes)
                {
                    var node = CurrentModel.GetNodeById(en.Item2);
                    var edge = CurrentModel.GetEnabledConnectionByIds(en.Item1, en.Item2);
                    if (node == null || edge == null) throw new ArgumentOutOfRangeException();

                    node.FeedForwardCount++;
                    node.FeedForwardValue += edge.Weight * en.Item3;
                }

                foreach (var sn in currentLevelNodes.Select(n => n.Item2).Distinct())
                {
                    var node = CurrentModel.GetNodeById(sn);
                    nextLevelNodes.AddRange(from nn in node.TargetNodes where node.FeedForwardCount == node.SourceNodes.Count select new Tuple<int, int, double>(sn, nn, ActivationFunction(node.FeedForwardValue)));
                }
            }

            var outputNodes = CurrentModel.NodeGens.Where(n => n.Type == NodeType.Output).ToList();
            var output = MapNeuralNetowrkOutputToMove(outputNodes);

            return output;
        }

        public List<double> MapBoardToNeuralNetworskInput()
        {
            var heroesGoldMax = ServerStuff.Heroes.Max(h => h.gold);
            var distanceToEnemies = GetDistancesToEnemies();

            var feature1 = (double)ServerStuff.Heroes[1].gold / heroesGoldMax;
            var feature2 = (double)ServerStuff.Heroes.Where(h => h.id != 1).Max(h => h.gold) / heroesGoldMax;
            var feature3 = GetDistanceToClosestTavern() / MaxBoardDistance;
            var feature4 = GetDistanceToClosestMine() / MaxBoardDistance;
            var feature5 = GetDistanceToClosestMine(null, true) / MaxBoardDistance;
            var feature6 = distanceToEnemies.Values.Min() / MaxBoardDistance;
            var enemyWithMaxGold = distanceToEnemies.Keys.Aggregate((i, j) => i.gold > j.gold ? i : j);
            var feature7 = distanceToEnemies[enemyWithMaxGold] / MaxBoardDistance;
            var enemyWithLowestHp = distanceToEnemies.Keys.Aggregate((i, j) => i.life < j.life ? i : j);
            var feature8 = enemyWithLowestHp.life < 40 ? distanceToEnemies[enemyWithLowestHp] / MaxBoardDistance : 1; // 1 represents infinity
            var enemyWithGreatestNumberOfMines = distanceToEnemies.Keys.Aggregate((i, j) => i.mineCount > j.mineCount ? i : j);
            var feature9 = distanceToEnemies[enemyWithGreatestNumberOfMines] / MaxBoardDistance;

            var featureList = new List<double>
            {
                feature1, feature2, feature3, feature4, feature5, feature6, feature7, feature8, feature9
            };

            return featureList;
        }

        public string MapNeuralNetowrkOutputToMove(List<NodeGenesModel> outputLayer)
        {
            var maxNeuron = outputLayer.Aggregate((i, j) => i.FeedForwardValue > j.FeedForwardValue ? i : j);
            var index = maxNeuron.NodeNumber;
            var minIndex = outputLayer.Min(o => o.NodeNumber);

            if (index == minIndex + 0) return GetDirectionGeneric(GetDistanceToClosestMine, false);
            if (index == minIndex + 1) return GetDirectionGeneric(GetDistanceToClosestMine, true);
            if (index == minIndex + 2) return GetDirectionGeneric(GetDistanceToClosestTavern, true);
            if (index == minIndex + 3) return GetDirectionGeneric(GetDistanceToClosestEnemy, (int?)1);
            if (index == minIndex + 4) return GetDirectionGeneric(GetDistanceToClosestEnemy, (int?)2);
            if (index == minIndex + 5) return GetDirectionGeneric(GetDistanceToClosestEnemy, (int?)3);

            throw new ArgumentOutOfRangeException();
        }

        #endregion

        #region Training

        public void Train(bool withPhaseTwo = false)
        {
            var population1 = TrainPhaseOne("m1");
            var population2 = TrainPhaseOne("m2");
            var population3 = TrainPhaseOne("m3");
            var population4 = TrainPhaseOne("m4");
            var population5 = TrainPhaseOne("m5");
            var population6 = TrainPhaseOne("m6");

            var bestOfPopulation1 = population1.OrderByDescending(i => i.Value).Take(Parameters.BestGenotypesOfPhaseOneCount).ToList();
            var bestOfPopulation2 = population2.OrderByDescending(i => i.Value).Take(Parameters.BestGenotypesOfPhaseOneCount).ToList();
            var bestOfPopulation3 = population3.OrderByDescending(i => i.Value).Take(Parameters.BestGenotypesOfPhaseOneCount).ToList();
            var bestOfPopulation4 = population4.OrderByDescending(i => i.Value).Take(Parameters.BestGenotypesOfPhaseOneCount).ToList();
            var bestOfPopulation5 = population5.OrderByDescending(i => i.Value).Take(Parameters.BestGenotypesOfPhaseOneCount).ToList();
            var bestOfPopulation6 = population6.OrderByDescending(i => i.Value).Take(Parameters.BestGenotypesOfPhaseOneCount).ToList();

            ObjectManager.WriteToJsonFile(Parameters.TrainedPhaseOneMap1, bestOfPopulation1.First());
            ObjectManager.WriteToJsonFile(Parameters.TrainedPhaseOneMap2, bestOfPopulation2.First());
            ObjectManager.WriteToJsonFile(Parameters.TrainedPhaseOneMap3, bestOfPopulation3.First());
            ObjectManager.WriteToJsonFile(Parameters.TrainedPhaseOneMap4, bestOfPopulation4.First());
            ObjectManager.WriteToJsonFile(Parameters.TrainedPhaseOneMap5, bestOfPopulation5.First());
            ObjectManager.WriteToJsonFile(Parameters.TrainedPhaseOneMap6, bestOfPopulation6.First());

            if (!withPhaseTwo) return;

            var startPopulationToPhaseTwo = new List<Genotype>();
            startPopulationToPhaseTwo.AddRange(bestOfPopulation1);
            startPopulationToPhaseTwo.AddRange(bestOfPopulation2);
            startPopulationToPhaseTwo.AddRange(bestOfPopulation3);
            startPopulationToPhaseTwo.AddRange(bestOfPopulation4);
            startPopulationToPhaseTwo.AddRange(bestOfPopulation5);
            startPopulationToPhaseTwo.AddRange(bestOfPopulation6);

            TrainPhaseTwo(startPopulationToPhaseTwo);
        }

        private IEnumerable<Genotype> TrainPhaseOne(string map)
        {
            ServerStuff = new ServerStuff(Parameters.ServerSecretKey, true, Parameters.ServerNumberOfTurns, Parameters.ServerUrl, map);

            var parentPopulation = new List<Genotype>();

            Console.Out.WriteLine($"-------------------------------------PHASE ONE STARTED (map {map})-------------------------------------");

            for (var j = 0; j < Parameters.GenerationsPhaseOneCount; j++)
            {
                var population = new List<Genotype>();

                Console.Out.WriteLine("-------------------------------------");
                Console.Out.WriteLine($"Generation nr: {j}");

                var watch = new Stopwatch();
                watch.Start();

                for (var i = 0; i < Parameters.PopulationCount; i++)
                {
                    var genotype = i == 0 ? _initialGenomeBuilder.CreateInitialGenotype(Parameters.InputLayerNeuronsCount, Parameters.OutputLayerNeuronsCount) : parentPopulation[i];

                    CurrentModel = genotype;

                    Console.Out.WriteLine($"Genotype nr: {i}");

                    Run(true);

                    Console.Out.WriteLine($"Score (gold): {ServerStuff.MyHero.gold}");

                    genotype.Value = ServerStuff.MyHero.gold;
                    population.Add(genotype);
                }

                watch.Stop();

                Console.Out.WriteLine($"FINISHED Generation nr: {j}. Time elapsed: {watch.ElapsedMilliseconds} ms");

                var partBestPopulation1 = population.OrderByDescending(i => i.Value).Take((int)(Parameters.PopulationCount * Parameters.BestOfPopulationPercentage)).ToList();
                var partBestPopulation2 = population.OrderByDescending(i => i.Value).Take((int)(Parameters.PopulationCount * Parameters.BestOfPopulationPercentage)).ToList();
                //var partWorsePopulation = population.OrderBy(i => i.Value).Take((int)(Parameters.PopulationCount * Parameters.BestOfPopulationPercentage)).ToList();

                var changedPartBestPopulation1 = _neatGeneticAlgorithm.CreateNewPopulationWithCrossover(partBestPopulation1);
                var changedPartBestPopulation2 = _neatGeneticAlgorithm.CreateNewPopulationWithCrossover(partBestPopulation2);
                //var changedPartWorsePopulation = _neatGeneticAlgorithm.CreateNewPopulationWithMutation(partWorsePopulation);

                parentPopulation = new List<Genotype>();
                parentPopulation.AddRange(changedPartBestPopulation1);
                parentPopulation.AddRange(changedPartBestPopulation2);
            }

            Console.Out.WriteLine($"-------------------------------------PHASE ONE ENDED (map {map})-------------------------------------");

            return parentPopulation;
        }

        // TODO finish this method
        private void TrainPhaseTwo(List<Genotype> startPopulation)
        {
            // create appropriate serverstuff
            var parentPopulation = startPopulation;
            ObjectManager.WriteToJsonFile(Parameters.TrainedModel, parentPopulation.First());
        }

        #endregion

        #region Test methods

        // To test please change Compute() function to not use map board method
        public void TestGraphCompute()
        {
            var genotype = new Genotype
            {
                NodeGens = new List<NodeGenesModel>(),
                GenomeConnection = new List<ConnectionGenesModel>()
            };

            // ---- INPUT ----

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 0,
                SourceNodes = new HashSet<int>(),
                TargetNodes = new HashSet<int> { 4, 5 },
                Type = NodeType.Input
            });

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 1,
                SourceNodes = new HashSet<int>(),
                TargetNodes = new HashSet<int> { 4, 6 },
                Type = NodeType.Input
            });

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 2,
                SourceNodes = new HashSet<int>(),
                TargetNodes = new HashSet<int> { 5, 6 },
                Type = NodeType.Input
            });

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 3,
                SourceNodes = new HashSet<int>(),
                TargetNodes = new HashSet<int> { 6 },
                Type = NodeType.Input
            });

            // ---- HIDDEN ----

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 4,
                SourceNodes = new HashSet<int> { 0, 1 },
                TargetNodes = new HashSet<int> { 7 },
                Type = NodeType.Hidden
            });

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 5,
                SourceNodes = new HashSet<int> { 0, 2 },
                TargetNodes = new HashSet<int> { 7 },
                Type = NodeType.Hidden
            });

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 6,
                SourceNodes = new HashSet<int> { 1, 2, 3 },
                TargetNodes = new HashSet<int> { 8, 10 }, // 10 is 2 "layers" after
                Type = NodeType.Hidden
            });

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 7,
                SourceNodes = new HashSet<int> { 4, 5 },
                TargetNodes = new HashSet<int> { 9 },
                Type = NodeType.Hidden
            });

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 8,
                SourceNodes = new HashSet<int> { 6 },
                TargetNodes = new HashSet<int> { 9, 10 },
                Type = NodeType.Hidden
            });

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 9,
                SourceNodes = new HashSet<int> { 7, 8 },
                TargetNodes = new HashSet<int> { 11, 12 },
                Type = NodeType.Hidden
            });

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 10,
                SourceNodes = new HashSet<int> { 6, 8 },
                TargetNodes = new HashSet<int> { 11, 12 },
                Type = NodeType.Hidden
            });

            // ---- OUTPUT ----

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 11,
                SourceNodes = new HashSet<int> { 9, 10 },
                TargetNodes = new HashSet<int>(),
                Type = NodeType.Output
            });

            genotype.NodeGens.Add(new NodeGenesModel
            {
                NodeNumber = 12,
                SourceNodes = new HashSet<int> { 9, 10 },
                TargetNodes = new HashSet<int>(),
                Type = NodeType.Output
            });

            // ---- EDGES ----

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 0,
                OutNode = 4,
                Status = ConnectionStatus.Enabled,
                Weight = 0.1
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 0,
                OutNode = 5,
                Status = ConnectionStatus.Enabled,
                Weight = 0.3
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 1,
                OutNode = 4,
                Status = ConnectionStatus.Enabled,
                Weight = 0.2
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 1,
                OutNode = 6,
                Status = ConnectionStatus.Enabled,
                Weight = 0.4
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 2,
                OutNode = 5,
                Status = ConnectionStatus.Enabled,
                Weight = 0.5
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 2,
                OutNode = 6,
                Status = ConnectionStatus.Enabled,
                Weight = 0.3
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 3,
                OutNode = 6,
                Status = ConnectionStatus.Enabled,
                Weight = 0.4
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 4,
                OutNode = 7,
                Status = ConnectionStatus.Enabled,
                Weight = 0.35
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 5,
                OutNode = 7,
                Status = ConnectionStatus.Enabled,
                Weight = 0.42
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 6,
                OutNode = 8,
                Status = ConnectionStatus.Enabled,
                Weight = 0.18
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 6,
                OutNode = 10,
                Status = ConnectionStatus.Enabled,
                Weight = 0.03
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 7,
                OutNode = 9,
                Status = ConnectionStatus.Enabled,
                Weight = 0.2
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 8,
                OutNode = 9,
                Status = ConnectionStatus.Enabled,
                Weight = 0.7
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 8,
                OutNode = 10,
                Status = ConnectionStatus.Enabled,
                Weight = 0.28
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 9,
                OutNode = 11,
                Status = ConnectionStatus.Enabled,
                Weight = 0.82
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 9,
                OutNode = 12,
                Status = ConnectionStatus.Enabled,
                Weight = 0.3
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 10,
                OutNode = 11,
                Status = ConnectionStatus.Enabled,
                Weight = 0.7
            });

            genotype.GenomeConnection.Add(new ConnectionGenesModel
            {
                InNode = 10,
                OutNode = 12,
                Status = ConnectionStatus.Enabled,
                Weight = 0.5
            });

            CurrentModel = genotype;

            var watch = new Stopwatch();
            watch.Start();

            Compute();
            watch.Stop();
            Console.WriteLine($"elapsed: {watch.ElapsedMilliseconds}ms");
        }

        #endregion
    }
}
