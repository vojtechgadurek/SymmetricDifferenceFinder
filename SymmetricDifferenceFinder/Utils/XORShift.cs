using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Utils
{
	//source https://en.wikipedia.org/wiki/Xorshift
	public class XORShift
	{
		ulong _a;
		int _count = 0;
		Random _random;

		public XORShift(Random random)
		{
			_a = (ulong)random.NextInt64();
			_random = random;
		}

		public ulong Next()
		{

			ulong x = _a;
			x ^= x << 13;
			x ^= x >> 17;
			x ^= x << 5;
			if (_count++ % 128 == 0)
			{
				x ^= (ulong)_random.NextInt64();
			}
			return _a = x;
		}
	}
}
