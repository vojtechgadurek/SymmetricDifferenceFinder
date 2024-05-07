using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SymmetricDifferenceFinder.Decoders.Common;

namespace SymmetricDifferenceFinder.Decoders.HyperGraph
{
	public interface IHypergraphTable<TValue>
	{
		TValue Get(int index);
		int Count { get; }
		int NumberOfHashedItems(int index);
	}
	public class HyperGraphDecoder<TTable> : IDecoder where TTable : IHypergraphTable<ulong>
	{
		TTable table;
		readonly public Expression<Func<int, TTable, bool>> IsPure;
		readonly public Expression<Action<ulong, TTable>> Remove;
		readonly public Expression<Action<ulong, TTable>> Add;

		readonly public Expression<Action<int, TTable, List<int>>> RemoveAndAddIfPure;

		public DecodingState State => DecodingState.NotStarted;

		public DecodingState DecodingState => throw new NotImplementedException();

		public HyperGraphDecoder()
		{


		}



		public void Decode()
		{


		}

		public HashSet<ulong> GetDecodedValues()
		{
			throw new NotImplementedException();
		}
	}

}
