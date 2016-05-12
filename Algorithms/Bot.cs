using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace vindinium.Algorithms
{
    internal class Bot
    {
        protected readonly ServerStuff ServerStuff;
        private readonly string _botName;

        public Bot(ServerStuff serverStuff, string botName = "Evaluation")
        {
            ServerStuff = serverStuff;
            _botName = botName;
        }

        /// <summary>
        /// Starts everything (do not change!).
        /// </summary>
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

        /// <summary>
        /// Main method that gives information where bot should move (stay, west, north, east, south).
        /// </summary>
        /// <returns>Name of direction to move.</returns>
        protected virtual string GetDirection()
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
            x = currPos.x - 1;
            y = currPos.y;

            if (x >= 0)
                value = EvaluateState(board[x][y], closestTavern, new Pos() { x = x, y = y });

            if (value > maxValue)
            {
                maxValue = value;
                bestDirection = Direction.North;
            }

            // ------ RIGHT ------
            x = currPos.x + 1;
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
            y = currPos.y - 1;

            if (y >= 0)
                value = EvaluateState(board[x][y], closestTavern, new Pos() { x = x, y = y });

            if (value > maxValue)
            {
                maxValue = value;
                bestDirection = Direction.West;
            }

            // ------ UP ------
            x = currPos.x;
            y = currPos.y + 1;

            if (y < board.Length)
                value = EvaluateState(board[x][y], closestTavern, new Pos() { x = x, y = y });

            if (value > maxValue)
            {
                bestDirection = Direction.East;
            }

            return bestDirection;
        }

        /// <summary>
        /// Get all mines on the board that are not belong to current hero.
        /// </summary>
        /// <returns>List of position of mines.</returns>
        protected virtual List<Pos> GetAllMines()
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
                        minesList.Add(new Pos { x = j, y = i });
                    }
                }
            }

            return minesList;
        }

        /// <summary>
        /// Return distance to all given mines in manhattan distance. Method uses A* algorithm (or will be soon) to prevent from collision with objects.
        /// </summary>
        /// <param name="from">From where calculate distance to given mines (default: actual position of current hero).</param>
        /// <returns>Distances to all given mines from given position.</returns>
        protected virtual Dictionary<Pos, int> GetDistancesToMines(Pos from = null)
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

        /// <summary>
        /// Method evaluating state of current position in board. Needs to be improved experimentically.
        /// </summary>
        /// <param name="tile">Board element's type.</param>
        /// <param name="closestMine">Manhattan distance to closest mine calculated from actual position.</param>
        /// <param name="newPos">Position of next state.</param>
        /// <returns></returns>
        protected virtual double EvaluateState(Tile tile, int closestMine, Pos newPos = null)
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
                    value += myHero.life < 30 && myHero.gold > 2 ? 200 : 10;
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

        /// <summary>
        /// Returns manhattan distance between 2 positions.
        /// </summary>
        /// <param name="from">Position from.</param>
        /// <param name="to">Position to.</param>
        /// <returns>Distance between positions.</returns>
        protected int GetManhattanDistance(Pos from, Pos to)
        {
            return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y);
        }

        /// <summary>
        /// Algorithm approximating nearest position from position to position (including impassable tiles)
        /// </summary>
        /// <param name="from">Position from.</param>
        /// <param name="to">Position to.</param>
        /// <returns>Smallest distance between to positions.</returns>
        protected int Astar(Pos from, Pos to)
        {
            throw new NotImplementedException();
        }
    }
}
