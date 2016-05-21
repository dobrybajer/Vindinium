using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using vindinium.PathFinding;

namespace vindinium.Algorithm
{
    public class Bot
    {
        protected readonly ServerStuff ServerStuff;

        #region Private fields
        
        private readonly string _botName;
        private MyPathNode[,] _pathGrid;
        private SpatialAStar<MyPathNode, object> _aStar;
        private readonly List<Pos> _allTavernsPositions;
        private readonly List<Pos> _allMinesPositions;

        #endregion

        #region Properties

        protected const int MaxBotHp = 100;

        protected int MaxBoardDistance { get; set; }


        #endregion

        #region Constructor and game initialization (do not change!)

        public Bot(ServerStuff serverStuff, string botName = "Evaluation")
        {
            ServerStuff = serverStuff;
            _botName = botName;
            MaxBoardDistance = (serverStuff.Board.Length - 1)*2;
            _allTavernsPositions = GetAllTaverns();
            _allMinesPositions = GetAllMines();
        }

        // TODO add possibility to not to run web page (only computation)
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

            InitializePathGrid();
            InitializeAStarModel();

            while (ServerStuff.Finished == false && ServerStuff.Errored == false)
            {
                ServerStuff.MoveHero(GetDirection());

                Console.Out.WriteLine("Completed turn " + ServerStuff.CurrentTurn);
            }

            if (ServerStuff.Errored)
            {
                Console.Out.WriteLine("Error: " + ServerStuff.ErrorText);
            }

            Console.Out.WriteLine(_botName + " bot finished");
        }

        public virtual void Train()
        {
            Run();
        }

        #endregion

        #region Methods for calculating closest distances to different objects on board (taverns, mines, enemies) that specify some criterias

        private List<Pos> GetAllMines(bool notNeutral = false)
        {
            var minesList = new List<Pos>();

            for (var i = 0; i < ServerStuff.Board.Length; ++i)
            {
                for (var j = 0; j < ServerStuff.Board.Length; ++j)
                {
                    if (!notNeutral && ServerStuff.Board[i][j] == Tile.GOLD_MINE_NEUTRAL || ServerStuff.Board[i][j] == Tile.GOLD_MINE_2 || ServerStuff.Board[i][j] == Tile.GOLD_MINE_3 || ServerStuff.Board[i][j] == Tile.GOLD_MINE_4)
                    {
                        minesList.Add(new Pos { x = i, y = j });
                    }
                }
            }

            return minesList;
        }

        /// <summary>
        /// Return distance to all given mines in manhattan distance. Method uses A* algorithm (or will be soon) to prevent from collision with objects.
        /// </summary>
        /// <param name="from">From where calculate distance to given mines (default: actual position of current hero).</param>
        /// <param name="notNeutral"></param>
        /// <returns>Distances to all given mines from given position.</returns>
        private Dictionary<Pos, int> GetDistancesToMines(Pos from = null, bool notNeutral = false)
        {
            var minesList = notNeutral ? GetAllMines(true) : _allMinesPositions;
            var dict = new Dictionary<Pos, int>();

            foreach (var t in minesList)
            {
                var x = FindPathLength(from ?? ServerStuff.MyHero.pos, t);
                dict.Add(t, x);
            }

            return dict;
        }

        protected double GetDistanceToClosestMine(Pos from = null, bool notNeutral = false)
        {
            var distanceToMines = GetDistancesToMines(from, notNeutral);
            return distanceToMines.Values.Min();
        }

        /// <summary>
        /// Get all taverns on the board.
        /// </summary>
        /// <returns>List of position of taverns.</returns>
        private List<Pos> GetAllTaverns()
        {
            var tavernsList = new List<Pos>();

            for (var i = 0; i < ServerStuff.Board.Length; ++i)
            {
                for (var j = 0; j < ServerStuff.Board.Length; ++j)
                {
                    if (ServerStuff.Board[i][j] == Tile.TAVERN)
                    {
                        tavernsList.Add(new Pos { x = i, y = j });
                    }
                }
            }

            return tavernsList;
        }

