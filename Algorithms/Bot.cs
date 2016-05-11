using System;
using System.Diagnostics;
using System.Threading;

namespace vindinium.Algorithms
{
    internal abstract class Bot
    {
        protected readonly ServerStuff ServerStuff;
        private readonly string _botName;

        protected Bot(ServerStuff serverStuff, string botName = "Random")
        {
            ServerStuff = serverStuff;
            _botName = botName;
        }

        //starts everything
        public virtual void Run()
        {
            Console.Out.WriteLine(_botName + " bot running");

            ServerStuff.CreateGame();

            if (ServerStuff.Errored == false)
            {
                //opens up a webpage so you can view the game, doing it async so we dont time out
                new Thread(delegate()
                {
                    Process.Start(ServerStuff.ViewUrl);
                }).Start();
            }
            
            while (ServerStuff.Finished == false && ServerStuff.Errored == false)
            {
                ServerStuff.MoveHero(GetDirection());

                Console.Out.WriteLine("completed turn " + ServerStuff.CurrentTurn);
            }

            if (ServerStuff.Errored)
            {
                Console.Out.WriteLine("error: " + ServerStuff.ErrorText);
            }

            Console.Out.WriteLine(_botName + " bot finished");
        }

        protected abstract string GetDirection();

        protected abstract double EvaluateState(Tile tile, int closestMine, Pos newPos = null);
    }
}
