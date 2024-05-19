using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;
using SymmetricDifferenceFinder.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RedaFasta;

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
						new[] { HashingFunctionProvider.Get(hashingFunctionFamily, TableSize, 0) },
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

		public class FastaFileReaderBenchmark
		{

			public const int Length = 1024;
			public const int BufferLength = 4096;
			public const int DataLength = Length * BufferLength;
			public const int kMerSize = 31;
			public const int charsInFile = 100_000_000;

			[ParamsSource(nameof(TableLengths))]
			public ulong TableSize;


			public static IEnumerable<ulong> TableLengths()
				=> new ulong[] { 10_000_000 };

			readonly public Random random = new Random();

			[GlobalSetup]
			public void SetUp()
			{
				StreamWriter writer = new StreamWriter("test.test");
				writer.WriteLine($">test l={charsInFile} k={kMerSize}");
				writer.WriteLine(RandomString(100000000));
				string RandomString(int length)
				{
					const string chars = "ACGT";

					StringBuilder stringBuilder = new StringBuilder(length);
					for (int i = 0; i < length; i++)
					{
						stringBuilder.Append(chars[new Random().Next(chars.Length)]);
					}
					return stringBuilder.ToString();
				}
				writer.Close();

			}

			[Benchmark]
			public ulong[] ParalelEncode()
			{
				var encoder =
					new NoConflictEncoder<XORTable>(
						new XORTable((int)TableSize),
						HashingFunctionCombinations
						.GetFromSameFamily(3, new MultiplyShiftFamily())
						.GetNoConflictFactory()((int)TableSize)
						.Select(h => LittleSharp.Utils.Buffering.BufferFunction(h).Compile()),
						1024);

				string fastaFilePath = "test.test";

				var config = FastaFile.Open(new StreamReader(fastaFilePath));
				var reader = new FastaFileReader(config.kMerSize, config.nCharsInFile, config.textReader);

				var buffer = new ulong[1024 * 1024];
				while (true)
				{
					var data = reader.BorrowBuffer();
					if (data is null)
					{
						break;
					}
					encoder.EncodeParallel(buffer, data.Size);
					reader.RecycleBuffer(data);
				}
				return encoder.GetTable().GetUnderlyingTable();
			}

			[Benchmark]
			public ulong[] NonParralelEncode()
			{
				var encoder =
					new Encoder<XORTable>(
						new XORTable((int)TableSize),
						HashingFunctionCombinations
						.GetFromSameFamily(3, new MultiplyShiftFamily())
						.GetFactory()((int)TableSize)
						.Select(h => LittleSharp.Utils.Buffering.BufferFunction(h).Compile()),
						1024);

				string fastaFilePath = "test.test";

				var config = FastaFile.Open(new StreamReader(fastaFilePath));
				var reader = new FastaFileReader(config.kMerSize, config.nCharsInFile, config.textReader);

				var buffer = new ulong[1024 * 1024];
				while (true)
				{
					var data = reader.BorrowBuffer();
					if (data is null)
					{
						break;
					}
					encoder.Encode(buffer, data.Size);
					reader.RecycleBuffer(data);
				}
				return encoder.GetTable().GetUnderlyingTable();
			}

			[Benchmark]
			public ulong[] BenchmarkDecoder3()
			{
				var encoder =
					new NoConflictEncoder<XORTable>(
						new XORTable((int)TableSize),
						HashingFunctionCombinations
						.GetFromSameFamily(3, new MultiplyShiftFamily())
						.GetNoConflictFactory()((int)TableSize)
						.Select(h => LittleSharp.Utils.Buffering.BufferFunction(h).Compile()),
						1024);



				string fastaFilePath = "test.test";

				var config = FastaFile.Open(new StreamReader(fastaFilePath));
				var reader = new FastaFileReader(config.kMerSize, config.nCharsInFile, config.textReader);

				var buffer = new ulong[1024 * 1024];
				while (true)
				{
					var data = reader.BorrowBuffer();
					if (data is null)
					{
						break;
					}
					encoder.EncodeParallel(buffer, data.Size);
					reader.RecycleBuffer(data);
				}
				return encoder.GetTable().GetUnderlyingTable();
			}

			[GlobalCleanup]
			public void Cleanup()
			{
				File.Delete("test.test");
			}

		}

	}
}
