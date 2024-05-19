using Microsoft.Diagnostics.Tracing;
using SymmetricDifferenceFinder.RetrievalTesting.BatteryTests;
using SymmetricDifferenceFinder.RetrievalTesting.SimpleTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SymmetricDifferenceFinder.Tests
{
	public static class BasicRetrievalTests
	{
		public static void TestIBLT()
		{
			var test = Combinations.Combinations.IBLT;
			var hashingFunction = HashingFunctionCombinations.GetFromSameFamily(3, new LinearCongruenceFamily()).GetNoConflictFactory();

			int size = 10000;
			var batteryTest = new BatteryTest(0.78, 0.82, 0.001, size);
			var factory = new RetrievalTestFactory<IBLTTable, IBLTTable>(test, hashingFunction, (int x) => RandomDataFactory.GetData(x).ToArray());

			var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), 100);

			foreach (var x in answer)
			{
				Console.WriteLine(x.ToString());
			}
		}

		public static void TestIBLT2()
		{
			var test = Combinations.Combinations.IBLT;
			var hashingFunction = HashingFunctionCombinations.GetFromSameFamily(3, new LinearCongruenceFamily()).GetFactory();

			int size = 10000;
			var batteryTest = new BatteryTest(0.75, 0.81, 0.001, size);
			var factory = new RetrievalTestFactory<IBLTTable, IBLTTable>(test, hashingFunction, (int x) => RandomDataFactory.GetData(x).ToArray());

			var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), 100);

			foreach (var x in answer)
			{
				Console.WriteLine(x.ToString());
			}
		}


		public static void TestHPW3()
		{
			var test = Combinations.Combinations.HPW;
			var hashingFunction = HashingFunctionCombinations.GetFromSameFamilyLastWeaker(4, new LinearCongruenceFamily()).GetNoConflictFactory();

			int size = 10000;
			var batteryTest = new BatteryTest(0.7, 0.82, 0.005, size);
			var factory = new RetrievalTestFactory<XORTable, XORTable>(test, hashingFunction, (int x) => RandomDataFactory.GetData(x).ToArray());

			var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), 100);

			foreach (var x in answer)
			{
				Console.WriteLine(x.ToString());
			}
		}



		public static void TestXOR()
		{
			var test = Combinations.Combinations.HPW;
			var hashingFunction = HashingFunctionCombinations.GetFromSameFamily(3, new LinearCongruenceFamily()).GetNoConflictFactory();

			int size = 10000;
			var batteryTest = new BatteryTest(0.75, 0.82, 0.005, size);
			var factory = new RetrievalTestFactory<XORTable, XORTable>(test, hashingFunction, (int x) => RandomDataFactory.GetData(x).ToArray());

			var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), 100);

			foreach (var x in answer)
			{
				Console.WriteLine(x.ToString());
			}
		}

		public static void TestXOR2()
		{
			var test = Combinations.Combinations.HPW;
			var hashingFunction = HashingFunctionCombinations.GetFromSameFamily(3, new LinearCongruenceFamily()).GetFactory();

			int size = 10000;
			var batteryTest = new BatteryTest(0.75, 0.82, 0.005, size);
			var factory = new RetrievalTestFactory<XORTable, XORTable>(test, hashingFunction, (int x) => RandomDataFactory.GetData(x).ToArray());

			var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), 100);

			foreach (var x in answer)
			{
				Console.WriteLine(x.ToString());
			}
		}

	}
}
