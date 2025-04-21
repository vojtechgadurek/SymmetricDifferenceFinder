
using Iced.Intel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RedaFasta;
using SymmetricDifferenceFinder.Improvements;
using SymmetricDifferenceFinder.Improvements.StringFactories;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using KMerUtils.KMer;
using Microsoft.Diagnostics.Tracing.Parsers.JScript;
using Microsoft.Diagnostics.Tracing.Parsers.AspNet;
using System.Runtime.InteropServices;

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

    public record class StringTestConfig(double Start, double End, double Step, int TestsInBattery, int StringLenght, int Size, Type stringFactory, Type pipeline, IHashFunctionFamily HfFamily)
    {
        public IEnumerable<string> Run<T>(Func<double, double, double, int, int, int, IHashFunctionFamily, Type, Type, T> func)
            where T : IEnumerable<BatteryDecodingResult>
        {
            Console.WriteLine($"Starting test {this.ToString()}");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var answer = new string[] { this.ToString() }.Concat(func(Start, End, Step, TestsInBattery, StringLenght, Size, HfFamily, stringFactory, pipeline)
                .Select(x => x.ToString()).Select(x => { Console.WriteLine(x); return x; })).ToList();

            Console.WriteLine($"Test {this.ToString()} took {stopwatch.ElapsedMilliseconds} ms"); ;

            return answer;
        }
    };

    public record class BasicTestConfig(double Start, double End, double Step, int TestsInBattery, int Size, IHashFunctionFamily HfFamily)
    {
        public IEnumerable<string> Run<T>(Func<double, double, double, int, int, IHashFunctionFamily, T> func)
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


    public static ulong[] LoadKmersFromFile(string filename)
    {
        //load strings from file 
        string fastaFilePath = filename;
        TextReader textReader = new StreamReader(fastaFilePath);
        var cfg = FastaFile.Open(textReader);
        var reader = new FastaFileReader(cfg.kMerSize, cfg.nCharsInFile, cfg.textReader);
        //Load data
        ulong[] data = new ulong[cfg.nCharsInFile];
        int i = 0;

        while (true)
        {
            var buffer = reader.BorrowBuffer();
            if (buffer == null)
            {
                break;
            }
            Array.Copy(buffer.Data, 0, data, i, buffer.Size);
            i += buffer.Size;
            reader.RecycleBuffer(buffer);
        }
        return data;
    }


    record struct TestResult(int TableSize, int NItems, int IncorrectlyRecovered, long time);


    public static void GrapRecovery(HPWDecoder<XORTable> decoder, Encoder<XORTable> encoder, int kMerLength, int maxDistance, int minDistance)
    {
        Random random = new Random();
        var newlyGuessed =
            KMerUtils.DNAGraph.Recover.RecoverGraphCanonicalV3(
                decoder.GetDecodedValues()
                //RemoveHeaders
                .Select(x => x >>> 2)
                .ToArray(), kMerLength, maxDistance, minDistance, false
                ).Where(_ => random.Next(0, 1) == 0).ToArray();



        newlyGuessed.ToHashSet().ExceptWith(decoder.GetDecodedValues());

        //We should not forget that some of the values are already in the set
        //And we do not want to lose them

        //HPWWithOracle.Decode();
        encoder.Encode(newlyGuessed.ToArray(), newlyGuessed.Count());
    }

    public static void TestFixedData(Span<string> args)
    {
        //ARGS

        //datasource startingsize enddingsize 
        HashSet<ulong> hashsetData;
        ulong[] data;
        int starttablesize;
        int endtabelesize;
        int nTests;

        int argscount = 0;
        string datasource = args[argscount++];
        double starttablesizecof = double.Parse(args[argscount++]);
        double endtabelesizecof = double.Parse(args[argscount++]);
        double stepcof = double.Parse(args[argscount++]);
        int decoderSteps = int.Parse(args[argscount++]);

        nTests = int.Parse(args[argscount++]);
        var hashFunctionTypes = args[argscount++].Split('-').Select(x => HashFunctionProvider.GetFamilyByName(x)).ToList();
        string fileToStoreResults = args[argscount++];

        const string filecall = "file-";
        const string generatedcall = "generate-";

        bool graph_recovery = false;
        string[] graph_des =args[argscount++].Split("-");

        int min_distance = 0;
        int max_distance = 0;
        int graph_steps = 0;

        if (graph_des[0].StartsWith("graph")){
            max_distance = int.Parse(graph_des[1]);
            graph_steps = int.Parse(graph_des[2]);
            Console.WriteLine("graph");
        }
        if (datasource.StartsWith(filecall))
        {
            data = LoadKmersFromFile(datasource.Substring(filecall.Length));
            hashsetData = new HashSet<ulong>(data);
            hashsetData.Remove(0ul);
            data = hashsetData.ToArray();
            if (data.Contains(0ul)) throw new ArgumentException("Data contains 0");
            Console.WriteLine(hashsetData.Count);
        }
        else if (datasource.StartsWith(generatedcall))
        {
            var argsfile = datasource.Substring(generatedcall.Length).Split('-');
            int nStrings = int.Parse(argsfile[0]);
            int stringLength = int.Parse(argsfile[1]);

            hashsetData = StringDataFactory<KMerStringFactory, CanonicalOrder>.GetRandomStringData(nStrings * stringLength, stringLength);
            data = hashsetData.ToArray();
            Console.WriteLine(data.Length);
        }
        else
        {
            throw new ArgumentException("Unknown data source");
        }


        starttablesize = (int)(starttablesizecof * data.Length);
        endtabelesize = (int)(endtabelesizecof * data.Length);

        Console.WriteLine(starttablesize);
        Console.WriteLine(endtabelesize);

        int step = (int)((endtabelesize - starttablesize) * stepcof);


        Stopwatch stopwatch = new();


        TestResult DoOneTest(ulong tableSize)
        {

            List<IHashFunctionScheme> schemes = hashFunctionTypes.Select(x => HashFunctionProvider.Get(x, tableSize, 0)).ToList();

            EncoderFactory<XORTable> encoderFactory = new EncoderFactory<XORTable>(new EncoderConfiguration<XORTable>(schemes, (int)tableSize), 
                size => new XORTable(size));

            var encoder = encoderFactory.Create();
            encoder.Encode(hashsetData.ToArray(), hashsetData.Count());

            HPWDecoderFactory<XORTable> table = new HPWDecoderFactory<XORTable>(schemes.Select(x => x.Create()));
            var decoder = table.Create(encoder.GetTable());

            Massager<KMerStringFactory, CanonicalOrder> massager = 
                new Massager<KMerStringFactory, CanonicalOrder>(decoder, schemes.Select(x => x.Create().Compile()));

            massager.NStepsRecovery =decoderSteps;

            stopwatch.Restart();

            if (graph_recovery)
                massager.NStepsRecovery = 100;
            
            massager.Decode();

            if(graph_recovery)
            {
                massager.NStepsRecovery = decoderSteps;
                for(int i = 0; i < graph_steps; i++)
                {
                    GrapRecovery(massager.HPWDecoder, encoder, 31, max_distance, min_distance);
                    massager.Decode();
                    if(massager.DecodingState == DecodingState.Success)
                    {
                        break;
                    }
                }
            }
            stopwatch.Stop();
            var ans = new HashSet<ulong>(massager.GetDecodedValues());
            ans.SymmetricExceptWith(hashsetData);
            return new TestResult((int)tableSize, hashsetData.Count(), ans.Count(), stopwatch.ElapsedMilliseconds);
        }

        int tableSize = starttablesize;



        var allresults = new List<TestResult[]>();

        while (tableSize < endtabelesize)
        {
            Console.WriteLine($"table size {tableSize}");

            var results = new TestResult[nTests];

            var op = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
            Parallel.ForEach(Enumerable.Range(0, nTests), op, (i) => { results[i] = DoOneTest((ulong)tableSize); });

            var average_decoder = results.Average(x => x.IncorrectlyRecovered);
            Console.WriteLine($"table size finished {tableSize}; mul {tableSize / (double)hashsetData.Count()}, {average_decoder}");
            allresults.Add(results);
            tableSize += step;
        }

        File.WriteAllText(
            fileToStoreResults, JsonSerializer.Serialize(allresults));

    }

    public static void MultiplierSearch(Span<string> args)
    {

    }


    public static double MultiplierSearch(double minMultiply, double maxMultiply, int steps, Func<double, bool> Test)
    {
        for (int i = 0; i < steps; i++)
        {
            var mid = (minMultiply + maxMultiply) / 2;
            if (Test(mid))
            {
                //Console.WriteLine((mid, "Succ"));
                minMultiply = mid;
            }
            else
            {
                //Console.WriteLine((mid, "Fail"));
                maxMultiply = mid;
            }
        }
        return minMultiply;
    }

    public static Func<double, bool> TestMultiplier(IEnumerable<IHashFunctionScheme> schemes, int tableSize, int nTests, Func<int, ulong[]> dataProvider)
    {
        EncoderFactory<XORTable> encoderFactory = new EncoderFactory<XORTable>(new EncoderConfiguration<XORTable>(schemes, (int)tableSize), size => new XORTable(size));
        HPWDecoderFactory<XORTable> decoderFactory = new HPWDecoderFactory<XORTable>(schemes.Select(x => x.Create()));

        var hfs = schemes.Select(x => x.Create().Compile());

        bool OneTest(double multiply)
        {

            for (int i = 0; i < nTests; i++)
            {
                var encoder = encoderFactory.Create();
                var decoder = decoderFactory.Create(encoder.GetTable());
                var massager = new Massager<KMerStringFactory, CanonicalOrder>(decoder, hfs);
                var data = dataProvider((int)(tableSize * multiply));
                encoder.Encode(data, data.Length);

                massager.NStepsRecovery = 1000;
                massager.NStepsDecoder = 100;
                massager.NStepsDecoderInitial = 1000;
                massager.Decode();


                massager.GetDecodedValues().SymmetricExceptWith(data);
                if (massager.GetDecodedValues().Count() != 0)
                {
                    return false;
                }
            }
            return true;
        }
        return OneTest;
    }

    public static Func<double, double> TestMul(IEnumerable<IHashFunctionScheme> schemes, int tableSize, int nTests, Func<int, ulong[]> dataProvider, 
        (int recovery, int decoder, int initial) config)
    {
        EncoderFactory<XORTable> encoderFactory = new EncoderFactory<XORTable>(new EncoderConfiguration<XORTable>(schemes, (int)tableSize), size => new XORTable(size));
        HPWDecoderFactory<XORTable> decoderFactory = new HPWDecoderFactory<XORTable>(schemes.Select(x => x.Create()));

        var hfs = schemes.Select(x => x.Create().Compile());

        double OneTest(double multiply)
        {
            double countSucc = 0;
            for (int i = 0; i < nTests; i++)
            {
                var encoder = encoderFactory.Create();
                var decoder = decoderFactory.Create(encoder.GetTable());
                var massager = new Massager<KMerStringFactory, CanonicalOrder>(decoder, hfs);
                var data = dataProvider((int)(tableSize * multiply));
                encoder.Encode(data, data.Length);


                massager.NStepsRecovery = config.recovery;
                massager.NStepsDecoder = config.decoder;
                massager.NStepsDecoderInitial = config.initial;
                massager.Decode();


                massager.GetDecodedValues().SymmetricExceptWith(data);
                if (massager.GetDecodedValues().Count() == 0 ) countSucc+=1;
            }
            return countSucc / nTests;
        }
        return OneTest;
    }


    public static List<(int, double)> TestDifferentKMerLengths(int startKmerLength, int endKmerLength, double step, int nSteps, int nTests, int tableSize, IEnumerable<IHashFunctionScheme> hfs)
    {

        List<int> values = new();
        List<(int, double)> result = new();


        while (startKmerLength < endKmerLength)
        {
            values.Add(startKmerLength);
            startKmerLength = (int)Math.Ceiling(startKmerLength * step);
        }

        Parallel.ForEach(values, (startKmerLength) =>
        {
            //Console.WriteLine($"Currently - {startKmerLength} - is tested");
            startKmerLength = (int)Math.Ceiling(startKmerLength * step);
            var f = TestMultiplier(hfs,
                    tableSize, nTests,
                    x => StringDataFactory<KMerStringFactory, CanonicalOrder>.GetRandomStringData(x, startKmerLength).ToArray());

            var res = (startKmerLength,
                MultiplierSearch(
                    0.1, 2, nSteps,
                    f
                    ));

            lock (result) { result.Add(res); }

            Console.WriteLine(result[^1]);

        });
        return result;
    }
    public static List<(int,double, double, long)> TestDifferentKMerLengthsMul(int startKmerLength, int endKmerLength, double step, int nSteps, int nTests, int tableSize, 
        IEnumerable<IHashFunctionScheme> hfs,
        (int, int, int) config,
        double startmul,
        double endmul,
        double mulstep,
        int nThreads)
    {

        List<int> values = new();
        List<(int,double, double, long)> result = new();


        while (startKmerLength < endKmerLength)
        {
            values.Add(startKmerLength);
            startKmerLength = (int)Math.Ceiling(startKmerLength * step);
        }
        ParallelOptions parallelOptions = new ParallelOptions();
        parallelOptions.MaxDegreeOfParallelism = nThreads;
        Parallel.ForEach(values, parallelOptions, (startKmerLength) =>
        {
            //Console.WriteLine($"Currently - {startKmerLength} - is tested");
            startKmerLength = (int)Math.Ceiling(startKmerLength * step);
            var f = TestMul(hfs,
                    tableSize, nTests,
                    x => StringDataFactory<KMerStringFactory, CanonicalOrder>.GetRandomStringData(x, startKmerLength).ToArray(),
                    config
                    );

            Stopwatch stop = new Stopwatch();
            for (double i = startmul; i < endmul; i += mulstep)
            {
                stop.Restart();
                var r = f(i);
                stop.Stop();

                lock (result) {
                    var res = (startKmerLength, i, r, stop.ElapsedMilliseconds);
                    Console.WriteLine(res);
                    result.Add(res);
                };
            }
        });
        return result;
    }



    public static void Main(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            args = ["a"];
        }

        if (args[0] == "data")
        {


            foreach (var i in StringDataFactory<KMerStringFactory, None>.GetRandomStringData(100, 10)
                .ToArray()) Console.WriteLine(i.ToStringRepre(11));

            ulong kMer = 0;

            Console.WriteLine("-----------------");
            for (int i = 0; i < 100; i++)
            {
                var f = new KMerStringFactory();
                Console.WriteLine(kMer.ToStringRepre(11));
                kMer = f.GetPossibleBefore(kMer)[Random.Shared.Next(4)];
            }
            return;
        }
        ;
        if (args[0] == "fixed-data")
        {
            TestFixedData(args.AsSpan().Slice(1));
            return;
        }
        if (args[0] == "multiplier-search")
        {
            int argscount = 1;
            string answerFile = args[argscount++];
            ulong tableSize = ulong.Parse(args[argscount++]);
            var hashFunctionTypes = args[argscount++].Split('-').Select(x => HashFunctionProvider.Get(HashFunctionProvider.GetFamilyByName(x), tableSize, 0)).ToList();
            int nTests = int.Parse(args[argscount++]);
            int nSteps = int.Parse(args[argscount++]);
            int minKmerLength = int.Parse(args[argscount++]);
            int maxKmerLength = int.Parse(args[argscount++]);
            double step = double.Parse(args[argscount++]);

            var result = TestDifferentKMerLengths((int)minKmerLength, (int)maxKmerLength, step, nSteps, nTests, (int)tableSize, hashFunctionTypes);
            File.WriteAllText(answerFile, String.Join("\n", result.Select(x => $"{x.Item1} {x.Item2}")));
            return;
        }

        if (args[0] == "mul-search")
        {
            int argscount = 1;
            string answerFile = args[argscount++];
            ulong tableSize = ulong.Parse(args[argscount++]);
            var hashFunctionTypes = args[argscount++].Split('-').Select(x => HashFunctionProvider.Get(HashFunctionProvider.GetFamilyByName(x), tableSize, 0)).ToList();
            int nTests = int.Parse(args[argscount++]);
            int nSteps = int.Parse(args[argscount++]);
            int minKmerLength = int.Parse(args[argscount++]);
            int maxKmerLength = int.Parse(args[argscount++]);
            double step = double.Parse(args[argscount++]);

            int recovery = int.Parse(args[argscount++]);
            int decoder = int.Parse(args[argscount++]);
            int initial = int.Parse(args[argscount++]);
            var c = (recovery, decoder, initial);

            int nthreads = int.Parse(args[argscount++]);

            double startmul = double.Parse(args[argscount++]);
            double endmul = double.Parse(args[argscount++]);
            double stepmul = double.Parse(args[argscount++]);

            var result = TestDifferentKMerLengthsMul((int)minKmerLength, (int)maxKmerLength, step, nSteps, nTests, (int)tableSize, hashFunctionTypes,c, startmul, endmul, stepmul, nthreads);
            File.WriteAllText(answerFile, String.Join("\n", result.Select(x => $"{x.Item1} {x.Item2} {x.Item3} {x.Item4}")));
            return;
        }

        //END ARGSS


        //int nHashFunctions = 3;



        //if (args[1] == "generated-data") { }S

        StringTestConfig config;
        config = new StringTestConfig(1, 1.5, 0.01, 1, 31, 1000, typeof(KMerStringFactory), typeof(CanonicalOrder), new TabulationFamily());
        //config = config with { HfFamily = new LinearCongruenceFamily() };

        config = config with { HfFamily = new TabulationFamily() };
        Write("\\Tests\\WARMUP.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        //config = config with { TestsInBattery = 500, Step = 0.01, Start = 1.0, End = 1.35 };
        //config = config with { HfFamily = new MultiplyShiftFamily() };
        //Write("\\Tests\\KMerRetrieval0mul.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        ////Write("\\Tests\\KMerRetrieval0lin.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));


        ////config = config with { pipeline = typeof(None) };

        ////Write("\\Tests\\KMerRetrieval1mulNone.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        ////config = config with { HfFamily = new LinearCongruenceFamily() };

        ////Write("\\Tests\\KMerRetrieval1LinNone.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        //config = config with { HfFamily = new TabulationFamily() };
        //config = config with { pipeline = typeof(CanonicalOrder), TestsInBattery = 1, Start = 1.42, End = 1.6, Step = 0.01, StringLenght = 5 };

        //config = config with { StringLenght = 5 };
        ////Write("\\Tests\\KMerRetrieval5tab3.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        ////config = config with { StringLenght = 10 };
        ////Write("\\Tests\\KMerRetrieval10tab1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        ////config = config with { StringLenght = 100 };
        ////Write("\\Tests\\KMerRetrieval100tab1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        //config = config with { pipeline = typeof(CanonicalOrder), TestsInBattery = 1, Start = 1, End = 4, Step = 0.01, StringLenght = 5 };

        //config = config with { StringLenght = 30 };
        //Write("\\Tests\\KMerRetrieval1000tab4.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        ////config = config with { StringLenght = 10000 };
        ////Write("\\Tests\\KMerRetrieval10000tab1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));


        ////config = config with { HfFamily = new LinearCongruenceFamily() };

        ////config = config with { StringLenght = 5 };
        ////Write("\\Tests\\KMerRetrieval5lin1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        ////config = config with { StringLenght = 10 };
        ////Write("\\Tests\\KMerRetrieval10lin1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        ////config = config with { StringLenght = 100 };
        ////Write("\\Tests\\KMerRetrieval100lin1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));


        ////config = config with { StringLenght = 1000 };
        ////Write("\\Tests\\KMerRetrieval1000lin1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        ////config = config with { StringLenght = 10000 };
        ////Write("\\Tests\\KMerRetrieval10000lin1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));


        ////config = config with { pipeline = typeof(None) };
        ////config = config with { HfFamily = new TabulationFamily() };


        ////config = config with { StringLenght = 5 };
        ////Write("\\Tests\\KMerRetrieval5tabNone1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        ////config = config with { StringLenght = 10 };
        ////Write("\\Tests\\KMerRetrieval10tabNone1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        ////config = config with { StringLenght = 100 };
        ////Write("\\Tests\\KMerRetrieval100tabNone1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));


        ////config = config with { StringLenght = 1000 };
        ////Write("\\Tests\\KMerRetrieval1000tabNone1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        ////config = config with { StringLenght = 10000 };
        ////Write("\\Tests\\KMerRetrieval10000tabNone1.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));



        //config = config with { StringLenght = 10000, stringFactory = typeof(NumberStringFactory), pipeline = typeof(None), End = 2 };
        //config = config with { HfFamily = new MultiplyShiftFamily() };


        //Write("\\Tests\\KMerRetrievalStringTab.txt", config.Run(Tests.BasicRetrievalTests.TestMassagers));

        ////Write("\\Tests\\KMerRetrievalStringTab.txt", config.Run(Tests.BasicRetrievalTests.TestMassagersWeaker));


        //BasicTestConfig confBase = new BasicTestConfig(0.72, 0.85, 0.001, 100, 10000, new PolynomialFamily(3));




        ////confBase = confBase with { HfFamily = new TabulationFamily(), Start = 0.1, Step = 0.1, TestsInBattery = 1 };


        ////////Warm up
        //////	Write("\\Tests\\WARMUP.txt", (confBase with { End = 0.73 }).Run(Tests.BasicRetrievalTests.TestRetrieval));








        //////confBase = confBase with { HfFamily = new MultiplyShiftFamily() };
        //////Write("\\Tests\\BaseRetrievalIBLTmul3.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrievalIBLT));
        //////Write("\\Tests\\BaseRetrievalHWPmul3.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrieval));

        ////confBase = confBase with { HfFamily = new LinearCongruenceFamily() };
        ////Write("\\Tests\\BaseRetrievalIBLTlin3.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrievalIBLT));
        ////Write("\\Tests\\BaseRetrievalHWPlin3.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrieval));

        ////confBase = confBase with { HfFamily = new TabulationFamily() };


        ////Write("\\Tests\\BaseRetrievalHPWTab3.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrieval));
        ////Write("\\Tests\\BaseRetrievalIBLTTab3.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrievalIBLT));


        ////confBase = confBase with { HfFamily = new PolynomialFamily(2) };


        ////Write("\\Tests\\BaseRetrievalHPWpol2.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrieval));
        ////Write("\\Tests\\BaseRetrievalIBLTpol2.txt", confBase.Run(Tests.BasicRetrievalTests.TestRetrievalIBLT));

    }
}
