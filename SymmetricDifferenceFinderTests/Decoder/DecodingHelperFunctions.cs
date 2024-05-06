using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinderTests.Decoder
{
	public class DecodingHelperFunctionsTest
	{

		struct MockTable : ISketch
		{
			public ulong[] _table;

			public MockTable(ulong[] table)
			{
				_table = table;
			}

			public ulong Get(ulong key)
			{
				return _table[key];
			}

			public ISketch SymmetricDifference(ISketch other)
			{
				throw new NotImplementedException();
			}
		}

		[Fact]
		public void GetLooksPureTest()
		{
			Expression<Func<ulong, ulong>> f = x => x;
			var looksPure = DecodingHelperFunctions.GetLooksPure<MockTable>(new List<Expression<Func<ulong, ulong>>> { f }).Compile();

			var table = new ulong[10];

			table[0] = 0;

			table[1] = 1;

			table[6] = 7;
			var mockTable = new MockTable(table);

			Assert.True(looksPure(1, mockTable));
			Assert.False(looksPure(6, mockTable));

			Assert.False(looksPure(0, mockTable));
		}

		[Fact]

		public void AddIfLooksPureTest()
		{
			Expression<Func<ulong, ulong>> f = x => x;
			var looksPure = DecodingHelperFunctions.GetLooksPure<MockTable>(new List<Expression<Func<ulong, ulong>>> { f });
			var addIfLooksPure = DecodingHelperFunctions.GetAddIfLooksPure<HashSet<ulong>, MockTable>(looksPure).Compile();

			var table = new ulong[10];

			table[0] = 0;

			table[1] = 1;

			table[6] = 7;
			var mockTable = new MockTable(table);

			var set = new HashSet<ulong>();

			addIfLooksPure(1, set, mockTable);

			Assert.True(set.Contains(1));

			addIfLooksPure(6, set, mockTable);

			Assert.False(set.Contains(6));

			addIfLooksPure(0, set, mockTable);

			Assert.False(set.Contains(0));
		}
	}
}
