using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.Oracles
{
	public struct Cache<TOracle> : IOracle
		where TOracle : struct, IOracle
	{
		Dictionary<ulong, ulong[]> _dic = new();
		static TOracle _oracle = new();
		//static ThreadLocal<LimitedSizeDic<ulong[]>> _dic = new(() => new());


		public Cache()
		{
		}

		public ulong[] GetClose(ulong id)
		{
			if (!_dic.TryGetValue(id, out ulong[]? answer))
			{
				answer = _oracle.GetClose(id);
				_dic.Add(id, answer);
			};

			//var answer = _dic.Value.GetValue(id);
			//if (answer is not null) return answer;
			//answer = _oracle.GetClose(id);
			//_dic.Value.SetValue(id, answer);
			return answer;
		}


		public int Size()
		{
			return _oracle.Size();
		}
	}

	public class LimitedSizeDic<T>
	{

		const ulong _size = 1024 * 16;
		(ulong, T)[] _cachedValues = new (ulong, T)[_size];

		public T? GetValue(ulong key)
		{
			var (keyCashed, cached) = _cachedValues[key % _size];

			if (keyCashed == key) return cached;
			else return default(T);
		}

		public void SetValue(ulong key, T value)
		{
			_cachedValues[key % _size] = (key, value);
		}



	}
}
