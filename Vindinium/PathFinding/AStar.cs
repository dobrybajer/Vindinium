using System;
using System.Collections.Generic;

namespace vindinium.PathFinding
{
    public class SpatialAStar<TPathNode, TUserContext> where TPathNode : IPathNode<TUserContext>
    {
        private readonly OpenCloseMap _mClosedSet;
        private readonly OpenCloseMap _mOpenSet;
        private readonly PriorityQueue<PathNode> _mOrderedOpenSet;
        private readonly PathNode[,] _mCameFrom;
        private readonly OpenCloseMap _mRuntimeGrid;
        private readonly PathNode[,] _mSearchSpace;

        public TPathNode[,] SearchSpace { get; private set; }

        public int Width { get; }

        public int Height { get; }
        
        public SpatialAStar(TPathNode[,] inGrid)
        {
            SearchSpace = inGrid;
            Width = inGrid.GetLength(0);
            Height = inGrid.GetLength(1);
            _mSearchSpace = new PathNode[Width, Height];
            _mClosedSet = new OpenCloseMap(Width, Height);
            _mOpenSet = new OpenCloseMap(Width, Height);
            _mCameFrom = new PathNode[Width, Height];
            _mRuntimeGrid = new OpenCloseMap(Width, Height);
            _mOrderedOpenSet = new PriorityQueue<PathNode>(PathNode.Comparer);

            for (var x = 0; x < Width; x++)
                for (var y = 0; y < Height; y++)
                {
                    if (inGrid[x, y] == null) throw new ArgumentNullException();

                    _mSearchSpace[x, y] = new PathNode(x, y, inGrid[x, y]);
                }
        }

        protected virtual double Heuristic(PathNode inStart, PathNode inEnd)
        {
            return Math.Abs(inStart.X - inEnd.X) + Math.Abs(inStart.Y - inEnd.Y);
        }

        protected virtual double NeighborDistance(PathNode inStart, PathNode inEnd)
        {
            return Heuristic(inStart, inEnd);
        }

        internal LinkedList<TPathNode> Search(Pos inStartNode, Pos inEndNode, TUserContext inUserContext)
        {
            var startNode = _mSearchSpace[inStartNode.x, inStartNode.y];
            var endNode = _mSearchSpace[inEndNode.x, inEndNode.y];

            if (startNode == endNode)
                return new LinkedList<TPathNode>(new[] { startNode.UserContext });

            var neighborNodes = new PathNode[4];

            _mClosedSet.Clear();
            _mOpenSet.Clear();
            _mRuntimeGrid.Clear();
            _mOrderedOpenSet.Clear();

            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    _mCameFrom[x, y] = null;
                }
            }

            startNode.G = 0;
            startNode.H = Heuristic(startNode, endNode);
            startNode.F = startNode.H;

            _mOpenSet.Add(startNode);
            _mOrderedOpenSet.Push(startNode);
            _mRuntimeGrid.Add(startNode);
            
            while (!_mOpenSet.IsEmpty)
            {
                var x = _mOrderedOpenSet.Pop();

                if (x == endNode)
                {
                    var result = ReconstructPath(_mCameFrom, _mCameFrom[endNode.X, endNode.Y]);

                    result.AddLast(endNode.UserContext);

                    return result;
                }

                _mOpenSet.Remove(x);
                _mClosedSet.Add(x);

                StoreNeighborNodes(x, neighborNodes);

                foreach (var y in neighborNodes)
                {
                    bool tentativeIsBetter;

                    if (y == null) continue;

                    if (!y.UserContext.IsWalkable(inUserContext) && !(y.X == endNode.X && y.Y == endNode.Y)) continue;

                    if (_mClosedSet.Contains(y)) continue;

                    var tentativeGScore = _mRuntimeGrid[x].G + NeighborDistance(x, y);
                    var wasAdded = false;

                    if (!_mOpenSet.Contains(y))
                    {
                        _mOpenSet.Add(y);
                        tentativeIsBetter = true;
                        wasAdded = true;
                    }
                    else if (tentativeGScore < _mRuntimeGrid[y].G)
                    {
                        tentativeIsBetter = true;
                    }
                    else
                    {
                        tentativeIsBetter = false;
                    }

                    if (!tentativeIsBetter) continue;

                    _mCameFrom[y.X, y.Y] = x;

                    if (!_mRuntimeGrid.Contains(y))
                        _mRuntimeGrid.Add(y);

                    _mRuntimeGrid[y].G = tentativeGScore;
                    _mRuntimeGrid[y].H = Heuristic(y, endNode);
                    _mRuntimeGrid[y].F = _mRuntimeGrid[y].G + _mRuntimeGrid[y].H;

                    if (wasAdded) _mOrderedOpenSet.Push(y);
                    else _mOrderedOpenSet.Update(y);
                }
            }

