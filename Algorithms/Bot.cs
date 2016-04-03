using System;
using System.Data;
using System.Diagnostics;
using System.Threading;

namespace vindinium.Algorithms
{
    abstract class Bot
    {
        protected readonly ServerStuff ServerStuff;
        private readonly string _botName;

        protected Bot(ServerStuff serverStuff, string botName = "random")
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

        protected virtual string GetDirection()
        {
            var random = new Random();
            switch (random.Next(0, 6))
            {
                case 0:
                    return Direction.East;
                case 1:
                    return Direction.North;
                case 2:
                    return Direction.South;
                case 3:
                    return Direction.Stay;
                case 4:
                    return Direction.West;
                default:
                    return Direction.Stay; 
            }
        }

        protected virtual double EvaluateState(Tile tile)
        {
            return 0.0;
        }
    }
}
