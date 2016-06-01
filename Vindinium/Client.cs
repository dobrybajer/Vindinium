using System;
using System.Collections.Generic;
using System.Globalization;
using vindinium.Algorithm;
using vindinium.NEAT;
using vindinium.Singletons;

namespace vindinium
{
    public class Client
    {
        /**
         * Launch client.
         * @param args args[0] Private key
         * @param args args[1] [training|arena]
         * @param args args[2] number of turns
         * @param args args[3] HTTP URL of Vindinium server (optional)
         */

        private static void Main(string[] args)
        {
            // ----------------------- Setting Parameters -----------------------

            if (args.Length == 0) throw new Exception("Private key missing in program parameters... Ending.");
            Parameters.ServerSecretKey = args[0];//"e5ua10cb";
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

            //var neatBot = new NeatBot();
            //neatBot.Train();             // Training: only Phase One
            //neatBot.Train(true);         // Training: Phase One and Two
            //neatBot.Play();              // Playing on arena using Trained Model (required)
            //neatBot.TestGraphCompute();  // Test graph created in order to test Compute() function

            //var training = new Training();
            //training.Train();
            Play();

            Console.Out.WriteLine("done");
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