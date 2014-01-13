using System;
using System.Collections.Generic;

namespace WorldProcessing.Planning.Searching
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
		/// <param name="neighbours">Function to determine the neighbours of a node</param>
		/// <param name="distance">Function to determine the distance between two nodes</param>
		/// <param name="estimate">Function to estimate the distance between a node and the goal</param>
		/// <returns></returns>
		static public Path<Node> FindPath<Node>(
			Node start, 
			Node destination, 
			Func<Node,IEnumerable<Node>> neighbours, 
			Func<Node, Node, Double> distance, 
			Func<Node, double> estimate)
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
				foreach (Node n in neighbours(path.LastStep))
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