            return null;
        }

        private static LinkedList<TPathNode> ReconstructPath(PathNode[,] cameFrom, PathNode currentNode)
        {
            var result = new LinkedList<TPathNode>();

            ReconstructPathRecursive(cameFrom, currentNode, result);

            return result;
        }

        private static void ReconstructPathRecursive(PathNode[,] cameFrom, PathNode currentNode, LinkedList<TPathNode> result)
        {
            var item = cameFrom[currentNode.X, currentNode.Y];

            if (item != null)
            {
                ReconstructPathRecursive(cameFrom, item, result);

                result.AddLast(currentNode.UserContext);
            }
            else
                result.AddLast(currentNode.UserContext);
        }

        private void StoreNeighborNodes(PathNode inAround, IList<PathNode> inNeighbors)
        {
            var x = inAround.X;
            var y = inAround.Y;

            inNeighbors[0] = y > 0 ? _mSearchSpace[x, y - 1] : null;
            inNeighbors[1] = x > 0 ? _mSearchSpace[x - 1, y] : null;
            inNeighbors[2] = x < Width - 1 ? _mSearchSpace[x + 1, y] : null;
            inNeighbors[3] = y < Height - 1 ? _mSearchSpace[x, y + 1] : null;
        }

        protected class PathNode : IPathNode<TUserContext>, IComparer<PathNode>, IIndexedObject
        {
            public static readonly PathNode Comparer = new PathNode(0, 0, default(TPathNode));

            public TPathNode UserContext { get; internal set; }

            public double G { get; internal set; }

            public double H { get; internal set; }

            public double F { get; internal set; }

            public int Index { get; set; }

            public bool IsWalkable(TUserContext inContext)
            {
                return UserContext.IsWalkable(inContext);
            }

            public int X { get; internal set; }

            public int Y { get; internal set; }

            public int Compare(PathNode x, PathNode y)
            {
                return x.F < y.F ? -1 : (x.F > y.F ? 1 : 0);
            }

            public PathNode(int inX, int inY, TPathNode inUserContext)
            {
                X = inX;
                Y = inY;
                UserContext = inUserContext;
            }
        }

        private class OpenCloseMap
        {
            private readonly PathNode[,] _mMap;

            private int Width { get; }

            private int Height { get; }

            private int Count { get; set; }

            public PathNode this[PathNode node] => _mMap[node.X, node.Y];

            public bool IsEmpty => Count == 0;

            public OpenCloseMap(int inWidth, int inHeight)
            {
                _mMap = new PathNode[inWidth, inHeight];
                Width = inWidth;
                Height = inHeight;
            }

            public void Add(PathNode inValue)
            {
                Count++;
                _mMap[inValue.X, inValue.Y] = inValue;
            }

            public bool Contains(PathNode inValue)
            {
                var item = _mMap[inValue.X, inValue.Y];

                return item != null;
            }

            public void Remove(PathNode inValue)
            {
                Count--;
                _mMap[inValue.X, inValue.Y] = null;
            }

            public void Clear()
            {
                Count = 0;

                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        _mMap[x, y] = null;
                    }
                }
            }
        }
    }
}
