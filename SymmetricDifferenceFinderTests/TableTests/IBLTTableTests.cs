using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinderTests.TableTests
{
	public class IBLTTableTests
	{
		[Fact]
		public void TestLooksPureHyperGraphDecode()
		{

			ulong size = 10;
			Expression<Func<ulong, ulong>> h1 = x => x % size;
			Expression<Func<ulong, ulong>> h2 = x => 0;

			var IsPure = IBLTTable.GetLooksPure(new[] { h1, h2 }).Compile();

			IBLTTable t = new IBLTTable((int)size);

			t.Add(0, 1);
			t.Add(1, 1);

			Assert.True(IsPure(1, t));

			t.Add(0, 1);
			t.Add(1, 1);

			Assert.False(IsPure(1, t));

			t.Add(0, 2);
			t.Add(2, 2);

			Assert.True(IsPure(2, t));

			t.Remove(0, 3);
			t.Remove(3, 3);

			Assert.True(IsPure(3, t));

			t.Remove(0, 3);
			t.Remove(3, 3);

			Assert.False(IsPure(3, t));
		}
	}
}
