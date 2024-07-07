using LittleSharp.Literals;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using SymmetricDifferenceFinder.RetrievalTesting.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SymmetricDifferenceFinderTests.Decoder
{
	public class TestTabulation
	{
		ITestOutputHelper _output;
		public TestTabulation(ITestOutputHelper output)
		{
			_output = output;
			Scope._debugOutput = _output;
		}
		[Fact]
		void TestTabulationDecode()
		{
			var size = 10Ul;
			var tf = new TabulationFamily();
			var data = RandomDataFactory.GetRandomData((int)size * 5 / 10);


			var hsf = new List<Expression<Func<ulong, ulong>>> { tf.GetScheme(size, 0).Create(), tf.GetScheme(size, 0).Create(), tf.GetScheme(size, 0).Create() };
			List<Func<ulong, ulong>> hfsL = hsf.Select(x => x.Compile()).ToList();
			{
				var table = new XORTable((int)size);



				foreach (var hf in hfsL)
				{
					foreach (var item in data)
					{
						var hash = hf(item);
						_output.WriteLine($"Init {hash} value");
						table.Add(hash, item);
					}
				}


				var decoder = new HPWDecoderFactory<XORTable>(hsf).Create(table);
				decoder.Decode();
				var decodedValues = decoder.GetDecodedValues();
				decodedValues.SymmetricExceptWith(data);
			}
			{
				var table = new XORTable((int)size);
				foreach (var hf in hfsL)
				{
					foreach (var item in data)
					{
						var hash = hf(item);
						_output.WriteLine($"Init {hash} value");

						table.Add(hash, item);
					}
				}


				var decoder = new HPWDecoderFactory<XORTable>(hsf).Create(table);
				bool IsPure(ulong hash)
				{
					bool check = false;
					var x = decoder.Sketch.Get(hash);
					_output.WriteLine($"{x} value");
					if (x == 0)
					{
						_output.WriteLine($"{hash} is not pure ZERO");
						return false;
					};

					foreach (var hf in hfsL)
					{
						_output.WriteLine(hf(x).ToString());
						if (hf(x) == hash)
						{
							_output.WriteLine($"{hash} is pure");
							return true;
						};
					}

					_output.WriteLine($"{hash} is not pure");
					return false; ;

				}


				HashSet<ulong> pure = new HashSet<ulong>();

				foreach (var i in Enumerable.Range(0, decoder.Size))
				{

					if (IsPure((ulong)i))
					{
						pure.Add((ulong)i);
					}
				};

				HashSet<ulong> nextPure = new HashSet<ulong>();
				HashSet<ulong> decodedValues = new();

				while (pure.Count > 0)
				{
					foreach (var p in pure)
					{
						if (IsPure(p))
						{
							var x = decoder.Sketch.Get(p);
							foreach (var hf in hfsL)
							{
								decoder.Sketch.Toggle(hf(x), x);
								if (true /*IsPure(hf(x))*/) nextPure.Add(hf(x));
							}

							if (decodedValues.Contains(x))
							{
								decodedValues.Remove(x);
							}
							else
							{
								decodedValues.Add(x);
							}
						}
					}
					HashSet<ulong> oldPure = pure;
					pure = nextPure;
					nextPure = oldPure;
					nextPure.Clear();
				}

			}


		}

		[Fact]
		void TestTabulationSimple()
		{
			var size = 10Ul;
			var tf = new TabulationFamily();
			var data = new HashSet<ulong>() { 10, 20, 30, 40, 50 };


			var hsf = new List<Expression<Func<ulong, ulong>>> { tf.GetScheme(size, 0).Create(), tf.GetScheme(size, 0).Create(), tf.GetScheme(size, 0).Create() };
			List<Func<ulong, ulong>> hfsL = hsf.Select(x => x.Compile()).ToList();
			{
				var table = new XORTable((int)size);

				for (ulong i = 0; i < size; i++)
				{
					table.Toggle(i, 100);
				}



				var decoder = new HPWDecoderFactory<XORTable>(hsf).Create(table);
				decoder.Decode();
				var decodedValues = decoder.GetDecodedValues();
				decodedValues.SymmetricExceptWith(data);
			}
		}
	}
}
