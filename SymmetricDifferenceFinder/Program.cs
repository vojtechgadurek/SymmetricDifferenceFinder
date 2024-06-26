﻿using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SymmetricDifferenceFinder;
public class Program
{

	public static void Write(string name, IEnumerable<string> print)
	{
		StreamWriter streamWriter = new StreamWriter(name);
		foreach (var line in print)
		{
			streamWriter.WriteLine(line);
		}
		streamWriter.Close();
	}

	public record class StringTestConfig(double Start, double End, double Step, int TestsInBattery, int StringLenght, int Size, IHashingFunctionFamily HfFamily)
	{
		public IEnumerable<string> Run<T>(Func<double, double, double, int, int, int, IHashingFunctionFamily, T> func)
			where T : IEnumerable<BatteryDecodingResult>
		{
			Console.WriteLine($"Starting test {this.ToString()}");
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			var answer = new string[] { this.ToString() }.Concat(func(Start, End, Step, TestsInBattery, StringLenght, Size, HfFamily).Select(x => x.ToString()).Select(x => { Console.WriteLine(x); return x; })).ToList();

			Console.WriteLine($"Test {this.ToString()} took {stopwatch.ElapsedMilliseconds} ms"); ;

			return answer;
		}
	};

	public record class BasicTestConfig(double Start, double End, double Step, int TestsInBattery, int Size, IHashingFunctionFamily HfFamily)
	{
		public IEnumerable<string> Run<T>(Func<double, double, double, int, int, IHashingFunctionFamily, T> func)
			where T : IEnumerable<BatteryDecodingResult>
		{
			Console.WriteLine($"Starting test {this.ToString()}");
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			var answer = new string[] { this.ToString() }.Concat(func(Start, End, Step, TestsInBattery, Size, HfFamily).Select(x => x.ToString()).Select(x => { Console.WriteLine(x); return x; })).ToList();

			Console.WriteLine($"Test {this.ToString()} took {stopwatch.ElapsedMilliseconds} ms"); ;

			return answer;
		}
	};

	public static void Main(string[] args)
	{
		//StringTestConfig config;
		//config = new StringTestConfig(0.1, 2, 0.05, 1, 40, 10000, new LinearCongruenceFamily() );
		//config = config with { HfFamily = new TabulationFamily() };
		//Write("\\Tests\\KMerRetrieval9lin.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		//config = new StringTestConfig(0.7, 2, 0.05, 10, 100, 10000, new LinearCongruenceFamily());
		//Write("\\Tests\\KMerRetrieval0lin.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		//config = config with { HfFamily = new MultiplyShiftFamily() };

		//Write("\\Tests\\KMerRetrieval0mul.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		//config = new StringTestConfig(0.8, 1.4, 0.005, 100, 100, 10000, new LinearCongruenceFamily());
		//Write("\\Tests\\KMerRetrieval1lin.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		//config = config with { StringLenght = 31 };
		//Write("\\Tests\\KMerRetrieval2lin.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		//config = config with { StringLenght = 10 };
		//Write("\\Tests\\KMerRetrieval3lin.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));


		//config = config with { StringLenght = 1000 };
		//Write("\\Tests\\KMerRetrieval4lin.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));


		BasicTestConfig confBase = new BasicTestConfig(0.72, 0.85, 0.001, 100, 10000, new LinearCongruenceFamily());

		//confBase = confBase with { HfFamily = new TabulationFamily() };

		//Write("\\Tests\\BaseRetrievalHPWlinA.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrieval));


		Write("\\Tests\\BaseRetrievalHPWlin1.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrieval));
		Write("\\Tests\\BaseRetrievalIBLTlin1.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrievalIBLT));



		confBase = confBase with { HfFamily = new MultiplyShiftFamily() };
		Write("\\Tests\\BaseRetrievalIBLTmul1.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrievalIBLT));
		Write("\\Tests\\BaseRetrievalHWPmul1.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrieval));


	}
}
