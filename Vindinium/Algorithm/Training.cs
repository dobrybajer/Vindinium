using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vindinium.NEAT;
using vindinium.NEAT.Crossover;
using vindinium.NEAT.Mutation;
using vindinium.Singletons;

namespace vindinium.Algorithm
{
    public class Training
    {
        #region Private Fields

        private readonly InitialGenomeBuilder _initialGenomeBuilder;

        private readonly NeatGeneticAlgorithm _neatGeneticAlgorithm;

        #endregion

        #region Constructor

        public Training()
        {
            _initialGenomeBuilder = new InitialGenomeBuilder(); 
            _neatGeneticAlgorithm = new NeatGeneticAlgorithm(new CrossoverProvider(new CorrelationProvider()), new MutationProvider());
        }

        #endregion

        #region Training

        public void Train(bool withPhaseOne = false, bool withPhaseTwo = false)
        {
            List<Genotype> startPopulationToPhaseTwo = null;
            if (withPhaseOne)
            {
                var population1 = TrainPhaseOne("m1");
                var population2 = TrainPhaseOne("m2");
                var population3 = TrainPhaseOne("m3");
                var population4 = TrainPhaseOne("m4");
                var population5 = TrainPhaseOne("m5");
                var population6 = TrainPhaseOne("m6");

                var bestOfPopulation1 =
                    population1.OrderByDescending(i => i.Value).Take(Parameters.BestGenotypesOfPhaseOneCount).ToList();
                var bestOfPopulation2 =
                    population2.OrderByDescending(i => i.Value).Take(Parameters.BestGenotypesOfPhaseOneCount).ToList();
                var bestOfPopulation3 =
                    population3.OrderByDescending(i => i.Value).Take(Parameters.BestGenotypesOfPhaseOneCount).ToList();
                var bestOfPopulation4 =
                    population4.OrderByDescending(i => i.Value).Take(Parameters.BestGenotypesOfPhaseOneCount).ToList();
                var bestOfPopulation5 =
                    population5.OrderByDescending(i => i.Value).Take(Parameters.BestGenotypesOfPhaseOneCount).ToList();
                var bestOfPopulation6 =
                    population6.OrderByDescending(i => i.Value).Take(Parameters.BestGenotypesOfPhaseOneCount).ToList();

                ObjectManager.WriteToJsonFile(Parameters.TrainedPhaseOneMap1, bestOfPopulation1.First());
                ObjectManager.WriteToJsonFile(Parameters.TrainedPhaseOneMap2, bestOfPopulation2.First());
                ObjectManager.WriteToJsonFile(Parameters.TrainedPhaseOneMap3, bestOfPopulation3.First());
                ObjectManager.WriteToJsonFile(Parameters.TrainedPhaseOneMap4, bestOfPopulation4.First());
                ObjectManager.WriteToJsonFile(Parameters.TrainedPhaseOneMap5, bestOfPopulation5.First());
                ObjectManager.WriteToJsonFile(Parameters.TrainedPhaseOneMap6, bestOfPopulation6.First());

                startPopulationToPhaseTwo = new List<Genotype>();
                startPopulationToPhaseTwo.AddRange(bestOfPopulation1);
                startPopulationToPhaseTwo.AddRange(bestOfPopulation2);
                startPopulationToPhaseTwo.AddRange(bestOfPopulation3);
                startPopulationToPhaseTwo.AddRange(bestOfPopulation4);
                startPopulationToPhaseTwo.AddRange(bestOfPopulation5);
                startPopulationToPhaseTwo.AddRange(bestOfPopulation6);

                ObjectManager.WriteToJsonFile("startPopulationToPhaseTwo.txt", startPopulationToPhaseTwo);
            }

            if (!withPhaseTwo) return;
            TrainPhaseTwo(startPopulationToPhaseTwo);
        }

