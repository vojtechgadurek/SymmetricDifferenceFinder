using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.Graphs
{
	public class HyperGraph
	{
		HashSet<ulong>[] _edges;

		public HyperGraph(ulong size)
		{
			_edges = new HashSet<ulong>[size];
			for (int i = 0; i < _edges.Length; i++)
			{
				_edges[i] = new HashSet<ulong>();
			}
		}

		public void AddEdge(ulong id, ulong[] edge)
		{
			foreach (var vertex in edge)
			{
				if (!_edges[vertex].Contains(id))
					_edges[vertex].Add(id);
			}
		}

		public void RemoveEdge(ulong id, ulong[] edge)
		{
			foreach (var vertex in edge)
			{
				if (_edges[vertex].Contains(id))
					_edges[vertex].Remove(id);
			}
		}

		public HashSet<ulong> GetBucket(ulong id)
		{
			return _edges[id];
		}
	}
}
