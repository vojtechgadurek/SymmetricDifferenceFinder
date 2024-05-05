using System;
using System.Collections.Generic;
using System.Linq;
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
	}
}
