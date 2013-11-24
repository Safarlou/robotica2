using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Planning.Searching
{
    /// <summary>
    /// Node interface that should be implemented before the node can be used in search algorithms.
    /// </summary>
    /// <typeparam name="N"></typeparam>
    interface IHasNeighbours<N>
    {
        IEnumerable<N> Neighbours { get; }
    }
}
