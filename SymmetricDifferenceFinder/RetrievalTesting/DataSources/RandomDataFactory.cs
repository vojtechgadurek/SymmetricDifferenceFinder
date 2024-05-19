﻿using Microsoft.Diagnostics.Tracing.Parsers.AspNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.RetrievalTesting.DataSources
{
	public static class RandomDataFactory
	{
		public static HashSet<ulong> GetRandomData
			(int nItems)
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

		public static ulong NextInString(ulong i)
		{
			var answer = i + 1;
			if (answer == 0)
			{
				answer = 1;
			}
			return answer;
		}

		public static ulong BeforeInString(ulong i)
		{
			var answer = i - 1;
			if (answer == 0)
			{
				answer = ulong.MaxValue;
			}
			return answer;
		}

		public static HashSet<ulong> GetRandomStringData(int nItems, int stringLength)
		{
			Random random = new Random();
			HashSet<ulong> data = new HashSet<ulong>();
			for (int i = 0; i < nItems / stringLength; i++)
			{
				var x = GenerateNotNullRandomUInt64(0, random);
				data.Add(x);
				for (int j = 0; j < stringLength; j++)
				{
					x = NextInString(x);
					data.Add(x);

				}
			}
			return data;
		}

	}

}


