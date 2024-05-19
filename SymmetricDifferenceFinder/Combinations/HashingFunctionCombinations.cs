using Microsoft.Diagnostics.Tracing.Parsers.ApplicationServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SymmetricDifferenceFinder.Combinations;

namespace SymmetricDifferenceFinder.Combinations
{
	public class HashingFunctionCombinations
	{
		public static HashingFunctionCombination GetFromSameFamily(int number, IHashingFunctionFamily family)
		{
			var answer = new HashingFunctionCombination();
			for (int i = 0; i < number; i++)
			{
				answer.AddHashingFunction((size, offset) => family.GetScheme(size, offset).Create());
			}
			return answer;
		}

		public static HashingFunctionCombination GetFromSameFamilyLastWeaker(int number, IHashingFunctionFamily family)
		{
			var answer = new HashingFunctionCombination();
			for (int i = 0; i < number - 1; i++)
			{
				answer.AddHashingFunction((size, offset) => family.GetScheme(size, offset).Create());
			}

			answer.AddHashingFunction((size, offset) => family.GetScheme(size, offset).Create(),
				(h) => Utils.HashingFunctionFilter.Filter(h, (9, 1), 0));

			return answer;
		}
	}
}
