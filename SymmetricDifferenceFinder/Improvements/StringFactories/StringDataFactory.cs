using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SymmetricDifferenceFinder.Improvements.Oracles;

namespace SymmetricDifferenceFinder.Improvements.StringFactories
{
	public static class StringDataFactory<TStringFactory>
			where TStringFactory : struct, IStringFactory
	{
		public static HashSet<ulong> GetRandomStringData(int nItems, int stringLength)
		{
			Random random = new Random();
			PickOneRandomly<Cache<NextOracle<TStringFactory>>> stringFactory = default;
			TStringFactory s = default;
			HashSet<ulong> data = new HashSet<ulong>();
			int a = 0;
			for (int i = 0; i < nItems / stringLength; i++)
			{
				ulong value = s.GetRandom();
				//zero is forbidden value
				if (value == 0) value++;
				for (int j = 0; j < stringLength; j++)
				{
					value = stringFactory.GetRandom(value);
					data.Add(value);
					a++;
				}

			}

			return data;
		}
	}
}
