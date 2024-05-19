using BenchmarkDotNet.Validators;
using SymmetricDifferenceFinder.Decoders;
using SymmetricDifferenceFinder.Decoders.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HashingFunctions = System.Collections.Generic.IEnumerable<System.Linq.Expressions.Expression<System.Func<ulong, ulong>>>;


namespace SymmetricDifferenceFinder.RetrievalTesting.SimpleTests
{
	public record class DecodingResult
		(
			int Size,
			int NumberItems,
			DecodingState DecodingState,
			int DecodedIncorrectly
		);

	public class RetrievalTestFactory<TTable, TSketch>
		where TTable : ITable
		where TSketch : ISketch<TSketch>
	{
		readonly Func<int, TTable> _tableFactory;
		readonly Func<int, HashingFunctions> _hashingFunctionFactory;
		readonly Func<HashingFunctions, IDecoderFactory<TSketch>> _decoderFactoryFactory;
		readonly Func<int, ulong[]> _dataFactory;
		readonly Func<TTable, TSketch> _tableToSketch;

		public RetrievalTestFactory(
			Func<int, TTable> tableFactory,
			Func<int, HashingFunctions> hashingFunctionFactory,
			Func<HashingFunctions, IDecoderFactory<TSketch>> decoderFactoryFactory,
			Func<int, ulong[]> dataFactory,
			Func<TTable, TSketch> tableToSketch
			)
		{
			_tableFactory = tableFactory;
			_hashingFunctionFactory = hashingFunctionFactory;
			_decoderFactoryFactory = decoderFactoryFactory;
			_dataFactory = dataFactory;
			_tableToSketch = tableToSketch;
		}

		public RetrievalTestFactory(
			CombinationConfiguration<TTable, TSketch> configuration,
			Func<int, HashingFunctions> hashingFunctionFactory,
			Func<int, ulong[]> dataFactory
			)
		{
			_tableFactory = configuration.TableFactory!;
			_hashingFunctionFactory = hashingFunctionFactory;
			_decoderFactoryFactory = configuration.DecoderFactoryFactory!;
			_dataFactory = dataFactory;
			_tableToSketch = configuration.TableToSketch!;
		}

		public RetrievalTest<TTable, TSketch> Get(int size)
		{
			var hfs = _hashingFunctionFactory(size);
			var hfsCompiled = hfs.Select(hf => hf.Compile()).ToList();
			return
				new RetrievalTest<TTable, TSketch>(
				_decoderFactoryFactory(hfs),
				_dataFactory,
				() => _tableFactory(size),
				_tableToSketch,
				hfsCompiled
				);
		}

		public Func<int, DecodingResult> GetFactory(int size)
		{
			return (numberOfItems) => Get(size).Run(numberOfItems);
		}
	}


	public record class RetrievalTest<TTable, TSketch>
		(
		IDecoderFactory<TSketch> DecoderFactory,
		Func<int, ulong[]> RandomDataFactory,
		Func<TTable> TableFactory,
		Func<TTable, TSketch> TableToSketch,
		IEnumerable<HashingFunction> hfs
		)
		where TTable : ITable
		where TSketch : ISketch<TSketch>
	{
		public DecodingResult Run(int numberItems)
		{
			var table = TableFactory();
			var data = RandomDataFactory(numberItems);

			foreach (var hf in hfs)
			{
				foreach (var item in data)
				{
					table.Add(hf(item), item);
				}
			}

			var decoder = DecoderFactory.Create(TableToSketch(table));

			decoder.Decode();

			var decodedValues = decoder.GetDecodedValues();


			decodedValues.SymmetricExceptWith(data);

			return new DecodingResult(data.Length, numberItems, decoder.DecodingState, decodedValues.Count);
		}
	}



}
