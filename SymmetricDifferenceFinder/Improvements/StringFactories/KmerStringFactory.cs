using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.StringFactories
{
	struct KMerStringFactory : IStringFactory
	{
		static int size = 31;

		public int NPossibleNext => 4;

		public int NPossibleBefore => 4;

		public ulong[] GetPossibleBefore(ulong value)
		{
			ulong sizeMask = (1UL << (size * 2)) - 1UL;

			var answer = new ulong[4];
			for (ulong i = 0; i < 4; i++)
			{
				answer[i] = (sizeMask & (value << 2)) | i;
			}
			return answer;
		}

		public ulong[] GetPossibleNext(ulong value)
		{
			ulong sizeMask = (1UL << (size * 2)) - 1UL;
			var answer = new ulong[4];
			for (ulong i = 0; i < 4; i++)
			{
				answer[i] = (sizeMask & (value >> 2)) | (i << (size * 2 - 2));
			}
			return answer;
		}
	}
}
