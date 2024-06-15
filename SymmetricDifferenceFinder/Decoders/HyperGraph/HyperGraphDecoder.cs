using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsWPF;
using SymmetricDifferenceFinder.Decoders.Common;
using SymmetricDifferenceFinder.Utils;

namespace SymmetricDifferenceFinder.Decoders.HyperGraph
{
	public class HyperGraphDecoderFactory<TSketch> : IDecoderFactory<TSketch>
		where TSketch : IHyperGraphDecoderSketch<TSketch>
	{
		readonly public Action<TSketch, int, ListQueue<ulong>> Initialize;
		//readonly public Action<TSketch, ListQueue<ulong>, List<ulong>, List<ulong>> Decode;
		readonly public Action<TSketch, ListQueue<ulong>, List<ulong>, List<ulong>, int> DecodeWithLimitedNumberOfSteps;
		readonly public IReadOnlyList<Expression<HashingFunction>> HashingFunctions;
		readonly public Func<ulong, TSketch, bool> looksPure;


		public HyperGraphDecoderFactory(IEnumerable<Expression<HashingFunction>> hashingFunctions, bool allowNegativeCounts = true)
		{
			//Find GetLooksPure for TSketch
			var getLookPureMethodInfo = typeof(TSketch).GetMethod("GetLooksPure");
			var getLookPureException = () => new InvalidOperationException(
					$"{nameof(TSketch)} does not define public static Expression<Func<ulong, {nameof(TSketch)}, bool>>" +
					$"GetLooksPure(IEnumerable<Expression<Func<ulong, ulong>>>)");

			this.HashingFunctions = hashingFunctions.ToList();

			if (getLookPureMethodInfo is null)
				throw getLookPureException();

			Expression<Func<ulong, TSketch, bool>> looksPureEx;
			try
			{
				looksPureEx =
					(Expression<Func<ulong, TSketch, bool>>)getLookPureMethodInfo.Invoke(null, new object[] { hashingFunctions })!;
			}
			catch
			{
				throw getLookPureException();
			}


			var decodingMethod = allowNegativeCounts ? HyperGraphDecoderMainLoop.GetRemoveAndAddToPure<TSketch>(hashingFunctions) : HyperGraphDecoderMainLoop.GetOnlyRemoveAddToPure<TSketch>(hashingFunctions);
			//Decode = HyperGraphDecoderMainLoop.GetDecode(
			//	looksPure, decodingMethod
			//	).Compile();
			DecodeWithLimitedNumberOfSteps = HyperGraphDecoderMainLoop.GetDecodedLimitedSteps(
								looksPureEx, decodingMethod
												).Compile();
			Initialize = DecodingHelperFunctions.GetInitialize(
				DecodingHelperFunctions.GetAddIfLooksPure<ListQueue<ulong>, TSketch>(looksPureEx)
				).Compile();
			looksPure = looksPureEx.Compile();
		}

		public HyperGraphDecoder<TSketch> Create(TSketch sketch)
		{
			return new HyperGraphDecoder<TSketch>(Initialize, DecodeWithLimitedNumberOfSteps, looksPure, HashingFunctions.Count(), sketch);
		}

		IDecoder IDecoderFactory<TSketch>.Create(TSketch sketch)
		{
			return Create(sketch);
		}
	}

	public class HyperGraphDecoder<TSketch> : IDecoder where TSketch : IHyperGraphDecoderSketch<TSketch>
	{
		readonly public TSketch Sketch;
		readonly public Action<TSketch, int, ListQueue<ulong>> _initialize;
		readonly public Action<TSketch, ListQueue<ulong>, List<ulong>, List<ulong>, int> _decode;
		public List<ulong> AddKeys;
		public readonly List<ulong> RemoveKeys;
		HashSet<ulong> _decodedKeys;
		readonly public Func<ulong, TSketch, bool> LooksPure;
		int nHashFunctions;

		public HyperGraphDecoder(
			Action<TSketch, int, ListQueue<ulong>> initialize,
			Action<TSketch, ListQueue<ulong>, List<ulong>, List<ulong>, int> decode,
			Func<ulong, TSketch, bool> looksPure,
			int numberOfHashingFunctions,
			TSketch sketch)
		{
			_initialize = initialize;
			_decode = decode;
			Sketch = sketch;
			AddKeys = new List<ulong>(Sketch.Size());
			RemoveKeys = new List<ulong>(Sketch.Size());
			nHashFunctions = numberOfHashingFunctions;
			LooksPure = looksPure;

		}
		DecodingState _state = DecodingState.NotStarted;
		public DecodingState DecodingState => _state;
		public int IteratorMultiplicator = 2;

		public void Decode()
		{
			ListQueue<ulong> pure = new ListQueue<ulong>();
			//This expects decoding is not better than 2 time the Size of the sketch
			int numberOfSteps = nHashFunctions * Sketch.Size() * IteratorMultiplicator;
			_initialize(Sketch, Sketch.Size(), pure);
			_decode(Sketch, pure, AddKeys, RemoveKeys, numberOfSteps);
			_decodedKeys = new HashSet<ulong>(AddKeys);
			_decodedKeys.SymmetricExceptWith(RemoveKeys);
			if (Sketch.IsEmpty()) _state = DecodingState.Success;
			else _state = DecodingState.Failed;
		}

		public void OuterDecode(ListQueue<ulong> pure, List<ulong> addKeys, List<ulong> removeKeys, int numberOfSteps)
		{
			_decode(Sketch, pure, addKeys, removeKeys, numberOfSteps);
			if (Sketch.IsEmpty()) _state = DecodingState.Success;
			else _state = DecodingState.Failed;
		}


		public HashSet<ulong> GetDecodedValues()
		{
			return new HashSet<ulong>(_decodedKeys);
		}
	}

}
