namespace vindinium.Algorithm
{
    internal class NeatBot : Bot
    {

        public NeatBot(ServerStuff serverStuff) : base(serverStuff, "Neat") { }

        protected override string GetDirection()
        {
            // TODO put here code for genetic algorithm that returns one number
            // 0 - Stay 
            // 1 - West 
            // 2 - North 
            // 3 - East
            // 4 - South 

            var bestDirection = Direction.GetDirectionByNumber(0);

            return bestDirection;
        }
    }
}
