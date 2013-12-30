using System.Collections.Generic;

namespace WorldProcessing.Planning.Searching
{
    /// <summary>
    /// Node interface that should be implemented before the node can be used in search algorithms.
    /// </summary>
    /// <typeparam name="N"></typeparam>
    public interface IHasNeighbours<N>
    {
        IEnumerable<N> Neighbours { get; }
    }
}
