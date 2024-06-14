using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.Oracles
{
	public struct PickOneRandomly<TOracle>
		where TOracle : struct, IOracle
	{
		static Random _random = new Random();
		TOracle oracle = default;

		public PickOneRandomly()
		{
		}

		public ulong GetRandom(ulong id)
		{
			var nodes = oracle.GetClose(id);
			if (oracle.Size() == 1) return nodes[0];
			return nodes[_random.Next(nodes.Length)];
		}
	}
}
