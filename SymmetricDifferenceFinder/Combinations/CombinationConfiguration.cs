using SymmetricDifferenceFinder.RetrievalTesting.SimpleTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using HashingFunctions = System.Collections.Generic.IEnumerable<System.Linq.Expressions.Expression<System.Func<ulong, ulong>>>;


namespace SymmetricDifferenceFinder.Combinations
{
	public class CombinationConfiguration<TTable, TSketch>
		where TSketch : ISketch<TSketch>
		where TTable : ITable

	{
		public Func<int, TTable>? TableFactory;
		public Func<int, HashingFunctions>? HashingFunctionFactory;
		public Func<HashingFunctions, IDecoderFactory<TSketch>>? DecoderFactoryFactory;
		public Func<int, ulong[]>? DataFactory;
		public Func<TTable, TSketch>? TableToSketch;

		public CombinationConfiguration<TTable, TSketch> SetTableFactory(Func<int, TTable> tableFactory)
		{
			TableFactory = tableFactory;
			return this;
		}

		public CombinationConfiguration<TTable, TSketch> SetHashingFunctionFactory(Func<int, HashingFunctions> hashingFunctionFactory)
		{
			HashingFunctionFactory = hashingFunctionFactory;
			return this;
		}

		public CombinationConfiguration<TTable, TSketch> SetDecoderFactoryFactory
			(Func<HashingFunctions, IDecoderFactory<TSketch>> decoderFactoryFactory)
		{
			DecoderFactoryFactory = decoderFactoryFactory;
			return this;
		}

		public CombinationConfiguration<TTable, TSketch> SetTableToSketch(Func<TTable, TSketch> tableToSketch)
		{
			TableToSketch = tableToSketch;
			return this;
		}

	}
}