        private Dictionary<Pos, int> GetDistancesToTaverns(Pos from = null)
        {
            var dict = new Dictionary<Pos, int>();

            foreach (var t in _allTavernsPositions)
            {
                var x = FindPathLength(from ?? ServerStuff.MyHero.pos, t);
                dict.Add(t, x);
            }

            return dict;
        }

        protected double GetDistanceToClosestTavern(Pos from = null, bool paremeterNotUsesButNeeded = false)
        {
            var distanceToTaverns = GetDistancesToTaverns(from);
            return distanceToTaverns.Values.Min();
        }

        protected Dictionary<Pos, Hero> GetAllEnemies()
        {
            var enemiesList = new Dictionary<Pos, Hero>();

            for (var i = 0; i < ServerStuff.Board.Length; ++i)
            {
                for (var j = 0; j < ServerStuff.Board.Length; ++j)
                {
                    if (ServerStuff.Board[i][j] == Tile.HERO_2) enemiesList[new Pos {x = i, y = j}] = ServerStuff.Heroes[1];
                    if (ServerStuff.Board[i][j] == Tile.HERO_3) enemiesList[new Pos {x = i, y = j}] = ServerStuff.Heroes[2];
                    if (ServerStuff.Board[i][j] == Tile.HERO_4) enemiesList[new Pos {x = i, y = j}] = ServerStuff.Heroes[3];
                }
            }

            return enemiesList;
        }

        protected Dictionary<Hero, int> GetDistancesToEnemies(Pos from = null)
        {
            var enemiesList = GetAllEnemies();
            var dict = new Dictionary<Hero, int>();

            foreach (var t in enemiesList)
            {
                var x = FindPathLength(from ?? ServerStuff.MyHero.pos, t.Key);
                dict.Add(t.Value, x);
            }

            return dict;
        }

        protected double GetDistanceToClosestEnemy(Pos pos = null, int? numberOfEnemy = null)
        {
            var distanceToEnemies = GetDistancesToEnemies(pos);

            if (!numberOfEnemy.HasValue) return distanceToEnemies.Values.Min();

            var specificEnemy = distanceToEnemies.Keys.First(e => e.id == numberOfEnemy.Value);
            return distanceToEnemies[specificEnemy];
        }

        #endregion

        #region Protected and virtual methods

        /// <summary>
        /// Main method that gives information where bot should move (stay, west, north, east, south).
        /// </summary>
        /// <returns>Name of direction to move.</returns>
        protected virtual string GetDirection()
        {
            // only for debug reasons
            if (ServerStuff.CurrentTurn == 12)
            {
                Console.WriteLine("DEBUG - stop breakpoint");
            }

            var value = double.MinValue;
            var maxValue = double.MinValue;
            var bestDirection = Direction.Stay;
            
            var board = ServerStuff.Board;
            var myHero = ServerStuff.MyHero;
            var currPos = new Pos {x = myHero.pos.y, y = myHero.pos.x};

            var closestMine = (int)GetDistanceToClosestMine();

            // ------ LEFT ------
            var x = currPos.x - 1;
            var y = currPos.y;

            if (x >= 0)
                value = EvaluateState(board[x][y], closestMine, new Pos{ x = x, y = y });

            if (value > maxValue)
            {
                maxValue = value;
                bestDirection = Direction.West;
            }

            // ------ RIGHT ------
            x = currPos.x + 1;
            y = currPos.y;

            if (x < board.Length)
                value = EvaluateState(board[x][y], closestMine, new Pos{ x = x, y = y });

            if (value > maxValue)
            {
                maxValue = value;
                bestDirection = Direction.East;
            }

            // ------ UP ------
            x = currPos.x;
            y = currPos.y - 1;

            if (y >= 0)
                value = EvaluateState(board[x][y], closestMine, new Pos { x = x, y = y });

            if (value > maxValue)
            {
                maxValue = value;
                bestDirection = Direction.North;
            }

            // ------ DOWN ------
            x = currPos.x;
            y = currPos.y + 1;

            if (y < board.Length)
                value = EvaluateState(board[x][y], closestMine, new Pos{ x = x, y = y });

            if (value > maxValue)
            {
                bestDirection = Direction.South;
            }

            Console.WriteLine(bestDirection);

            return bestDirection;
        }

