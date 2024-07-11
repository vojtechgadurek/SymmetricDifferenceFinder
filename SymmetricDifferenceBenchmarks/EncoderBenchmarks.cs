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
			public static IEnumerable<Type> HashingFunctionsToTest() => [typeof(MultiplyShiftFamily)];

			public const int Length = 1024;
			public const int BufferLength = 4096;
			public const int DataLength = Length * BufferLength;

			[ParamsSource(nameof(TableLengths))]
			public ulong TableSize;


			public static IEnumerable<ulong> TableLengths()
				=> Enumerable.Range(10, 27).Select(x => 1ul << x);

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
			public object ParallelEncode()
			{
				var hfs = HashingFunctionCombinations
						.GetFromSameFamily(3, new MultiplyShiftFamily())
						.GetNoConflictFactory()((int)TableSize)
						.Select(h => LittleSharp.Utils.Buffering.BufferFunction(h).Compile()).ToList();

				var GetEncoder = () =>
					new Encoder<XORTable>(
						new XORTable((int)TableSize),
						hfs,
						1024);


				string fastaFilePath = "test.test";

				var config = FastaFile.Open(new StreamReader(fastaFilePath));
				var reader = new FastaFileReader(config.kMerSize, config.nCharsInFile, config.textReader);

				var buffer = new ulong[1024 * 1024];
				var CreateTask = () => new Task<XORTable>(() =>
				{
					var encoder = GetEncoder();
					while (true)
					{
						FastaFileReader.Buffer? data;
						lock (reader)
						{
							data = reader.BorrowBuffer();
						}
						if (data is null)
						{
							break;
						}
						encoder.Encode(data.Data, data.Size);
						reader.RecycleBuffer(data);
					}
					return encoder.GetTable();
				});

				var tasks = Enumerable.Range(0, 4).Select(_ => CreateTask()).ToArray();
				foreach (var task in tasks)
				{
					task.Start();
				}
				Task.WaitAll(tasks.ToArray());
				return tasks.Aggregate(new XORTable((int)TableSize), (t, x) => x.Result.SymmetricDifference(t));
			}

			[GlobalCleanup]
			public void Cleanup()
			{
				File.Delete("test.test");
			}

		}

	}
}
