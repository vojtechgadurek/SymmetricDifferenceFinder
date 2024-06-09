using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.StringFactories
{
	public struct NumberStringFactory : IStringFactory
	{
		public int NPossibleNext => 1;
		public int NPossibleBefore => 1;

		public ulong[] GetPossibleBefore(ulong value)
		{
			var answer = value - 1;
			if (answer == 0)
			{
				answer = ulong.MaxValue;
			}
			return [answer];
		}

		public ulong[] GetPossibleNext(ulong value)
		{
			var answer = value + 1;
			if (answer == 0)
			{
				answer = 1;
			}
			return [answer];
		}
	}
}
