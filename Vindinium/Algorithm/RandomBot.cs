namespace vindinium.Algorithm
{
    public class RandomBot : Bot
    {

        public RandomBot(ServerStuff serverStuff) : base(serverStuff, "Random") { }

        protected override string GetDirection()
        {
            return Direction.GetRandomDirection();
        }
    }
}
