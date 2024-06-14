using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.Oracles
{
	public struct Cache<TOracle> : IOracle
		where TOracle : struct, IOracle
	{
		static ThreadLocal<Dictionary<ulong, ulong[]>> _dic = new(() => new());
		static TOracle _oracle = default;

		public Cache()
		{
		}

		public ulong[] GetClose(ulong id)
		{
			if (!_dic.Value.TryGetValue(id, out ulong[]? answer))
			{
				answer = _oracle.GetClose(id);
				_dic.Value.Add(id, answer);
			};
			return answer;
		}
		public int Size()
		{
			return _oracle.Size();
		}
	}
}
