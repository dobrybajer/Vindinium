using System.Collections.Generic;

namespace vindinium.PathFinding
{
    public class PriorityQueue<T> where T : IIndexedObject
    {
        protected List<T> InnerList = new List<T>();
        protected IComparer<T> MComparer;

        public PriorityQueue()
        {
            MComparer = Comparer<T>.Default;
        }

        public PriorityQueue(IComparer<T> comparer)
        {
            MComparer = comparer;
        }

        public PriorityQueue(IComparer<T> comparer, int capacity)
        {
            MComparer = comparer;
            InnerList.Capacity = capacity;
        }

        protected void SwitchElements(int i, int j)
        {
            var h = InnerList[i];
            InnerList[i] = InnerList[j];
            InnerList[j] = h;

            InnerList[i].Index = i;
            InnerList[j].Index = j;
        }

        protected virtual int OnCompare(int i, int j)
        {
            return MComparer.Compare(InnerList[i], InnerList[j]);
        }

        public int Push(T item)
        {
            var p = InnerList.Count;
            item.Index = InnerList.Count;
            InnerList.Add(item);

            do
            {
                if (p == 0) break;

                var p2 = (p - 1)/2;
                if (OnCompare(p, p2) < 0)
                {
                    SwitchElements(p, p2);
                    p = p2;
                }
                else break;

            } while (true);

            return p;
        }

        public T Pop()
        {
            var result = InnerList[0];
            var p = 0;

            InnerList[0] = InnerList[InnerList.Count - 1];
            InnerList[0].Index = 0;

            InnerList.RemoveAt(InnerList.Count - 1);

            result.Index = -1;

            do
            {
                var pn = p;
                var p1 = 2*p + 1;
                var p2 = 2*p + 2;

                if (InnerList.Count > p1 && OnCompare(p, p1) > 0) p = p1;
                if (InnerList.Count > p2 && OnCompare(p, p2) > 0) p = p2;

                if (p == pn) break;
                SwitchElements(p, pn);

            } while (true);

            return result;
        }

        public void Update(T item)
        {
            var count = InnerList.Count;

            while ((item.Index - 1 >= 0) && (OnCompare(item.Index - 1, item.Index) > 0))
                SwitchElements(item.Index - 1, item.Index);
            while ((item.Index + 1 < count) && (OnCompare(item.Index + 1, item.Index) < 0))
                SwitchElements(item.Index + 1, item.Index);   
        }

        public T Peek()
        {
            return InnerList.Count > 0 ? InnerList[0] : default(T);
        }

        public void Clear()
        {
            InnerList.Clear();
        }

        public int Count => InnerList.Count;
    }
}