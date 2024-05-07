using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace SymmetricDifferenceFinderTests.Decoder
{
	public class HyperGraphSpecialFunctionsTest
	{
		public ITestOutputHelper _output;

		public HyperGraphSpecialFunctionsTest(ITestOutputHelper output)
		{
			_output = output;

			Scope._debugOutput = _output;
		}

		[Fact]
		public void TestLooksPureHyperGraphDecode()
		{

			ulong size = 10;
			Expression<Func<ulong, ulong>> h1 = x => x % size;
			Expression<Func<ulong, ulong>> h2 = x => 0;

			var IsPure = HyperGraphDecoderMainLoop.GetIsPure<IBLTTable>(new[] { h1, h2 }).Compile();

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

		[Fact]

		public void TestRemoveAndAddToPureHyperGraphDecode()
		{
			void FieldContains(int count, ulong keySum, IBLTTable t, params ulong[] keys)
			{
				foreach (var key in keys)
				{
					Assert.Equal(count, t.GetCount(key));
					Assert.Equal(keySum, t.Get(key));
				}


			}

			ulong size = 10;
			Expression<Func<ulong, ulong>> h1 = x => x % size;
			Expression<Func<ulong, ulong>> h2 = x => 0;

			var f = HyperGraphDecoderMainLoop.GetRemoveAndAddToPure<IBLTTable>(new[] { h1, h2 }).Compile();

			IBLTTable t = new IBLTTable((int)size);

			List<ulong> p = new List<ulong>();
			List<ulong> a = new List<ulong>();

			t.Add(0, 1);
			t.Add(1, 1);
			FieldContains(1, 1, t, 0, 1);

			f(1, t, p, a);

			Assert.Contains(1UL, a);

			Assert.Single(a);


			FieldContains(0, 0, t, 0, 1);

			t.Remove(0, 3);
			t.Remove(3, 3);

			FieldContains(-1, 3, t, 0, 3);

			f(3, t, p, a);

			Assert.Contains(3UL, a);

			Assert.Equal(2, a.Count);

			FieldContains(0, 0, t, 0, 3);


		}
	}
}
