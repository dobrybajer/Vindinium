using System;

namespace vindinium.Algorithms
{
    class EvaluationBot : Bot
    {

        public EvaluationBot(ServerStuff serverStuff)
            : base(serverStuff, "Evaluation")
        {

        }

        protected override string GetDirection()
        {
            var maxValue = -double.MaxValue;
            var bestDirection = Direction.Stay;
            var value = 0.0;

            var board = ServerStuff.Board;
            var myHero = ServerStuff.MyHero;
            var currPos = myHero.pos;

            // ------ LEFT ------
            var x = currPos.x--;
            var y = currPos.y; 

            if(x >= 0)
                value = EvaluateState(board[x][y]);

            if (value > maxValue)
            {
                maxValue = value;
                bestDirection = Direction.West;
            }

            // ------ RIGHT ------
            x = currPos.x++;
            y = currPos.y;

            if (x < board.Length)
                value = EvaluateState(board[x][y]);

            if (value > maxValue)
            {
                maxValue = value;
                bestDirection = Direction.East;
            }

            // ------ DOWN ------
            x = currPos.x;
            y = currPos.y--;

            if (x >= 0)
                value = EvaluateState(board[x][y]);

            if (value > maxValue)
            {
                maxValue = value;
                bestDirection = Direction.South;
            }

            // ------ UP ------
            x = currPos.x;
            y = currPos.y++;

            if (y < board.Length)
                value = EvaluateState(board[x][y]);

            if (value > maxValue)
            {
                maxValue = value;
                bestDirection = Direction.North;
            }

            return bestDirection;
        }

        // TODO add manhattan distance function and use it here to promote getting closer to gold mine
        protected override double EvaluateState(Tile tile)
        {
            var value = 0.0;

            var myHero = ServerStuff.MyHero;

            switch (tile)
            {
                case Tile.IMPASSABLE_WOOD:
                    value -= 100;
                    break;
                case Tile.FREE:
                    value += 5;
                    break;
                case Tile.HERO_1:
                    value += ServerStuff.Heroes[0].life < 40 && myHero.life > 40 ? 100 : -100;
                    break;
                case Tile.HERO_2:
                    value += ServerStuff.Heroes[1].life < 40 && myHero.life > 40 ? 100 : -100;
                    break;
                case Tile.HERO_3:
                    value += ServerStuff.Heroes[2].life < 40 && myHero.life > 40 ? 100 : -100;
                    break;
                case Tile.HERO_4:
                    value += ServerStuff.Heroes[3].life < 40 && myHero.life > 40 ? 100 : -100;
                    break;
                case Tile.TAVERN:
                    value += myHero.life < 30  && myHero.gold > 2 ? 200 : 10;
                    break;
                case Tile.GOLD_MINE_NEUTRAL:
                    value += myHero.life > 30 && myHero.gold > 2 ? 300 : -100;
                    break;
                case Tile.GOLD_MINE_1:
                    value += myHero.life > 30 && myHero.gold > 2 ? 500 : -100;
                    break;
                case Tile.GOLD_MINE_2:
                    value += myHero.life > 30 && myHero.gold > 2 ? 500 : -100;
                    break;
                case Tile.GOLD_MINE_3:
                    value += myHero.life > 30 && myHero.gold > 2 ? 500 : -100;
                    break;
                case Tile.GOLD_MINE_4:
                    value += myHero.life > 30 && ServerStuff.MyHero.gold > 2 ? 500 : -100;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("tile");
            }

            return value;
        }
    }
}
