using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SymmetricDifferenceFinder.Improvements.Graphs;
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


			var numberOfStrings = nItems / stringLength;

			void CreateString(int length)
			{
				ulong value = s.GetRandom();
				//zero is forbidden value
				if (value == 0) value++;
				for (int j = 0; j < length; j++)
				{
					value = stringFactory.GetRandom(value);
					data.Add(value);
				}
			}

			for (int i = 0; i < numberOfStrings; i++)
			{

				CreateString(stringLength);
			}

			if (nItems % stringLength != 0)
			{
				CreateString(nItems % stringLength);
			}


			return data;
		}
	}
}
