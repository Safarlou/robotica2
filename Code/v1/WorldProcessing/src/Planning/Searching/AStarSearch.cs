using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldProcessing.src.Planning.Searching
{
    /// <summary>
    /// Implements one search function to find the path from one node to another.
    /// Functions work on nodes that implement IHasNeighbours interface.
    /// </summary>
    public class AStarSearch
    {
        /// <summary>
        /// Finds path between two nodes according to A* algorithm.
        /// </summary>
        /// <typeparam name="Node">Generic node type</typeparam>
        /// <param name="start">Start node</param>
        /// <param name="destination">Destination node</param>
        /// <param name="distance">Function that determines the distance to the next node</param>
        /// <param name="estimate">Estimated distance to destination</param>
        /// <returns></returns>
        static public Path<Node> FindPath<Node>(
            Node start, 
            Node destination, 
            Func<Node, Node, Double> distance, 
            Func<Node, double> estimate) where Node : IHasNeighbours<Node>
        {
            var closed = new HashSet<Node>();
            var queue = new PriorityQueue<double, Path<Node>>();
            queue.Enqueue(0, new Path<Node>(start));
            while (!queue.IsEmpty)
            {
                var path = queue.Dequeue();
                if (closed.Contains(path.LastStep)) //prevent cycling
                    continue;
                if (path.LastStep.Equals(destination)) //reached destination
                    return path;
                closed.Add(path.LastStep); //node does not have to be considered again
                //add neighbours to priority queue
                foreach (Node n in path.LastStep.Neighbours)
                {
                    double d = distance(path.LastStep, n);
                    var newPath = path.AddStep(n, d);
                    queue.Enqueue(newPath.TotalCost + estimate(n), newPath);
                }
            }
            return null;
        }
    }
}