        protected virtual string GetDirectionGeneric<TSecondPar>(Func<Pos, TSecondPar, double> delegateFunction, TSecondPar par)
        {
            double value;
            var bestDirection = Direction.Stay;

            var board = ServerStuff.Board;
            var myHero = ServerStuff.MyHero;
            var currPos = new Pos { x = myHero.pos.y, y = myHero.pos.x };

            var closestObject = delegateFunction(null, par);

            // ------ LEFT ------
            var x = currPos.x - 1;
            var y = currPos.y;

            if (x >= 0)
            {
                value = delegateFunction(new Pos { x = x, y = y }, par);
                if (value < closestObject) bestDirection = Direction.West;
            }
            
            // ------ RIGHT ------
            x = currPos.x + 1;
            y = currPos.y;

            if (x < board.Length)
            {
                value = delegateFunction(new Pos { x = x, y = y }, par);
                if (value < closestObject) bestDirection = Direction.East;
            }
            
            // ------ UP ------
            x = currPos.x;
            y = currPos.y - 1;

            if (y >= 0)
            {
                value = delegateFunction(new Pos { x = x, y = y }, par);
                if (value < closestObject) bestDirection = Direction.North;
            }
            
            // ------ DOWN ------
            x = currPos.x;
            y = currPos.y + 1;

            if (y < board.Length)
            {
                value = delegateFunction(new Pos { x = x, y = y }, par);
                if (value < closestObject) bestDirection = Direction.South;
            }
            
            return bestDirection;
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
            var myHero = ServerStuff.MyHero;
            var value = 0.0;

            if (newPos != null)
            {
                var distanceToMines = GetDistancesToMines(newPos);
                var closestMineNextState = distanceToMines.Aggregate((l, r) => l.Value < r.Value ? l : r);

                value += closestMineNextState.Value < closestMine && tile == Tile.FREE || closestMineNextState.Key.x == newPos.x && closestMineNextState.Key.y == newPos.y ? 10000 : -10000;
            }
            
            switch (tile)
            {
                case Tile.IMPASSABLE_WOOD:
                    value += int.MinValue;
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

        #endregion

        #region Algorithm A*

        /// <summary>
        /// Initializes a grid used for A* path finding.
        /// </summary>
        private void InitializePathGrid()
        {
            _pathGrid = new MyPathNode[ServerStuff.Board.Length, ServerStuff.Board.Length];

            for (var x = 0; x < ServerStuff.Board.Length; x++)
                for (var y = 0; y < ServerStuff.Board.Length; y++)
                {
                    var isWall = ServerStuff.Board[x][y] != Tile.FREE;

                    _pathGrid[x, y] = new MyPathNode()
                    {
                        IsWall = isWall,
                        X = x,
                        Y = y,
                    };
                }
        }

        /// <summary>
        /// Updates a grid used for A* path finding. Only HEROES position are updated TODO, so it could be optimized.
        /// </summary>
        private void UpdatePathGrid()
        {
            for (var x = 0; x < ServerStuff.Board.Length; x++)
                for (var y = 0; y < ServerStuff.Board.Length; y++)
                    _pathGrid[x, y].IsWall = ServerStuff.Board[x][y] != Tile.FREE;
        }

        /// <summary>
        /// Initializes model for A* algorithm. TODO Could it be set once, or does it need to be initialized after every _pathGrid update?
        /// </summary>
        private void InitializeAStarModel()
        {
            _aStar = new SpatialAStar<MyPathNode, object>(_pathGrid);
        }

        /// <summary>
        /// Algorithm approximating nearest position from position to position (including impassable tiles).
        /// Algorithm used is A* with manhattan metric. 
        /// Distance is measured in number of moves needed to reach goal (including start and end points).
        /// </summary>
        /// <param name="from">Position from.</param>
        /// <param name="to">Position to.</param>
        /// <returns>Smallest distance between to positions.</returns>
        private int FindPathLength(Pos from, Pos to)
        {
            UpdatePathGrid();
            InitializeAStarModel();

            var path = _aStar.Search(from, to, null);

            return path?.Count ?? int.MaxValue;
        }

        #endregion
    }
}
