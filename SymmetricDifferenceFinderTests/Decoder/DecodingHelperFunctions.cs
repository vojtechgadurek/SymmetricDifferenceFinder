using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SymmetricDifferenceFinder.Decoders.Common;

namespace SymmetricDifferenceFinderTests.Decoder
{
	public class DecodingHelperFunctionsTest
	{

		struct MockTable : ISketch<MockTable>
		{
			public ulong[] _table;

			public MockTable(ulong[] table)
			{
				_table = table;
			}

			public int Size() => throw new NotImplementedException();

			public ulong Get(ulong key)
			{
				return _table[key];
			}

			public bool IsEmpty()
			{
				throw new NotImplementedException();
			}

			public MockTable SymmetricDifference(MockTable other)
			{
				throw new NotImplementedException();
			}
		}

		[Fact]

		public void AddIfLooksPureTest()
		{
			Expression<Func<ulong, MockTable, bool>> True = (ulong x, MockTable t) => true;
			Expression<Func<ulong, MockTable, bool>> False = (ulong x, MockTable t) => false;

			var addIfLooksPureTrue = DecodingHelperFunctions.GetAddIfLooksPure<HashSet<ulong>, MockTable>(True).Compile();
			var addIfLooksPureFalse = DecodingHelperFunctions.GetAddIfLooksPure<HashSet<ulong>, MockTable>(False).Compile();

			var set = new HashSet<ulong>();

			addIfLooksPureTrue(1UL, set, new MockTable());

			Assert.Single(set);
			Assert.Contains(1UL, set);

			addIfLooksPureFalse(2UL, set, new MockTable());

			Assert.Single(set);
			Assert.Contains(1UL, set);


		}
	}
}
