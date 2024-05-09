using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinderTests.TableTests
{
	public class XORTableTests
	{
		[Fact]
		public void BasicTest()
		{
			var array = new ulong[10];
			var table = new XORTable(array);

			table.Add(0, 0);

			Assert.Equal(0UL, array[0]);

			table.Add(0, 1);

			Assert.Equal(1UL, array[0]);

			table.Add(0, 2UL);

			Assert.Equal(3UL, array[0]);


			table.Add(0, 1);

			Assert.Equal(2UL, array[0]);

			table.Add(0, 2UL);

			Assert.Equal(0UL, array[0]);

		}

		[Fact]
		public void GetLooksPureTest()
		{
			Expression<Func<ulong, ulong>> f = x => x;
			var looksPure = XORTable.GetLooksPure(new[] { f }).Compile();

			var table = new ulong[10];

			table[0] = 0;

			table[1] = 1;

			table[6] = 7;
			var mockTable = new XORTable(table);

			Assert.True(looksPure(1, mockTable));
			Assert.False(looksPure(6, mockTable));

			Assert.False(looksPure(0, mockTable));
		}
	}
}
