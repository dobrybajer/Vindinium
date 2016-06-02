using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using vindinium.Algorithm;
using vindinium.NEAT;
using vindinium.Singletons;

namespace vindinium
{
    public class Client
    {
        private static void Main(string[] args)
        {
            // ----------------------- Setting Parameters -----------------------
            if (args.Length == 0) throw new Exception("Private key missing in program parameters... Ending.");

            if(args[0].Length < 10)
            { 
                Parameters.ServerSecretKey = args[0];
                if (args.Length >= 2) Parameters.ServerNumberOfTurns = uint.Parse(args[1]);
                if (args.Length >= 3) Parameters.ServerUrl = args[2]; // Parameters.CustomServerUrl
                if (args.Length >= 4) Parameters.PopulationCount = int.Parse(args[3]);
                if (args.Length >= 5) Parameters.BestOfPopulationPercentage = double.Parse(args[4], CultureInfo.InvariantCulture);
                if (args.Length >= 6) Parameters.GenerationsPhaseOneCount = int.Parse(args[5]);
                if (args.Length >= 7) Parameters.GenerationsPhaseTwoCount = int.Parse(args[6]);
                if (args.Length >= 8) Parameters.BestGenotypesOfPhaseOneCount = int.Parse(args[7]);
                if (args.Length >= 9) Parameters.ActivationFunction = (ActivationFunction)Enum.Parse(typeof(ActivationFunction), args[8], true);
                if (args.Length >= 10) Parameters.AddConnectionMutationProbablity = double.Parse(args[9], CultureInfo.InvariantCulture);
                if (args.Length >= 11) Parameters.DeleteConnectionMutationProbablity = double.Parse(args[10], CultureInfo.InvariantCulture);
                if (args.Length >= 12) Parameters.AddNodeMutationProbablity = double.Parse(args[11], CultureInfo.InvariantCulture);
                if (args.Length >= 13) Parameters.ConnectionWeightMutationProbablity = double.Parse(args[12], CultureInfo.InvariantCulture);
                if (args.Length >= 14) Parameters.MutationWheelPart = double.Parse(args[13], CultureInfo.InvariantCulture);
                if (args.Length >= 15) Parameters.CrossoverWheelPart = double.Parse(args[14], CultureInfo.InvariantCulture);

                // ---------------------- Creating bot and run ----------------------

                Parameters.ServerUrl = Parameters.CustomServerUrl;

                var training = new Training();
                training.Train(false, true);

                //var g = ObjectManager.ReadFromJsonFile<List<Genotype>>("generation29_populationCount30_m1_activationFunctionLinear_turns150.txt");
                //var playing = new Playing();
                //playing.Play(g.OrderByDescending(x => x.Value).Take(5).ToList());
            }
            else
            {
                Console.Out.WriteLine("Started training bot as a new proces...");

                Parameters.ServerSecretKey = "8pe3wfos"; // TODO na serwerze stworzyc nowego bota i wpisac tu na sztywno jego wygenerowany klucz
                Parameters.ServerUrl = Parameters.CustomServerUrl;

                var genotype = ObjectManager.ReadFromJsonFile<Genotype>(args[0]); // TODO tu ma byc podawana (jako argument uruchomienia exe) sciezka do pliku z botem z danej mapy trenowanym w fazie 1 (ścieżka względna w stousnku do folderu "CreatedObjects/")

                var playing = new Playing();
                playing.Play(genotype);

                Console.Out.WriteLine("Ending training bot as a new proces...");
            }

            Console.Read();
        }

        private static void Play()
        {
            var bestGenotypesBuilder = new BestGenerationBuilder();
            bestGenotypesBuilder.Build();
            var bestGenotypes = new List<Genotype>();
            foreach (var key in bestGenotypesBuilder.FilesPerMapDictionary.Keys)
            {
                var genotype = bestGenotypesBuilder.FilesPerMapDictionary[key];
                if (genotype != null)
                    bestGenotypes.Add(genotype);
            }
            var playing = new Playing();
            playing.Play(bestGenotypes);
        }
    }
}