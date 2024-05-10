using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Combinations
{
	public class HashingFunctionCombination
	{
		public record class HashingFunctionConfiguration(
			Func<Expression<HashingFunction>> factory,
			IEnumerable<Func<Expression<HashingFunction>, Expression<HashingFunction>>> Modificators
			);

		List<HashingFunctionConfiguration> _hashingFunctions = new List<HashingFunctionConfiguration>();

		public HashingFunctionCombination AddHashingFunction(Func<Expression<HashingFunction>> factory, params Func<Expression<HashingFunction>, Expression<HashingFunction>>[] modificators)
		{
			_hashingFunctions.Add(new HashingFunctionConfiguration(factory, modificators));
			return this;
		}

		public IEnumerable<Expression<HashingFunction>> Generate()
		{
			var result = new List<Expression<HashingFunction>>();
			foreach (var hf in _hashingFunctions)
			{
				var hf_ = hf.factory();
				foreach (var mod in hf.Modificators)
				{
					hf_ = mod(hf_);
				}
				result.Add(hf_);
			}
			return result;
		}


	}
}
