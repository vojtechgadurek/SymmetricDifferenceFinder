using System.Diagnostics;
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

	public record class StringTestConfig(double Start, double End, double Step, int TestsInBattery, int StringLenght, int Size, Type stringFactory, Type pipeline, IHashingFunctionFamily HfFamily)
	{
		public IEnumerable<string> Run<T>(Func<double, double, double, int, int, int, IHashingFunctionFamily, Type, Type, T> func)
			where T : IEnumerable<BatteryDecodingResult>
		{
			Console.WriteLine($"Starting test {this.ToString()}");
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			var answer = new string[] { this.ToString() }.Concat(func(Start, End, Step, TestsInBattery, StringLenght, Size, HfFamily, stringFactory, pipeline).Select(x => x.ToString()).Select(x => { Console.WriteLine(x); return x; })).ToList();

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
		StringTestConfig config;
		config = new StringTestConfig(0.1, 2, 0.05, 1, 100, 10000, typeof(KMerStringFactory), typeof(CanonicalOrder), new TabulationFamily());
		config = config with { HfFamily = new LinearCongruenceFamily() };

		//config = config with { HfFamily = new TabulationFamily() };
		//Write("\\Tests\\WARMUP.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		//config = config with { TestsInBattery = 500, Step = 0.01, Start = 1.0, End = 1.35 };
		//config = config with { HfFamily = new MultiplyShiftFamily() };
		//Write("\\Tests\\KMerRetrieval0mul.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		//Write("\\Tests\\KMerRetrieval0lin.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));


		//config = config with { pipeline = typeof(None) };

		//Write("\\Tests\\KMerRetrieval1mulNone.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		//config = config with { HfFamily = new LinearCongruenceFamily() };

		//Write("\\Tests\\KMerRetrieval1LinNone.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		config = config with { HfFamily = new TabulationFamily() };
		config = config with { pipeline = typeof(CanonicalOrder), TestsInBattery = 40, Start = 1.3, End = 1.6, Step = 0.01, StringLenght = 5 };

		config = config with { StringLenght = 5 };
		Write("\\Tests\\KMerRetrieval5tab1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		config = config with { StringLenght = 10 };
		Write("\\Tests\\KMerRetrieval10tab1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		config = config with { StringLenght = 100 };
		Write("\\Tests\\KMerRetrieval100tab1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));


		config = config with { StringLenght = 1000 };
		Write("\\Tests\\KMerRetrieval1000tab1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		config = config with { StringLenght = 10000 };
		Write("\\Tests\\KMerRetrieval10000tab1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));


		config = config with { HfFamily = new LinearCongruenceFamily() };

		config = config with { StringLenght = 5 };
		Write("\\Tests\\KMerRetrieval5lin1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		config = config with { StringLenght = 10 };
		Write("\\Tests\\KMerRetrieval10lin1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		config = config with { StringLenght = 100 };
		Write("\\Tests\\KMerRetrieval100lin1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));


		config = config with { StringLenght = 1000 };
		Write("\\Tests\\KMerRetrieval1000lin1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		config = config with { StringLenght = 10000 };
		Write("\\Tests\\KMerRetrieval10000lin1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));


		config = config with { pipeline = typeof(None) };
		config = config with { HfFamily = new TabulationFamily() };


		config = config with { StringLenght = 5 };
		Write("\\Tests\\KMerRetrieval5tabNone1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		config = config with { StringLenght = 10 };
		Write("\\Tests\\KMerRetrieval10tabNone1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		config = config with { StringLenght = 100 };
		Write("\\Tests\\KMerRetrieval100tabNone1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));


		config = config with { StringLenght = 1000 };
		Write("\\Tests\\KMerRetrieval1000tabNone1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		config = config with { StringLenght = 10000 };
		Write("\\Tests\\KMerRetrieval10000tabNone1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));



		//config = config with { StringLenght = 10000, stringFactory = typeof(NumberStringFactory), pipeline = typeof(None), End = 2 };
		//config = config with { HfFamily = new TabulationFamily() };


		//Write("\\Tests\\KMerRetrievalStringTab.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

		//Write("\\Tests\\KMerRetrievalStringTab.txt", config.Run(Tests.BasicRetrievalTests.TestMassagersWeaker));


		BasicTestConfig confBase = new BasicTestConfig(0.72, 0.85, 0.001, 100, 10000, new PolynomialFamily(3));




		//confBase = confBase with { HfFamily = new TabulationFamily(), Start = 0.1, Step = 0.1, TestsInBattery = 1 };


		//////Warm up
		////	Write("\\Tests\\WARMUP.txt", (confBase with { End = 0.73 }).Run(Tests.BasicRetrievalTests.TestRetrieval));








		////confBase = confBase with { HfFamily = new MultiplyShiftFamily() };
		////Write("\\Tests\\BaseRetrievalIBLTmul3.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrievalIBLT));
		////Write("\\Tests\\BaseRetrievalHWPmul3.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrieval));

		//confBase = confBase with { HfFamily = new LinearCongruenceFamily() };
		//Write("\\Tests\\BaseRetrievalIBLTlin3.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrievalIBLT));
		//Write("\\Tests\\BaseRetrievalHWPlin3.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrieval));

		//confBase = confBase with { HfFamily = new TabulationFamily() };


		//Write("\\Tests\\BaseRetrievalHPWTab3.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrieval));
		//Write("\\Tests\\BaseRetrievalIBLTTab3.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrievalIBLT));


		//confBase = confBase with { HfFamily = new PolynomialFamily(2) };


		//Write("\\Tests\\BaseRetrievalHPWpol2.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrieval));
		//Write("\\Tests\\BaseRetrievalIBLTpol2.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrievalIBLT));

	}
}
