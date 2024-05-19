using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.RetrievalTesting.DataSources
{
	public class RandomDataFactory
	{
		public static HashSet<ulong> GetData(int nItems)
		{
			var data = new HashSet<ulong>();
			var random = new Random();
			for (int i = 0; i < nItems; i++)
			{
				data.Add(GenerateNotNullRandomUInt64(0, random));
			}
			return data;
		}

		public static ulong GenerateNotNullRandomUInt64(ulong nullValue, Random random)
		{
			ulong randomLong = nullValue;
			while (randomLong == nullValue)
			{
				randomLong = (ulong)random.NextInt64();
			}
			return randomLong;
		}

	}
	void Test()
	{
	}
}


