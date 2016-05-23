﻿using System;
using vindinium.Algorithm;
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
            Parameters.ServerSecretKey = args[0];
            if (args.Length >= 2) Parameters.ServerNumberOfTurns = uint.Parse(args[1]);
            if (args.Length >= 3) Parameters.ServerUrl = args[2]; // Parameters.CustomServerUrl
            if (args.Length >= 4) Parameters.PopulationCount = int.Parse(args[3]);
            if (args.Length >= 5) Parameters.BestOfPopulationPercentage = double.Parse(args[4]);
            if (args.Length >= 6) Parameters.GenerationsPhaseOneCount = int.Parse(args[5]);
            if (args.Length >= 7) Parameters.GenerationsPhaseTwoCount = int.Parse(args[6]);
            if (args.Length >= 8) Parameters.BestGenotypesOfPhaseOneCount = int.Parse(args[7]);
            if (args.Length >= 9) Parameters.ActivationFunction = (ActivationFunction)Enum.Parse(typeof(ActivationFunction), args[8], true);

            // ---------------------- Creating bot and run ----------------------

            var neatBot = new NeatBot();
            neatBot.Train();             // Training: only Phase One
            //neatBot.Train(true);         // Training: Phase One and Two
            //neatBot.Play();              // Playing on arena using Trained Model (required)
            //neatBot.TestGraphCompute();  // Test graph created in order to test Compute() function

            Console.Out.WriteLine("done");
            Console.Read();
        }
    }
}