        // TODO dodac zapisywanie/wczytywanie po restarcie innowacji
        private IEnumerable<Genotype> TrainPhaseOne(string map)
        {
            Console.Out.WriteLine($"-------------------------------------PHASE ONE STARTED (map {map})-------------------------------------");

            var readFromFile = false;
            var startIndex = 0;
            for (var j = 0; j < Parameters.GenerationsPhaseOneCount; j++)
            {
                if (!ObjectManager.FileExist(j, Parameters.PopulationCount, map, Parameters.ActivationFunction.ToString(), Parameters.ServerNumberOfTurns)) continue;
                startIndex = j;
                readFromFile = true;
            }

            var parentPopulation = readFromFile ? ObjectManager.ReadGenerationFromFile<List<Genotype>>(startIndex, Parameters.PopulationCount, map, Parameters.ActivationFunction.ToString(), Parameters.ServerNumberOfTurns) : new List<Genotype>();
            var innovationsList = _initialGenomeBuilder.InitInnovationList(Parameters.InputLayerNeuronsCount, Parameters.OutputLayerNeuronsCount);

            for (var j = startIndex; j < Parameters.GenerationsPhaseOneCount; j++)
            {
                List<Genotype> population;
                
                if (!readFromFile)
                {
                    Console.Out.WriteLine($"Generation nr: {j}");
                    Console.Out.WriteLine("-------------------------------------");

                    var parallelParentPopulation = new List<Genotype>();
                    for (var i = 0; i < Parameters.PopulationCount; ++i)
                    {
                        parallelParentPopulation.Add(j == 0
                            ? _initialGenomeBuilder.CreateInitialGenotype(Parameters.InputLayerNeuronsCount, Parameters.OutputLayerNeuronsCount)
                            : parentPopulation[i]);

                        Thread.Sleep(50);
                    }

                    var syncLock = new object();
                    var parallelPopulation = new List<Genotype>();

                    var watch = new Stopwatch();
                    watch.Start();
                    //for (int i = 0; i < Parameters.PopulationCount; ++i)
                    //{
                    //    var neatBot = new NeatBot(i, map);
                    //    var parentGenotype = parallelParentPopulation[i];
                    //    var genotype = neatBot.TrainOneGame(parentGenotype);

                    //        parallelPopulation.Add(genotype);

                    //}
                    Parallel.For(0, Parameters.PopulationCount /*/ 5*/, i =>
                    {
                        var neatBot = new NeatBot(i, map);
                        var parentGenotype = parallelParentPopulation[i];
                        var genotype = neatBot.TrainOneGame(parentGenotype);
                        lock (syncLock)
                        {
                            parallelPopulation.Add(genotype);
                        }
                    });

                    //Parallel.For(Parameters.PopulationCount / 5, Parameters.PopulationCount / 5 * 2, i =>
                    //{
                    //    var neatBot = new NeatBot(i, map);
                    //    var parentGenotype = parallelParentPopulation[i];
                    //    var genotype = neatBot.TrainOneGame(parentGenotype);
                    //    lock (syncLock)
                    //    {
                    //        parallelPopulation.Add(genotype);
                    //    }
                    //});

                    //Parallel.For(Parameters.PopulationCount / 5 * 2, Parameters.PopulationCount / 5 * 3, i =>
                    //{
                    //    var neatBot = new NeatBot(i, map);
                    //    var parentGenotype = parallelParentPopulation[i];
                    //    var genotype = neatBot.TrainOneGame(parentGenotype);
                    //    lock (syncLock)
                    //    {
                    //        parallelPopulation.Add(genotype);
                    //    }
                    //});

                    //Parallel.For(Parameters.PopulationCount / 5 * 3, Parameters.PopulationCount / 5 * 4, i =>
                    //{
                    //    var neatBot = new NeatBot(i, map);
                    //    var parentGenotype = parallelParentPopulation[i];
                    //    var genotype = neatBot.TrainOneGame(parentGenotype);
                    //    lock (syncLock)
                    //    {
                    //        parallelPopulation.Add(genotype);
                    //    }
                    //});

                    //Parallel.For(Parameters.PopulationCount / 5 * 4, Parameters.PopulationCount, i =>
                    //{
                    //    var neatBot = new NeatBot(i, map);
                    //    var parentGenotype = parallelParentPopulation[i];
                    //    var genotype = neatBot.TrainOneGame(parentGenotype);
                    //    lock (syncLock)
                    //    {
                    //        parallelPopulation.Add(genotype);
                    //    }
                    //});

                    watch.Stop();
                    Console.Out.WriteLine($"FINISHED Generation nr: {j}. Time elapsed: {watch.ElapsedMilliseconds} ms");

                    population = parallelPopulation;
                    ObjectManager.WriteGenerationToFile(population, j, Parameters.PopulationCount, map, Parameters.ActivationFunction.ToString(), Parameters.ServerNumberOfTurns);                
                }
                else
                {
                    readFromFile = false;
                    population = parentPopulation;
                    Console.Out.WriteLine($"Generation {j} read from file.");
                }

                population = population.OrderByDescending(i => i.Value).ToList();
                var numberToTake = (int)(Parameters.PopulationCount * Parameters.BestOfPopulationPercentage);
                var halfBestOfPopulation = population.GetRange(0, numberToTake);

                var partBestPopulation1 = halfBestOfPopulation.Select(x => x.DeepCopy()).ToList();
                var partBestPopulation2 = halfBestOfPopulation.Select(x => x.DeepCopy()).ToList();

                var changedPartBestPopulation1 =  _neatGeneticAlgorithm.CreateNewPopulationWithMutation(partBestPopulation1, ref innovationsList);
                var changedPartBestPopulation2 = _neatGeneticAlgorithm.CreateNewPopulationWithCrossover(partBestPopulation2);

                parentPopulation = new List<Genotype>();
                parentPopulation.AddRange(changedPartBestPopulation1.Select(x => x.DeepCopy()).ToList());
                parentPopulation.AddRange(changedPartBestPopulation2.Select(x => x.DeepCopy()).ToList());
            }

            Console.Out.WriteLine($"-------------------------------------PHASE ONE ENDED (map {map})-------------------------------------");

            return parentPopulation;
        }

