using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.StringFactories
{
	public static class StringDataFactory<TStringFactory>
			where TStringFactory : struct, IStringFactory
	{
		public static HashSet<ulong> GetRandomStringData(int nItems, int stringLength)
		{
			Random random = new Random();
			TStringFactory stringFactory = default(TStringFactory);
			HashSet<ulong> data = new HashSet<ulong>();
			for (int i = 0; i < nItems / stringLength; i++)
			{
				ulong value = (ulong)random.NextInt64();
				for (int j = 0; j < stringLength; j++)
				{
					value = stringFactory.GetPossibleNext(value)[0];
					data.Add(value);
				}
			}
			return data;
		}
	}
}
