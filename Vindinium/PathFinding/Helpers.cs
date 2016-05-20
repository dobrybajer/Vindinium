namespace vindinium.PathFinding
{
    internal class MyPathNode : IPathNode<object>
    {
        public int X { get; set; }

        public int Y { get; set; }

        public bool IsWall { get; set; }

        public bool IsWalkable(object unused)
        {
            return !IsWall;
        }
    }

    public interface IPathNode<in TUserContext>
    {
        bool IsWalkable(TUserContext inContext);
    }

    public interface IIndexedObject
    {
        int Index { get; set; }
    }
}
