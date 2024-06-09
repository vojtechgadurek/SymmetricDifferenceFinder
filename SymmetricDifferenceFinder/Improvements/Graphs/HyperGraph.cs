using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.Graphs
{
	public class HyperGraph
	{
		HashSet<ulong>[] _attacks;

		public HyperGraph(ulong size)
		{
			_attacks = new HashSet<ulong>[size];
			for (int i = 0; i < _attacks.Length; i++)
			{
				_attacks[i] = new HashSet<ulong>();
			}
		}

		public void AddEdge(ulong id, ulong[] edges)
		{
			foreach (var edge in edges)
			{
				if (!_attacks[edge].Contains(id))
					_attacks[edge].Add(id);
			}
		}

		public void RemoveEdge(ulong ID, ulong[] edges)
		{
			foreach (var edge in edges)
			{
				if (_attacks[edge].Contains(ID))
					_attacks[edge].Remove(ID);
			}
		}

		public HashSet<ulong> GetBucket(ulong id)
		{
			return _attacks[id];
		}
	}
}
