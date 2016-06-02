using System;
using System.Collections.Generic;
using System.Linq;
using vindinium.NEAT;
using vindinium.Singletons;

namespace vindinium.Algorithm
{
    public class NeatBot : Bot
    {
        #region Private Fields

        private Genotype CurrentModel { get; set; }

        #endregion

        #region Constructor

        public NeatBot(string map = null) : base(0, "Neat")
        {
            ServerStuff = new ServerStuff(Parameters.ServerSecretKey, true, Parameters.ServerNumberOfTurns, Parameters.ServerUrl, map);
        }

        public NeatBot() : base(0, "Neat")
        {
        }

        public NeatBot(Genotype genotype) : base(0, "Neat")
        {
            CurrentModel = genotype;
        }

        public NeatBot(int genotypeNumber, string map = null) : base(genotypeNumber, "Neat")
        {
            ServerStuff = new ServerStuff(Parameters.ServerSecretKey, true, Parameters.ServerNumberOfTurns, Parameters.ServerUrl, map);
        }

        #endregion

        #region Main functions

        public string GetInfoAboutGame()
        {
            var info = "";

            for(var i = 0; i < ServerStuff.Heroes.Count; ++i)
            {
                info += $"Bot: {i} / ID: {ServerStuff.Heroes[i].id} / Name: {ServerStuff.Heroes[i].name} / Score: {ServerStuff.Heroes[i].gold} / Elo: {ServerStuff.Heroes[i].elo} / Crashed: {ServerStuff.Heroes[i].crashed}";
                info += Environment.NewLine;
            }

 
            return info;
        }

        public string GetBoardSize()
        {
            return ServerStuff.Board.Length.ToString();
        }

        public void Play(bool onlyComputation = false)
        {
            ServerStuff = new ServerStuff(Parameters.ServerSecretKey, false, 0, Parameters.ServerUrl, "");

            if (CurrentModel == null)
                CurrentModel = ObjectManager.ReadFromJsonFile<Genotype>(Parameters.TrainedModel);
            Run(onlyComputation);
        }

        public Genotype TrainOneGame(Genotype parentGenotype)
        {
            CurrentModel = parentGenotype.DeepCopy();

            //Console.Out.WriteLine($"Genotype: {CurrentGenotype}. START");
            Run(true);
            Console.Out.WriteLine($"Genotype: {CurrentGenotype}. END - Score (gold): {ServerStuff.MyHero.gold}");

            CurrentModel.Value = ServerStuff.MyHero.gold; // TODO dodac uwzglednianie DeathCount jako kary za bezsensowne giniecie

            return CurrentModel;
        }

        public Genotype TrainOneGameInArena(Genotype parentGenotype)
        {
            ServerStuff = new ServerStuff(Parameters.ServerSecretKey, false, 0, Parameters.ServerUrl, "");
            CurrentModel = parentGenotype.DeepCopy();

            Run(true);
            Console.Out.WriteLine($"Genotype: {CurrentGenotype}. END - Score (gold): {ServerStuff.MyHero.gold}");

            var isBotWinner = ServerStuff.Heroes.Max(h => h.gold) == ServerStuff.MyHero.gold ? 1 : 0;
            var bestEnemyGold = ServerStuff.Heroes.Where(h => h.name != "dobrybajer").Max(h => h.gold);

            CurrentModel.ValuePhaseTwo = new List<int> {isBotWinner, ServerStuff.MyHero.gold, bestEnemyGold};
            CurrentModel.DeathCount = DeathCount;
            CurrentModel.Value = ServerStuff.MyHero.gold;
            CurrentModel.MapSize = ServerStuff.Board.Length;

            return CurrentModel;
        }

        protected override string GetDirection()
        {
            return Compute();
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

        private void ClearGenome()
        {
            foreach (var g in CurrentModel.NodeGens)
            {
                g.FeedForwardCount = 0;
                g.FeedForwardValue = 0;
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

            //Console.Out.WriteLine($"Genotype: {CurrentGenotype}. Direction: {output}");
            ClearGenome();

            return output;
        }

        private List<double> MapBoardToNeuralNetworskInput()
        {
            var heroesGoldMax = ServerStuff.Heroes.Max(h => h.gold);
            var distanceToEnemies = GetDistancesToEnemies();

            var feature1 = heroesGoldMax != 0 ? (ServerStuff.Heroes[1].gold == heroesGoldMax ? 0.2 : 1 - (double)ServerStuff.Heroes[1].gold / heroesGoldMax) : 1;
            var feature2 = heroesGoldMax != 0 ? (double)ServerStuff.Heroes.Where(h => h.id != 1).Max(h => h.gold) / heroesGoldMax : 0;
            var feature3 = 1 - (double)ServerStuff.Heroes[1].life / MaxBotHp;
            var feature4 = 1 - GetDistanceToClosestMine() / MaxBoardDistance * 0.8;
            var feature5 = 1 - GetDistanceToClosestMine(null, true) / MaxBoardDistance;

            var enemyWithMaxGold = distanceToEnemies.Keys.Aggregate((i, j) => i.gold > j.gold ? i : j);
            var feature6 = enemyWithMaxGold.gold != 0 ? 1 - (double)distanceToEnemies[enemyWithMaxGold] / MaxBoardDistance * 0.8 : 0;

            var enemyWithLowestHp = distanceToEnemies.Keys.Aggregate((i, j) => i.life < j.life ? i : j);
            var feature7 = enemyWithLowestHp.life < 40 ? 1 - (double)distanceToEnemies[enemyWithLowestHp] / MaxBoardDistance * 0.6 : 0;

            var enemyWithGreatestNumberOfMines = distanceToEnemies.Keys.Aggregate((i, j) => i.mineCount > j.mineCount ? i : j);
            var feature8 = enemyWithGreatestNumberOfMines.mineCount != 0 ? 1 - (double)distanceToEnemies[enemyWithGreatestNumberOfMines] / MaxBoardDistance : 0;

            var featureList = new List<double>
            {
                feature1, feature2, feature3, feature4, feature5, feature6, feature7, feature8
            };

            return featureList;
        }

        private string MapNeuralNetowrkOutputToMove(List<NodeGenesModel> outputLayer)
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

    }
}
