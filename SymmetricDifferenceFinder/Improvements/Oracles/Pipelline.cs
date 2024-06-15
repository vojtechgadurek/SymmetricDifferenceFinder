using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.Oracles
{
	public interface IPipeline
	{
		ulong Modify(ulong value);
	}


	public struct None : IPipeline
	{
		public ulong Modify(ulong value)
		{
			return value;
		}
	}


	public struct Pipeline<TSource, TModifier> : IOracle
		where TSource : struct, IOracle
		where TModifier : struct, IPipeline
	{
		static TSource _source = default;
		static TModifier _mod = default;

		public ulong[] GetClose(ulong id)
		{
			var answer = _source.GetClose(id);
			for (int i = 0; i < answer.Length; i++)
			{
				answer[i] = _mod.Modify(answer[i]);
			}
			return answer;
		}

		public int Size()
		{
			return _source.Size();
		}
	}
}
