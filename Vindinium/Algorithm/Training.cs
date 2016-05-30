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

                    Parallel.For(0, 6, i =>
                    {
                        var neatBot = new NeatBot(i, map);
                        var parentGenotype = parallelParentPopulation[i];
                        var genotype = neatBot.TrainOneGame(parentGenotype);
                        lock (syncLock)
                        {
                            parallelPopulation.Add(genotype);
                        }
                    });

                    Parallel.For(6, 12, i =>
                    {
                        var neatBot = new NeatBot(i, map);
                        var parentGenotype = parallelParentPopulation[i];
                        var genotype = neatBot.TrainOneGame(parentGenotype);
                        lock (syncLock)
                        {
                            parallelPopulation.Add(genotype);
                        }
                    });

                    Parallel.For(12, 18, i =>
                    {
                        var neatBot = new NeatBot(i, map);
                        var parentGenotype = parallelParentPopulation[i];
                        var genotype = neatBot.TrainOneGame(parentGenotype);
                        lock (syncLock)
                        {
                            parallelPopulation.Add(genotype);
                        }
                    });

                    Parallel.For(18, 24, i =>
                    {
                        var neatBot = new NeatBot(i, map);
                        var parentGenotype = parallelParentPopulation[i];
                        var genotype = neatBot.TrainOneGame(parentGenotype);
                        lock (syncLock)
                        {
                            parallelPopulation.Add(genotype);
                        }
                    });

                    Parallel.For(24, Parameters.PopulationCount, i =>
                    {
                        var neatBot = new NeatBot(i, map);
                        var parentGenotype = parallelParentPopulation[i];
                        var genotype = neatBot.TrainOneGame(parentGenotype);
                        lock (syncLock)
                        {
                            parallelPopulation.Add(genotype);
                        }
                    });

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

        // TODO finish this method
        private void TrainPhaseTwo(List<Genotype> startPopulation)
        {
            // create appropriate serverstuff
            var parentPopulation = startPopulation;
            ObjectManager.WriteToJsonFile(Parameters.TrainedModel, parentPopulation.First());
        }

        #endregion
    }
}
