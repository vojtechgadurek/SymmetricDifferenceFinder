using SymmetricDifferenceFinder.Improvements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SymmetricDifferenceFinder.Improvements.Oracles;

namespace SymmetricDifferenceFinderTests.Improvements
{

	public class TestCanonicalOrders
	{
		[Theory]
		[InlineData(1)]
		[InlineData(100)]
		[InlineData(33333)]
		[InlineData(1 << 61)]


		void TestCanonicalOrder(ulong i)
		{
			CanonicalOrder a = default;
			var res = a.Other(a.Other(i));
			Assert.Equal(i, res);
		}
	}
}
