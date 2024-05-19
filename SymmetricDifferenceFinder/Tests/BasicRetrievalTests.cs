using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Diagnostics.Tracing;
using SymmetricDifferenceFinder.Improvements;
using SymmetricDifferenceFinder.RetrievalTesting.BatteryTests;
using SymmetricDifferenceFinder.RetrievalTesting.SimpleTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SymmetricDifferenceFinder.Tests
{
	public static class BasicRetrievalTests
	{
		public static void TestIBLT()
		{
			var test = Combinations.Combinations.IBLT();
			var hashingFunction = HashingFunctionCombinations.GetFromSameFamily(3, new LinearCongruenceFamily()).GetNoConflictFactory();

			int size = 10000;
			var batteryTest = new BatteryTest(0.78, 0.82, 0.001, size);
			var factory = new RetrievalTestFactory<IBLTTable, IBLTTable>(test, hashingFunction, (int x) => RandomDataFactory.GetRandomData(x).ToArray());

			var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), 100);

			foreach (var x in answer)
			{
				Console.WriteLine(x.ToString());
			}
		}

		public static void TestIBLT2()
		{
			var test = Combinations.Combinations.IBLT();
			var hashingFunction = HashingFunctionCombinations.GetFromSameFamily(3, new LinearCongruenceFamily()).GetFactory();

			int size = 10000;
			var batteryTest = new BatteryTest(0.75, 0.81, 0.001, size);
			var factory = new RetrievalTestFactory<IBLTTable, IBLTTable>(test, hashingFunction, (int x) => RandomDataFactory.GetRandomData(x).ToArray());

			var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), 100);

			foreach (var x in answer)
			{
				Console.WriteLine(x.ToString());
			}
		}


		public static void TestHPW3()
		{
			var test = Combinations.Combinations.HPW();
			var hashingFunction = HashingFunctionCombinations.GetFromSameFamilyLastWeaker(4, new LinearCongruenceFamily()).GetNoConflictFactory();

			int size = 10000;
			var batteryTest = new BatteryTest(0.7, 0.82, 0.005, size);
			var factory = new RetrievalTestFactory<XORTable, XORTable>(test, hashingFunction, (int x) => RandomDataFactory.GetRandomData(x).ToArray());

			var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), 100);

			foreach (var x in answer)
			{
				Console.WriteLine(x.ToString());
			}
		}


		public static void TestMassagers()
		{
			var test = Combinations.Combinations.HPW();
			var hashingFunction = HashingFunctionCombinations.GetFromSameFamily(3, new LinearCongruenceFamily()).GetNoConflictFactory();


			var decoderFactory = test.DecoderFactoryFactory;
			Random random = new Random();


			test.SetDecoderFactoryFactory((hfs) => new MassagerFactory(
				hfs,
				(HPWDecoderFactory<XORTable>)decoderFactory(hfs)));

			int size = 10000;
			var batteryTest = new BatteryTest(1, 2, 0.1, size);
			var factory = new RetrievalTestFactory<XORTable, XORTable>(test, hashingFunction, (int x) => RandomDataFactory.GetRandomStringData(x, 31).ToArray());

			var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), 100);

			foreach (var x in answer)
			{
				Console.WriteLine(x.ToString());
			}
		}

		public static void TestMassagersConflict()
		{
			var test = Combinations.Combinations.HPW();
			var hashingFunction = HashingFunctionCombinations.GetFromSameFamilyLastWeaker(4, new LinearCongruenceFamily()).GetNoConflictFactory();


			var decoderFactory = test.DecoderFactoryFactory;
			Random random = new Random();


			test.SetDecoderFactoryFactory((hfs) => new MassagerFactory(
				hfs,
				(HPWDecoderFactory<XORTable>)decoderFactory(hfs)));

			int size = 10000;
			var batteryTest = new BatteryTest(1, 2, 0.1, size);
			var factory = new RetrievalTestFactory<XORTable, XORTable>(test, hashingFunction, (int x) => RandomDataFactory.GetRandomStringData(x, 31).ToArray());

			var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), 100);

			foreach (var x in answer)
			{
				Console.WriteLine(x.ToString());
			}
		}



		public static void TestXOR()
		{
			var test = Combinations.Combinations.HPW();
			var hashingFunction = HashingFunctionCombinations.GetFromSameFamily(3, new LinearCongruenceFamily()).GetNoConflictFactory();

			int size = 10000;
			var batteryTest = new BatteryTest(0.75, 0.82, 0.005, size);
			var factory = new RetrievalTestFactory<XORTable, XORTable>(test, hashingFunction, (int x) => RandomDataFactory.GetRandomData(x).ToArray());

			var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), 100);

			foreach (var x in answer)
			{
				Console.WriteLine(x.ToString());
			}
		}

		public static void TestXOR2()
		{
			var test = Combinations.Combinations.HPW();
			var hashingFunction = HashingFunctionCombinations.GetFromSameFamily(3, new LinearCongruenceFamily()).GetFactory();

			int size = 10000;
			var batteryTest = new BatteryTest(0.75, 0.82, 0.005, size);
			var factory = new RetrievalTestFactory<XORTable, XORTable>(test, hashingFunction, (int x) => RandomDataFactory.GetRandomData(x).ToArray());

			var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), 100);

			foreach (var x in answer)
			{
				Console.WriteLine(x.ToString());
			}
		}

	}
}
