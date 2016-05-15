using System;
using vindinium.Algorithm;

namespace vindinium
{
    internal class Client
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
            var serverUrl = args.Length == 4 ? args[3] : "http://vindinium.org";

            //create the server stuff, when not in training mode, it doesnt matter what you use as the number of turns
            var serverStuff = new ServerStuff(args[0], args[1] != "arena", uint.Parse(args[2]), serverUrl, "m2");


            // ---------------------- OWN CODE ----------------------

            var bot = new Bot(serverStuff);
            var randomBot = new RandomBot(serverStuff);
            var neatBot = new NeatBot(serverStuff);

            bot.Run();
            
            // -------------------- END OWN CODE --------------------


            Console.Out.WriteLine("done");

            Console.Read();
        }
    }
}
