using System;
using System.Collections.Generic;
using System.Linq;
using vindinium.NEAT;

namespace vindinium.Algorithm
{
    internal class NeatBot : Bot
    {
        private Genotype TrainedModel { get; set; }

        private Genotype CurrentModel { get; set; }

        private const string TrainedPhaseOneBot1 = "PhaseOneBot1.txt";

        private const string TrainedPhaseOneBot2 = "PhaseOneBot2.txt";

        private const string TrainedPhaseOneBot3 = "PhaseOneBot3.txt";


        public NeatBot(ServerStuff serverStuff) : base(serverStuff, "Neat") { }

        protected override string GetDirection()
        {
            return Compute();
        }

        private string Compute()
        {
            var input = MapBoardToNeuralNetworskInput();

            var maxLayer = CurrentModel.NodeGens.Max(g => g.Level);

            var currentLayer = new List<NodeGenesModel>();

            for (var i = 0; i <= maxLayer; ++i)
            {
                // ReSharper disable once AccessToModifiedClosure
                currentLayer = CurrentModel.NodeGens.Where(g => g.Level == i).OrderBy(g => g.NodeNumber).ToList();
                if (i == 0)
                {
                    for (var j = 0; j < currentLayer.Count(); ++j)
                    {
                        currentLayer[j].FeedForwardValue = input[j];
                    }
                }
                else
                {
                    for (var j = 0; j < currentLayer.Count(); ++j)
                    {
                        var value = (from sourceNode in currentLayer[j].SourceNodes
                            let node = CurrentModel.GetNodeById(sourceNode)
                            let edge = CurrentModel.GetEnabledConnectionByIds(sourceNode, currentLayer[j].NodeNumber)
                            where node != null && edge != null
                            select node.FeedForwardValue*edge.Weight).Sum();

                        // TODO add here activation function value = f(value)
                        currentLayer[j].FeedForwardValue = value;
                    }
                }
            }

            var output = MapNeuralNetowrkOutputToMove(currentLayer);

            return output;
        }

        public List<double> MapBoardToNeuralNetworskInput()
        {
            var heroesGoldMax = ServerStuff.Heroes.Max(h => h.gold);
            var distanceToEnemies = GetDistancesToEnemies();

            var feature1 = (double) ServerStuff.Heroes[1].gold/heroesGoldMax;
            var feature2 = (double) ServerStuff.Heroes.Where(h => h.id != 1).Max(h => h.gold)/heroesGoldMax;
            var feature3 = GetDistanceToClosestTavern()/MaxBoardDistance;
            var feature4 = GetDistanceToClosestMine()/MaxBoardDistance;
            var feature5 = GetDistanceToClosestMine(null, true)/MaxBoardDistance;
            var feature6 = distanceToEnemies.Values.Min()/MaxBoardDistance;
            var enemyWithMaxGold = distanceToEnemies.Keys.Aggregate((i, j) => i.gold > j.gold ? i : j);
            var feature7 = distanceToEnemies[enemyWithMaxGold]/MaxBoardDistance;
            var enemyWithLowestHp = distanceToEnemies.Keys.Aggregate((i, j) => i.life < j.life ? i : j);
            var feature8 = enemyWithLowestHp.life < 40 ? distanceToEnemies[enemyWithLowestHp]/MaxBoardDistance : 1; // 1 represents infinity
            var enemyWithGreatestNumberOfMines = distanceToEnemies.Keys.Aggregate((i, j) => i.mineCount > j.mineCount ? i : j);
            var feature9 = distanceToEnemies[enemyWithGreatestNumberOfMines]/MaxBoardDistance;

            var featureList = new List<double>
            {
                feature1,
                feature2,
                feature3,
                feature4,
                feature5,
                feature6,
                feature7,
                feature8,
                feature9
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

        internal override void Train()
        {
            var population1 = TrainPhaseOne("m1");
            var population2 = TrainPhaseOne("m2");
            var population3 = TrainPhaseOne("m3");
            var population4 = TrainPhaseOne("m4");
            var population5 = TrainPhaseOne("m5");
            var population6 = TrainPhaseOne("m6");

            var bestOfPopulation1 = population1.OrderByDescending(i => i.Value).Take(6).ToList();
            var bestOfPopulation2 = population2.OrderByDescending(i => i.Value).Take(6).ToList();
            var bestOfPopulation3 = population3.OrderByDescending(i => i.Value).Take(6).ToList();
            var bestOfPopulation4 = population4.OrderByDescending(i => i.Value).Take(6).ToList();
            var bestOfPopulation5 = population5.OrderByDescending(i => i.Value).Take(6).ToList();
            var bestOfPopulation6 = population6.OrderByDescending(i => i.Value).Take(6).ToList();

            DataManager.ObjectManager.WriteToJsonFile(TrainedPhaseOneBot1, bestOfPopulation1.First());
            DataManager.ObjectManager.WriteToJsonFile(TrainedPhaseOneBot2, bestOfPopulation2.First());
            DataManager.ObjectManager.WriteToJsonFile(TrainedPhaseOneBot3, bestOfPopulation3.First());

            var startPopulationToPhaseTwo = new List<Genotype>();
            startPopulationToPhaseTwo.AddRange(bestOfPopulation1);
            startPopulationToPhaseTwo.AddRange(bestOfPopulation2);
            startPopulationToPhaseTwo.AddRange(bestOfPopulation3);
            startPopulationToPhaseTwo.AddRange(bestOfPopulation4);
            startPopulationToPhaseTwo.AddRange(bestOfPopulation5);
            startPopulationToPhaseTwo.AddRange(bestOfPopulation6);

            TrainPhaseTwo(startPopulationToPhaseTwo);
        }

        // TODO make this for every map from m1 to m6
        private IEnumerable<Genotype> TrainPhaseOne(string map)
        {
            var population = new List<Genotype>();

            for (var i = 0; i < 50; i++) // TODO check if restart ServerStuff needed
            {
                var genotype = new Genotype(); // TODO add function generating random genotype in default constructor
                CurrentModel = genotype;

                Run();

                genotype.Value = ServerStuff.MyHero.gold;
                population.Add(genotype);
            }

            var halfBestPopulation = population.OrderByDescending(i => i.Value).Take(population.Count/2).ToList();
            var changedHalfBestPopulation = halfBestPopulation; // TODO modify genotypes by crossovering and mutating

            var parentPopulation = new List<Genotype>();
            parentPopulation.AddRange(halfBestPopulation);
            parentPopulation.AddRange(changedHalfBestPopulation);

            for (var j = 0; j < 9; j++)
            {
                population = new List<Genotype>();
                for (var i = 0; i < 50; i++) // TODO check if restart ServerStuff needed
                {
                    var genotype = parentPopulation[i];
                    CurrentModel = genotype;

                    Run();

                    genotype.Value = ServerStuff.MyHero.gold;
                    population.Add(genotype);
                }

                halfBestPopulation = population.OrderByDescending(i => i.Value).Take(population.Count / 2).ToList();
                changedHalfBestPopulation = halfBestPopulation; // TODO modify genotypes by crossovering and mutating

                parentPopulation = new List<Genotype>();
                parentPopulation.AddRange(halfBestPopulation);
                parentPopulation.AddRange(changedHalfBestPopulation);
            }

            return parentPopulation;
        }

        // TODO finish this method
        private void TrainPhaseTwo(List<Genotype> startPopulation)
        {
            TrainedModel = new Genotype();
        }
    }
}
