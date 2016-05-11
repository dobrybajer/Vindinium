using System.Linq.Expressions;

namespace vindinium.Algorithms
{
    internal class RandomBot : Bot
    {

        public RandomBot(ServerStuff serverStuff) : base(serverStuff, "Random") { }

        protected override string GetDirection()
        {
            return Direction.GetRandomDirection();
        }

        protected override double EvaluateState(Tile tile, int closestMine, Pos newPos = null)
        {
            return 0.0;
        }
    }
}
