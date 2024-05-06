using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinderBenchmarks.Experiments
{
	public class DeVirtualizationBenchmarks
	{
		readonly Func<ulong, ulong> readonlyDivide = (x) => x / 2;
		Func<ulong, ulong> divide = (x) => x / 2;


		Random random = new Random();
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

		public ulong DivideAndSum()
		{
			ulong sum = 0;

			foreach (var buffer in stream)
			{
				for (int i = 0; i < buffer.Length; i++)
				{
					sum += divide(buffer[i]);
				}
			}
			return sum;
		}

		[Benchmark]
		public ulong ReadonlyDivideAndSum()
		{
			ulong sum = 0;

			var divider = new Divider(readonlyDivide);
			foreach (var buffer in stream)
			{
				for (int i = 0; i < buffer.Length; i++)
				{
					sum += divider.readonlyDivide(buffer[i]);
				}
			}
			return sum;
		}

		public class Divider
		{
			public readonly Func<ulong, ulong> readonlyDivide;
			public Divider(Func<ulong, ulong> divider)
			{
				readonlyDivide = divider;
			}


		}

		[Benchmark(Baseline = true)]
		public ulong InlineDivideAndSum()
		{
			ulong sum = 0;

			foreach (var buffer in stream)
			{
				for (int i = 0; i < buffer.Length; i++)
				{
					sum += buffer[i] / 2;
				}
			}
			return sum;
		}
	}
}
