using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.src.Planning.Searching
{
    /// <summary>
    /// Implements a PriorityQueue as a SortedDictionary of standard Queues.
    /// </summary>
    /// <typeparam name="P"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class PriorityQueue <P, V> 
    {
        private SortedDictionary<P, Queue<V>> data = new SortedDictionary<P, Queue<V>>();

        public void Enqueue(P priority, V value)
        {
            Queue<V> q;
            if (!data.TryGetValue(priority, out q))
            {
                q = new Queue<V>();
                data.Add(priority, q);
            }
            q.Enqueue(value);
        }

        public V Dequeue()
        {
            //will throw an exception if there is no first element!
            var pair = data.First();
            var v = pair.Value.Dequeue();
            if (pair.Value.Count == 0) //nothing left of the top priority
                data.Remove(pair.Key);
            return v;
        }

        public bool IsEmpty
        {
            get { return !data.Any(); }
        }
    }
}