        // TODO dodac zapisywanie/wczytywanie po restarcie innowacji
        private void TrainPhaseTwo(List<Genotype> startPopulation = null)
        {
            Console.Out.WriteLine("-------------------------------------PHASE TWO STARTED (map random on arena)-------------------------------------");

            var readFromFile = false;
            var startIndex = 0;
            for (var j = 0; j < Parameters.GenerationsPhaseTwoCount; j++)
            {
                if (!ObjectManager.FileExist(j, Parameters.PopulationCount, "PHASE_TWO", Parameters.ActivationFunction.ToString(), 0)) continue;
                startIndex = j;
                readFromFile = true;
            }

            var parentPopulation = readFromFile ? ObjectManager.ReadGenerationFromFile<List<Genotype>>(startIndex, Parameters.PopulationCount, "PHASE_TWO", Parameters.ActivationFunction.ToString(), 0) : startPopulation ?? new List<Genotype>();
            var innovationsList = _initialGenomeBuilder.InitInnovationList(Parameters.InputLayerNeuronsCount, Parameters.OutputLayerNeuronsCount);

            for (var j = startIndex; j < Parameters.GenerationsPhaseOneCount; j++)
            {
                List<Genotype> population;

                if (!readFromFile)
                {
                    Console.Out.WriteLine($"Generation nr: {j}");
                    Console.Out.WriteLine("-------------------------------------");

                    var watch = new Stopwatch();
                    watch.Start();

                    population = new List<Genotype>();

                    for (var i = 0; i < Parameters.PopulationCount; ++i)
                    {
                        var neatBot = new NeatBot();
                        var g = j == 0 && startPopulation == null ? _initialGenomeBuilder.CreateInitialGenotype(Parameters.InputLayerNeuronsCount, Parameters.OutputLayerNeuronsCount) : parentPopulation[i];
                        var genotype = neatBot.TrainOneGameInArena(g);

                        // TODO czy potrzeba odpalac to w nowych watkach?
                        new Thread(delegate ()
                        {
                            Process.Start("client.exe", "PATH_BOT_1"); // TODO dodac poprawne sciezki do pliku z aplikacja i do plikow z botami
                        }).Start();

                        new Thread(delegate ()
                        {
                            Process.Start("client.exe", "PATH_BOT_1"); // TODO dodac poprawne sciezki do pliku z aplikacja i do plikow z botami
                        }).Start();

                        new Thread(delegate ()
                        {
                            Process.Start("client.exe", "PATH_BOT_1"); // TODO dodac poprawne sciezki do pliku z aplikacja i do plikow z botami
                        }).Start();

                        population.Add(genotype);
                    }

                    watch.Stop();
                    Console.Out.WriteLine($"FINISHED Generation nr: {j}. Time elapsed: {watch.ElapsedMilliseconds} ms");

                    population = parentPopulation;
                    ObjectManager.WriteGenerationToFile(population, j, Parameters.PopulationCount, "PHASE_TWO", Parameters.ActivationFunction.ToString(), 0);
                }
                else
                {
                    readFromFile = false;
                    population = parentPopulation;
                    Console.Out.WriteLine($"Generation {j} read from file.");
                }

                var maxValueInPopulation = population.Max(p => p.Value);
                var maxDifferenceBetweenBotAndBestEnemyInPopulation = population.Max(p => Math.Abs(p.ValuePhaseTwo[1] - p.ValuePhaseTwo[2]));

                foreach (var p in population)
                {
                    p.Value = 0.2*p.ValuePhaseTwo[0] + 0.5*p.ValuePhaseTwo[1]/maxValueInPopulation + 0.3*p.ValuePhaseTwo[2]/maxDifferenceBetweenBotAndBestEnemyInPopulation;
                }

                population = population.OrderByDescending(i => i.Value).ToList();
                var numberToTake = (int)(Parameters.PopulationCount * Parameters.BestOfPopulationPercentage);
                var halfBestOfPopulation = population.GetRange(0, numberToTake);

                var partBestPopulation1 = halfBestOfPopulation.Select(x => x.DeepCopy()).ToList();
                var partBestPopulation2 = halfBestOfPopulation.Select(x => x.DeepCopy()).ToList();

                var changedPartBestPopulation1 = _neatGeneticAlgorithm.CreateNewPopulationWithMutation(partBestPopulation1, ref innovationsList);
                var changedPartBestPopulation2 = _neatGeneticAlgorithm.CreateNewPopulationWithCrossover(partBestPopulation2);

                parentPopulation = new List<Genotype>();
                parentPopulation.AddRange(changedPartBestPopulation1.Select(x => x.DeepCopy()).ToList());
                parentPopulation.AddRange(changedPartBestPopulation2.Select(x => x.DeepCopy()).ToList());
            }

            ObjectManager.WriteToJsonFile(Parameters.TrainedModel, parentPopulation.OrderByDescending(g => g.Value).First());

            Console.Out.WriteLine("-------------------------------------PHASE ONE ENDED (map random on arena)-------------------------------------");
        }

        #endregion
    }
}
