using System;
using System.Collections.Generic;
using System.Linq;

namespace vindinium.Algorithms
{
    internal class EvaluationBot : Bot
    {

        public EvaluationBot(ServerStuff serverStuff) : base(serverStuff, "Evaluation") { }

        protected override string GetDirection()
        {
            var maxValue = -double.MaxValue;
            var bestDirection = Direction.Stay;

            var board = ServerStuff.Board;
            var myHero = ServerStuff.MyHero;
            var currPos = myHero.pos;

            var distanceToTaverns = GetDistancesToMines();
            var closestTavern = distanceToTaverns.Values.Min();

            // ------ STAY ------
            var x = currPos.x;
            var y = currPos.y;

            var value = EvaluateState(board[x][y], closestTavern);

            //if (value > maxValue)
            //{
            //    maxValue = value;
            //    bestDirection = Direction.Stay;
            //}

            // ------ LEFT ------
            x = currPos.x-1;
            y = currPos.y; 

            if(x >= 0)
                value = EvaluateState(board[x][y], closestTavern, new Pos() { x = x, y = y });

            if (value > maxValue)
            {
                maxValue = value;
                bestDirection = Direction.North;
            }

            // ------ RIGHT ------
            x = currPos.x+1;
            y = currPos.y;

            if (x < board.Length)
                value = EvaluateState(board[x][y], closestTavern, new Pos() { x = x, y = y });

            if (value > maxValue)
            {
                maxValue = value;
                bestDirection = Direction.South;
            }

            // ------ DOWN ------
            x = currPos.x;
            y = currPos.y-1;

            if (y >= 0)
                value = EvaluateState(board[x][y], closestTavern, new Pos() { x = x, y = y });

            if (value > maxValue)
            {
                maxValue = value;
                bestDirection = Direction.West;
            }

            // ------ UP ------
            x = currPos.x;
            y = currPos.y+1;

            if (y < board.Length)
                value = EvaluateState(board[x][y], closestTavern, new Pos() { x = x, y = y });

            if (value > maxValue)
            {
                bestDirection = Direction.East;
            }

            return bestDirection;
        }

        protected int GetManhattanDistance(Pos p1, Pos p2)
        {
            return Math.Abs(p1.x - p2.x) + Math.Abs(p1.y - p2.y);
        }

        protected List<Pos> GetAllMines()
        {
            var minesList = new List<Pos>();

            for (var i = 0; i < ServerStuff.Board.Length; ++i)
            {
                for (var j = 0; j < ServerStuff.Board.Length; ++j)
                {
                    if (ServerStuff.Board[i][j] == Tile.GOLD_MINE_NEUTRAL ||
                        //ServerStuff.Board[i][j] == Tile.GOLD_MINE_1 ||
                        ServerStuff.Board[i][j] == Tile.GOLD_MINE_2 ||
                        ServerStuff.Board[i][j] == Tile.GOLD_MINE_3 ||
                        ServerStuff.Board[i][j] == Tile.GOLD_MINE_4)
                    {
                        minesList.Add(new Pos {x = j, y = i});
                    }
                }
            }

            return minesList;
        }

        protected Dictionary<Pos, int> GetDistancesToMines(Pos from = null)
        {
            var minesList = GetAllMines();
            var dict = new Dictionary<Pos, int>();

            foreach (var t in minesList)
            {
                var x = GetManhattanDistance(from ?? ServerStuff.MyHero.pos, t);
                dict.Add(t, x);
            }

            return dict;
        }

        protected override double EvaluateState(Tile tile, int closestMine, Pos newPos = null)
        {
            var value = 0.0;

            if (newPos != null)
            {
                var distanceToMines = GetDistancesToMines(newPos);
                var closestMineNextState = distanceToMines.Values.Min();

                value += closestMineNextState < closestMine ? 10000 : -10000;
            }
            else
            {
                //value += closestTavern == 1 ? 1000 : 0;
            }
            
            var myHero = ServerStuff.MyHero;

            switch (tile)
            {
                case Tile.IMPASSABLE_WOOD:
                    value -= 100000;
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
                    throw new ArgumentOutOfRangeException(nameof(tile));
            }

            return value;
        }
    }
}
