using System;
using System.Collections.Generic;
using System.IO;
using vindinium.NEAT;
using vindinium.Singletons;

namespace vindinium.Algorithm
{
    public class Playing
    {
        #region Playing

        public void Play(List<Genotype> bestGenotypes)
        {
            var result = "Hello and Welcome" + Environment.NewLine;

            var count = 1;

            foreach (var g in bestGenotypes)
            {
                var neatbot = new NeatBot(g);
                neatbot.Play();

                result += $"Game: {count} / Map size: {neatbot.GetBoardSize()}";
                result += Environment.NewLine;
                result += neatbot.GetInfoAboutGame();
                result += Environment.NewLine;

               count++;
            }

            File.WriteAllText(Parameters.DefaultPathToWrittenFiles + "ArenaLog" + DateTime.Now + ".txt", result);
        }

        public void Play(Genotype genotype)
        {
            var neatbot = new NeatBot(genotype);
            neatbot.Play(true);
        }

        #endregion
    }
}
