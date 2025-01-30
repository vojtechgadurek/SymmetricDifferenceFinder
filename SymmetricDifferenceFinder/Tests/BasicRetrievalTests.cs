using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Diagnostics.Tracing;
using RedaFastaTest;
using SymmetricDifferenceFinder.Improvements;
using SymmetricDifferenceFinder.RetrievalTesting.BatteryTests;
using SymmetricDifferenceFinder.RetrievalTesting.SimpleTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
            var factory = new RetrievalTestFactory<IBLTTable, IBLTTable>(test, hashingFunction, (int x) => RandomDataFactory.GetRandomKMerData(x, 31).ToArray());

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

        public static void TestIBLT3()
        {
            var test = Combinations.Combinations.IBLT();
            var hashingFunction = HashingFunctionCombinations.GetFromSameFamily(3, new LinearCongruenceFamily()).GetFactory();

            int size = 10000;
            var batteryTest = new BatteryTest(0.01, 1, 0.01, size);
            var factory = new RetrievalTestFactory<IBLTTable, IBLTTable>(test, hashingFunction, (int x) => RandomDataFactory.GetRandomData(x).ToArray());

            var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), 100);

            foreach (var x in answer)
            {
                Console.WriteLine(x.ToString());
            }
        }


        public static void TestIBLTMassager()
        {
            var test = Combinations.Combinations.IBLT();

            var decoderFactoryFactory = test.DecoderFactoryFactory;

            test.SetDecoderFactoryFactory((hfs) => new HyperGraphMassagerFactory<NumberStringFactory, IBLTTable>(
                hfs,
                (HyperGraphDecoderFactory<IBLTTable>)decoderFactoryFactory(hfs)));

            var hashingFunction = HashingFunctionCombinations.GetFromSameFamily(3, new LinearCongruenceFamily()).GetNoConflictFactory();

            int size = 10000;
            var batteryTest = new BatteryTest(0.9, 30, 0.05, size);

            var factory = new RetrievalTestFactory<IBLTTable, IBLTTable>(test, hashingFunction, (int x) => StringDataFactory<NumberStringFactory, None>.GetRandomStringData(x, 30).ToArray());

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


        public static IEnumerable<BatteryDecodingResult> TestMassagers(double start, double end, double step, int testsInBattery, int lengthKMer, int size, IHashFunctionFamily hfFamily, Type stringFactory, Type pipeline)
        {
            return (IEnumerable<BatteryDecodingResult>)typeof(BasicRetrievalTests).GetMethod(nameof(TestMassagersGeneric))!.MakeGenericMethod([stringFactory, pipeline]).Invoke(null, new object[] { start, end, step, testsInBattery, lengthKMer, size, hfFamily })!;
        }

        public static IEnumerable<BatteryDecodingResult> TestMassagersGeneric<TStringFactory, TPipeline>(double start, double end, double step, int testsInBattery, int lengthKMer, int size, IHashFunctionFamily hfFamily)
        where TStringFactory : struct, IStringFactory
        where TPipeline : struct, IPipeline
        {
            var test = Combinations.Combinations.HPW();

            Func<int, IEnumerable<Expression<HashingFunction>>> hf = (int size) =>
            {
                var result = new List<Expression<HashingFunction>>();
                var counter = 0;
                var hfSize = 0;

                hfSize = size;
                result.Add(hfFamily.GetScheme((ulong)hfSize, (ulong)counter).Create());
                result.Add(hfFamily.GetScheme((ulong)hfSize, (ulong)counter).Create());
                result.Add(hfFamily.GetScheme((ulong)hfSize, (ulong)counter).Create());

                return result;
            };

            var hashingFunction = hf;//HashingFunctionCombinations.GetFromSameFamily(3, hfFamily).GetFactory();


            var decoderFactory = test.DecoderFactoryFactory;
            Random random = new Random(2024_1);


            test.SetDecoderFactoryFactory((hfs) => new MassagerFactory<TStringFactory, TPipeline>(
                hfs,
                (HPWDecoderFactory<XORTable>)decoderFactory(hfs)));

            var batteryTest = new BatteryTest(start, end, step, size);


            var factory = new RetrievalTestFactory<XORTable, XORTable>(test, hashingFunction, (x) => StringDataFactory<KMerStringFactory, CanonicalOrder>.GetRandomStringData(x, lengthKMer).ToArray());

            var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), testsInBattery);

            return answer;
        }

        public static IEnumerable<BatteryDecodingResult> TestMassagersWeaker(double start, double end, double step, int testsInBattery, int lengthKMer, int size, IHashFunctionFamily hfFamily, Type stringFactory, Type pipeline)
        {
            return (IEnumerable<BatteryDecodingResult>)typeof(BasicRetrievalTests).GetMethod(nameof(TestMassagersGeneric))!.MakeGenericMethod([stringFactory, pipeline]).Invoke(null, new object[] { start, end, step, testsInBattery, lengthKMer, size, hfFamily })!;
        }

        public static IEnumerable<BatteryDecodingResult> TestMassagersGenericWeaker<TStringFactory, TPipeline>(double start, double end, double step, int testsInBattery, int lengthKMer, int size, IHashFunctionFamily hfFamily)
        where TStringFactory : struct, IStringFactory
        where TPipeline : struct, IPipeline
        {
            var test = Combinations.Combinations.HPW();
            var hashingFunction = HashingFunctionCombinations.GetFromSameFamilyLastWeaker(4, hfFamily).GetNoConflictFactory();


            var decoderFactory = test.DecoderFactoryFactory;
            Random random = new Random(2024_1);


            test.SetDecoderFactoryFactory((hfs) => new MassagerFactory<TStringFactory, TPipeline>(
                hfs,
                (HPWDecoderFactory<XORTable>)decoderFactory(hfs)));

            var batteryTest = new BatteryTest(start, end, step, size);
            var factory = new RetrievalTestFactory<XORTable, XORTable>(test, hashingFunction, (int x) => StringDataFactory<TStringFactory, TPipeline>.GetRandomStringData(x, lengthKMer).ToArray());

            var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), testsInBattery);

            return answer;
        }

        public static IEnumerable<BatteryDecodingResult> TestRetrievalIBLT(double start, double end, double step, int testsInBattery, int size, IHashFunctionFamily hfFamily)
        {
            var test = Combinations.Combinations.IBLT();
            var hashingFunction = HashingFunctionCombinations.GetFromSameFamily(3, hfFamily).GetFactory();


            var decoderFactory = test.DecoderFactoryFactory;
            Random random = new Random(2024_1);


            test.SetDecoderFactoryFactory((hfs) =>
                (HyperGraphDecoderFactory<IBLTTable>)decoderFactory(hfs));

            var batteryTest = new BatteryTest(start, end, step, size);
            var factory = new RetrievalTestFactory<IBLTTable, IBLTTable>(test, hashingFunction, (int x) => StringDataFactory<KMerStringFactory, None>.GetRandomStringData(x, 31).ToArray());
            var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), testsInBattery);
            return answer;
        }


        public static IEnumerable<BatteryDecodingResult> TestRetrieval(double start, double end, double step, int testsInBattery, int size, IHashFunctionFamily hfFamily)
        {
            var test = Combinations.Combinations.HPW();
            var hashingFunction = HashingFunctionCombinations.GetFromSameFamily(3, hfFamily).GetFactory();


            var decoderFactory = test.DecoderFactoryFactory;
            Random random = new Random(2024_1);


            test.SetDecoderFactoryFactory((hfs) =>
                (HPWDecoderFactory<XORTable>)decoderFactory(hfs));

            var batteryTest = new BatteryTest(start, end, step, size);
            var factory = new RetrievalTestFactory<XORTable, XORTable>(test, hashingFunction, (int x) => StringDataFactory<KMerStringFactory, None>.GetRandomStringData(x, 31).ToArray());
            var answer = batteryTest.Run((numberItems) => factory.Get(size).Run(numberItems), testsInBattery);
            return answer;
        }


        public static void TestMassagersConflict()
        {
            var test = Combinations.Combinations.HPW();
            var hashingFunction = HashingFunctionCombinations.GetFromSameFamily(3, new LinearCongruenceFamily()).GetNoConflictFactory();


            var decoderFactory = test.DecoderFactoryFactory;
            Random random = new Random();


            test.SetDecoderFactoryFactory((hfs) => new MassagerFactory<NumberStringFactory, CanonicalOrder>(
                hfs,
                (HPWDecoderFactory<XORTable>)decoderFactory(hfs)));

            int size = 10000;
            var batteryTest = new BatteryTest(0.7, 0.8, 0.02, size);
            var factory = new RetrievalTestFactory<XORTable, XORTable>(test, hashingFunction, (int x) => RandomDataFactory.GetRandomKMerData(x, 31).ToArray());

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
