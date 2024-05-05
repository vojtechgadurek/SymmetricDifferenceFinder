using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;
using SymmetricDifferenceFinder.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinderBenchmarks
{
	public class EncoderBenchmarks
	{
		public class BasicEncodingBenchmark
		{

			[ParamsSource(nameof(HashingFunctionsToTest))]
			public Type hashingFunctionFamily;

			public static IEnumerable<Type> HashingFunctionsToTest() => HashingFunctionProvider.GetAllHashingFunctionFamilies();

			public const int Length = 1024;
			public const int BufferLength = 4096;
			public const int DataLength = Length * BufferLength;

			[ParamsSource(nameof(TableLengths))]
			public ulong TableSize;


			public static IEnumerable<ulong> TableLengths()
				=> new ulong[] { 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144, 524288, 1048576 };

			readonly public Random random = new Random();

			ulong[][] stream => GetDataStream(Length, BufferLength);

			ulong[][] GetDataStream(int size, int bufferLength)
			{

				var data = new ulong[size][];
				for (int i = 0; i < size; i++)
				{
					data[i] = new ulong[bufferLength];
					random.NextBytes(MemoryMarshal.Cast<ulong, byte>(data[i].AsSpan()));
				}
				return data;
			}


			[Benchmark]
			public ulong[] BenchmarkDecoder()
			{
				var config = new EncoderConfiguration<XORTable>(
						new[] { HashingFunctionProvider.Get(hashingFunctionFamily, TableSize) },
						(int)TableSize
					);

				var factory = new EncoderFactory<XORTable>(config, size => new XORTable(new ulong[size]));

				var encoder = factory.Create();

				foreach (var buffer in stream)
				{
					encoder.Encode(buffer, buffer.Length);
				}


				return encoder.GetTable().GetUnderlyingTable();
			}

		}

	}
}
