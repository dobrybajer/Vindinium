using System;
using vindinium.Algorithms;

namespace vindinium
{
    class Client
    {
        /**
         * Launch client.
         * @param args args[0] Private key
         * @param args args[1] [training|arena]
         * @param args args[2] number of turns
         * @param args args[3] HTTP URL of Vindinium server (optional)
         */
        static void Main(string[] args)
        {
            var serverUrl = args.Length == 4 ? args[3] : "http://vindinium.org";

            //create the server stuff, when not in training mode, it doesnt matter
            //what you use as the number of turns
            var serverStuff = new ServerStuff(args[0], args[1] != "arena", uint.Parse(args[2]), serverUrl, null);

            //create the random bot, replace this with your own bot
            var bot = new RandomBot(serverStuff);
            
            //create the evaluation
            //var bot = new EvaluationBot(serverStuff);

            //now kick it all off by running the bot.
            bot.Run();

            Console.Out.WriteLine("done");

            Console.Read();
        }
    }
}
