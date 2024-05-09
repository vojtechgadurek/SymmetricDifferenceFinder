using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using SymmetricDifferenceFinderTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinderTests.EncoderTests
{
	public class BasicEncoderTests
	{
		[Fact]
		public void SimpleEncodingTest()
		{

			int size = 1024;
			int bufferSize = 4;

			ulong[] table = new ulong[size];

			var hf = Enumerable.Range(0, 1).Select(_ => (Action<ulong[], ulong[], int, int>)(
				(ulong[] keys, ulong[] hashes, int start, int length) =>
				{
					for (int i = 0; i < length; i++)
					{
						hashes[i] = keys[i];
					}
				})
			);

			Encoder<OverwriteTable> encoder = new Encoder<OverwriteTable>(
				new OverwriteTable(table),
				hf,
				bufferSize);

			encoder.Encode(new ulong[] { 1, 2, 3, 4 }, 4);

			for (int i = 1; i < 5; i++)
			{
				Assert.Equal((ulong)i, table[i]);
			}

			for (int i = 5; i < size; i++)
			{
				Assert.Equal(0UL, table[i]);
			}

		}

		[Fact]
		public void SimpleEncodingTestWithModuloHashingFunction()
		{

			int size = 1024;

			var config = new EncoderConfiguration<OverwriteTable>(
				new IHashingFunctionScheme[] { new ModuloScheme((ulong)size, 0) },
				size
			);

			var factory = new EncoderFactory<OverwriteTable>(config, size => new OverwriteTable(new ulong[size]));

			var encoder = factory.Create();


			encoder.Encode(new ulong[] { 1, 2, 3, 4 }, 4);

			var table = encoder.GetTable()._table;
			for (int i = 1; i < 5; i++)
			{
				Assert.Equal((ulong)i, table[i]);
			}

			for (int i = 5; i < size; i++)
			{
				Assert.Equal(0UL, table[i]);
			}



		}


	}
}
