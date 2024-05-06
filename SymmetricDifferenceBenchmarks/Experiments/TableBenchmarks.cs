using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Microsoft.Diagnostics.Runtime;
using BenchmarkDotNet.Diagnosers;

namespace SymmetricDifferenceFinderBenchmarks.Experiments
{
	[DisassemblyDiagnoser(printInstructionAddresses: true, syntax: DisassemblySyntax.Masm)]
	public class TableBenchmarks
	{
		static Random random = new Random();
		ulong[] data = new ulong[1024];
		void RandomDataGenerate()
		{
			random.NextBytes(MemoryMarshal.Cast<ulong, byte>(data));
		}

		Func<ulong[], int, ulong>? Xor;
		Func<ulong[], ulong>? XorSetSize;

		int Length = 1024;
		int BufferLength = 4096;

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

		[GlobalSetup]
		public void Setup()
		{
			RandomDataGenerate();
			{
				var f = CompiledFunctions.Create<ulong[], int, ulong>(out var table_, out var nItems);

				f.S.DeclareVariable<int>(out var i_, 0)
					.Assign(f.Output, 0)
					.While(
					i_.V < nItems.V,
					new Scope()
						.Assign(f.Output, f.Output.V ^ table_.V.ToTable<ulong>()[i_.V].V)
						.Assign(i_, i_.V + 1)
					);
				Xor = f.Construct().Compile();
			}

			{
				var f2 = CompiledFunctions.Create<ulong[], ulong>(out var table_);
				f2.S.DeclareVariable<int>(out var i_, 0)
					.DeclareVariable<int>(out var nItems, 1024)
					.Assign(f2.Output, 0)
					.While(
					i_.V < nItems.V,
					new Scope()
						.Assign(f2.Output, f2.Output.V ^ table_.V.ToTable<ulong>()[i_.V].V)
						.Assign(i_, i_.V + 1)
					);


				XorSetSize = f2.Construct().Compile();
			}
		}

		[Benchmark(Baseline = true)]
		public ulong TestBase()
		{
			ulong answer = 0;

			foreach (var buffer in stream)
			{
				for (int i = 0; i < buffer.Length; i++)
				{
					answer ^= buffer[i];
				}
			}
			return answer;
		}

		interface IXORField
		{
			void Xor(ulong value);
			ulong Get();
		}
		record struct XORField : IXORField
		{
			ulong _field = 0;

			public XORField()
			{
			}

			public ulong Get()
			{
				return _field;
			}

			public void Xor(ulong value)
			{
				_field ^= value;
			}
		}

		class BufferXOR<T> where T : struct, IXORField
		{

			T field;

			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveInlining)]
			public ulong Encode(ulong[] values)
			{
				for (int i = 0; i < values.Length; i++)
				{
					field.Xor(values[i]);
				}
				return field.Get();
			}

			public ulong Get()
			{
				return field.Get();
			}

		}

		[Benchmark]

		public ulong TestStructInterfaceMethod()
		{
			var answer = new BufferXOR<XORField>();
			foreach (var buffer in stream)
			{
				answer.Encode(buffer);
			}
			return answer.Get();
		}



	}
}
