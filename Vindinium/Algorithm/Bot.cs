using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using vindinium.PathFinding;

namespace vindinium.Algorithm
{
    public enum ActivationFunction
    {
        Linear,            // result from 0 to 1 (increasing value)
        Unipolar,          // result 0 or 1 (increasing value)
        Sigmoid,           // result from 0.02 to 0.98 (increasing value)
        ArcTangens,        // result from 0 to 0.98 (increasing value)
        Gaussian,          // result from 0.05 to 1 (decreasing value)
        HiperbolicTangens  // result from 0 to 0.99 (increasing value)
    }

    public class Bot
    {
        #region Private fields
        
        private readonly string _botName;
        private MyPathNode[,] _pathGrid;
        private SpatialAStar<MyPathNode, object> _aStar;
        private List<Pos> _allTavernsPositions;
        
        #endregion

        #region Properties

        protected const int MaxBotHp = 100;

        protected int MaxBoardDistance { get; set; }

        protected ServerStuff ServerStuff { get; set; }

        protected int DeathCount { get; set; }

        protected readonly int CurrentGenotype;

        #endregion

        #region Constructor and game initialization (do not change!)

        public Bot(int genotypeNumber = 0, string botName = "Evaluation")
        {
            _botName = botName;
            CurrentGenotype = genotypeNumber;
        }

        private void Initialize()
        {
            MaxBoardDistance = (ServerStuff.Board.Length - 1) * 2;
            _allTavernsPositions = GetAllTaverns();
        }

        /// <summary>
        /// Starts everything (do not change!).
        /// </summary>
        public virtual void Run(bool onlyComputation = false)
        {
            //Console.Out.WriteLine($"Genotype: {CurrentGenotype}. Bot {_botName} running");

            ServerStuff.CreateGame();
            
            if (!onlyComputation && ServerStuff.Errored == false)
            {
                //opens up a webpage so you can view the game, doing it async so we dont time out
                new Thread(delegate()
                {
                    Process.Start(ServerStuff.ViewUrl);
                }).Start();
            }
            else
            {
                Console.Out.WriteLine($"Hello, it is {ServerStuff.MyHero.name}");
            }

            Initialize();
            InitializePathGrid();
            InitializeAStarModel();

            var life = 100;

            while (ServerStuff.Finished == false && ServerStuff.Errored == false)
            {
                //var watch = new Stopwatch();
                //if (onlyComputation) watch.Start();
                var direction = GetDirection();
                
                //if (onlyComputation)
                //{
                    //watch.Stop();
                    //Console.Out.WriteLine($"Genotype: {CurrentGenotype}. Computation time: {watch.ElapsedMilliseconds} ms");
                //}

                ServerStuff.MoveHero(direction);

                if (Math.Abs(life - ServerStuff.MyHero.life) > 50) DeathCount++;
                life = ServerStuff.MyHero.life;

                //Console.Out.WriteLine($"Genotype: {CurrentGenotype}. Completed turn " + ServerStuff.CurrentTurn);
            }

            if (ServerStuff.Errored)
            {
                Console.Out.WriteLine($"Genotype: {CurrentGenotype}. Error: " + ServerStuff.ErrorText);
            }

            //Console.Out.WriteLine($"Genotype: {CurrentGenotype}. Bot {_botName} finished");
        }

        #endregion

        #region Methods for calculating closest distances to different objects on board (taverns, mines, enemies) that specify some criterias

        private IEnumerable<Pos> GetAllMines(bool notNeutral = false)
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
            // TODO what if we own all mines? Then there is no place to go and we will stay. Should be handled somehow, but possibility of this situation is rather small
            var minesList = GetAllMines(notNeutral && ServerStuff.Heroes.Where(h => h.id != 0).Sum(h => h.mineCount) > 0);

            var dict = new Dictionary<Pos, int>();

            foreach (var t in minesList)
            {
                var x = FindPathLength(from ?? new Pos { x = ServerStuff.MyHero.pos.y, y = ServerStuff.MyHero.pos.x }, t);
                dict.Add(t, x);
            }

            return dict;
        }

        protected double GetDistanceToClosestMine(Pos from = null, bool notNeutral = false)
        {
            var distanceToMines = GetDistancesToMines(from, notNeutral);
            return distanceToMines.Count > 0 ? distanceToMines.Values.Min() : MaxBoardDistance;
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
                var x = FindPathLength(from ?? new Pos { x = ServerStuff.MyHero.pos.y, y = ServerStuff.MyHero.pos.x }, t);
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
                var x = FindPathLength(from ?? new Pos { x = ServerStuff.MyHero.pos.y, y = ServerStuff.MyHero.pos.x } , t.Key);
                dict.Add(t.Value, x);
            }

            return dict;
        }

        protected double GetDistanceToClosestEnemy(Pos pos = null, int? option = null)
        {
            var distanceToEnemies = GetDistancesToEnemies(pos);

            if (!option.HasValue) return distanceToEnemies.Values.Min();

            Hero specificEnemy;
            switch (option.Value)
            {
                case 1:
                    specificEnemy = distanceToEnemies.Keys.Aggregate((i, j) => i.gold > j.gold ? i : j);
                    break;
                case 2:
                    specificEnemy = distanceToEnemies.Keys.Aggregate((i, j) => i.life < j.life ? i : j);
                    break;
                case 3:
                    specificEnemy = distanceToEnemies.Keys.Aggregate((i, j) => i.mineCount > j.mineCount ? i : j);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

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
                if (value < closestObject && (closestObject <= 2 || board[x][y] == Tile.FREE))
                {
                    bestDirection = Direction.West;
                    closestObject = value;
                }
            }
            
            // ------ RIGHT ------
            x = currPos.x + 1;
            y = currPos.y;

            if (x < board.Length)
            {
                value = delegateFunction(new Pos { x = x, y = y }, par);
                if (value < closestObject && (closestObject <= 2 || board[x][y] == Tile.FREE))
                {
                    bestDirection = Direction.East;
                    closestObject = value;
                }
            }
            
            // ------ UP ------
            x = currPos.x;
            y = currPos.y - 1;

            if (y >= 0)
            {
                value = delegateFunction(new Pos { x = x, y = y }, par);
                if (value < closestObject && (closestObject <= 2 || board[x][y] == Tile.FREE))
                {
                    bestDirection = Direction.North;
                    closestObject = value;
                }
            }
            
            // ------ DOWN ------
            x = currPos.x;
            y = currPos.y + 1;

            if (y < board.Length)
            {
                value = delegateFunction(new Pos { x = x, y = y }, par);
                if (value < closestObject && (closestObject <= 2 || board[x][y] == Tile.FREE))
                    bestDirection = Direction.South;
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
