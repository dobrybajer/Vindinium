using vindinium.Algorithm;

namespace vindinium.Singletons
{
    public static class Parameters
    {
        public static int InputLayerNeuronsCount { get; set; } = 9;

        public static int OutputLayerNeuronsCount { get; set; } = 6;

        public static int PopulationCount { get; set; } = 50;

        public static double BestOfPopulationPercentage { get; set; } = 0.5;

        public static int GenerationsPhaseOneCount { get; set; } = 10;

        public static int BestGenotypesOfPhaseOneCount { get; set; } = 6;

        public static int GenerationsPhaseTwoCount { get; set; } = 10;

        public static ActivationFunction ActivationFunction { get; set; } = ActivationFunction.Linear;

        public const string DefaultPathToWrittenFiles = "../../CreatedObjects/";
        public const string TrainedPhaseOneMap1 = "PhaseOneBot1.txt";
        public const string TrainedPhaseOneMap2 = "PhaseOneBot2.txt";
        public const string TrainedPhaseOneMap3 = "PhaseOneBot3.txt";
        public const string TrainedPhaseOneMap4 = "PhaseOneBot4.txt";
        public const string TrainedPhaseOneMap5 = "PhaseOneBot5.txt";
        public const string TrainedPhaseOneMap6 = "PhaseOneBot6.txt";
        public const string TrainedModel = "VindiniumBot.txt";

        public const string DefaultServerUrl = "http://vindinium.org";
        public const string CustomServerUrl = "http://192.168.0.18:9000";

        public static string ServerSecretKey { get; set; }
        public static bool ServerTrainingMode { get; set; } = true;
        public static string ServerUrl { get; set; } = DefaultServerUrl;
        public static uint ServerNumberOfTurns { get; set; } = 10;
    }
}
