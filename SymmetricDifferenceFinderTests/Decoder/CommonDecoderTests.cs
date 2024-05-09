using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace SymmetricDifferenceFinderTests.Decoder
{
	public class CommonDecoderTests
	{

		void Encode(ulong key, IEnumerable<Func<ulong, ulong>> hfs, ITable table)
		{
			foreach (var hf in hfs)
			{
				table.Add(hf(key), key);
			}
		}


		[Theory]
		[InlineData(3)]
		[InlineData(5)]
		[InlineData(7)]
		[InlineData(8)]
		[InlineData(10)]
		[InlineData(13)]

		public void RunSimpleDecodingTest(int size)
		{
			var data = HashingFunctionsCombinations.GetValues((ulong)size);
			var factories = DecoderSketchCombinations.GetValues();
			foreach (var dat in data)
			{
				foreach (var fact in factories)
				{
					SimpleDecodingTest(fact.tableFactory, fact.decoderFactory, (int)dat.Item1, dat.Item2);
				}
			}
		}


		void SimpleDecodingTest(
				Func<int, ITable> tableFactory,
				Func<ITable, IEnumerable<Expression<Func<ulong, ulong>>>, IDecoder> decoderFactory,
				int size, IEnumerable<Expression<Func<ulong, ulong>>> hfs)
		{
			// Test decoding over one 

			var t = tableFactory(size);

			var valuesToEncode = Enumerable.Range(1, size - 2).Select(x => (ulong)x).ToList();

			var hfsCompiled = hfs.Select(hf => hf.Compile()).ToList();
			foreach (var value in valuesToEncode)
			{
				Encode(value, hfsCompiled, t);
			}

			var decoder = decoderFactory(t, hfs);
			decoder.Decode();


			foreach (var i in valuesToEncode)
			{
				Assert.Contains(i, decoder.GetDecodedValues());
			}
		}

	}
}
