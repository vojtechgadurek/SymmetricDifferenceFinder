using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace SymmetricDifferenceFinderTests.Decoder
{
	public class CommonDecoderTests
	{
		void SimpleDecodingTest(Func<ulong[], IEnumerable<Expression<Func<ulong, ulong>>>, IDecoder> decoderConstructor)
		{
			// Test decoding over one 
			const int size = 10;
			ulong[] table = new ulong[size];

			for (ulong i = 0; i < size; i++)
			{
				table[i] = i;
			}

			var decoder = decoderConstructor(table, new Expression<Func<ulong, ulong>>[] { (ulong x) => x });

			decoder.Decode();

			Assert.True(table.All(x => x == 0));
			for (ulong i = 1; i < size; i++)
			{
				Assert.Contains(i, decoder.GetDecodedValues());
			}
		}

		void OverlayDecodingTest(Func<ulong[], IEnumerable<Expression<Func<ulong, ulong>>>, IDecoder> decoderConstructor)
		{
			const int size = 11;
			ulong[] table = new ulong[size];

			for (ulong i = 1; i < size - 1; i++)
			{
				table[i] ^= i;
				table[i + 1] ^= i;
			}

			var decoder = decoderConstructor(table, new Expression<Func<ulong, ulong>>[] { (ulong x) => x % size, (ulong x) => (x + 1) % size });

			decoder.Decode();

			Assert.True(table.All(x => x == 0));
			for (ulong i = 1; i < size - 1; i++)
			{
				Assert.Contains(i, decoder.GetDecodedValues());
			}
		}


		void FakeNonRegularFunctions(Func<ulong[], IEnumerable<Expression<Func<ulong, ulong>>>, IDecoder> decoderConstructor, int size)
		{
			ulong[] table = new ulong[size];

			for (ulong i = 1; i < (ulong)size - 1; i++)
			{
				table[0] ^= i;
				table[i] = i;
			}

			var decoder = decoderConstructor(table, new Expression<Func<ulong, ulong>>[] { (ulong x) => x % (ulong)size, (ulong x) => 0 });

			decoder.Decode();

			Assert.True(table.All(x => x == 0));
			for (ulong i = 1; i < (ulong)size - 1; i++)
			{
				Assert.Contains(i, decoder.GetDecodedValues());
			}
		}

		[Fact]
		public void SimpleHPWDecoderTest()
		{
			Func<ulong[], IEnumerable<Expression<Func<ulong, ulong>>>, IDecoder> decoderConstructor =
				(data, hf) => new HPWDecoderFactory<XORTable>(hf).Create(new XORTable(data));

			SimpleDecodingTest(decoderConstructor);
		}

		[Fact]
		public void OverlayHPWDecoderTest()
		{
			Func<ulong[], IEnumerable<Expression<Func<ulong, ulong>>>, IDecoder> decoderConstructor =
				(data, hf) => new HPWDecoderFactory<XORTable>(hf).Create(new XORTable(data));

			OverlayDecodingTest(decoderConstructor);
		}

		[Theory]

		[InlineData(2)]
		[InlineData(4)]
		[InlineData(5)]
		[InlineData(9)]
		[InlineData(10)]
		[InlineData(20)]

		public void FakeNonRegularHPWDecoderTest(int size)
		{
			Func<ulong[], IEnumerable<Expression<Func<ulong, ulong>>>, IDecoder> decoderConstructor =
				(data, hf) => new HPWDecoderFactory<XORTable>(hf).Create(new XORTable(data));
			FakeNonRegularFunctions(decoderConstructor, size);
		}
	}
}
