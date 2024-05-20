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
		readonly public Action<TSketch, ListQueue<ulong>, List<ulong>, List<ulong>> Decode;
		public HyperGraphDecoderFactory(IEnumerable<Expression<HashingFunction>> hashingFunctions)
		{
			//Find GetLooksPure for TSketch
			var getLookPureMethodInfo = typeof(TSketch).GetMethod("GetLooksPure");
			var getLookPureException = () => new InvalidOperationException(
					$"{nameof(TSketch)} does not define public static Expression<Func<ulong, {nameof(TSketch)}, bool>>" +
					$"GetLooksPure(IEnumerable<Expression<Func<ulong, ulong>>>)");

			if (getLookPureMethodInfo is null)
				throw getLookPureException();

			Expression<Func<ulong, TSketch, bool>> looksPure;
			try
			{
				looksPure =
					(Expression<Func<ulong, TSketch, bool>>)getLookPureMethodInfo.Invoke(null, new object[] { hashingFunctions })!;
			}
			catch
			{
				throw getLookPureException();
			}


			Decode = HyperGraphDecoderMainLoop.GetDecode(
				looksPure, HyperGraphDecoderMainLoop.GetOnlyRemoveAddToPure<TSketch>(hashingFunctions)
				).Compile();

			Initialize = DecodingHelperFunctions.GetInitialize(
				DecodingHelperFunctions.GetAddIfLooksPure<ListQueue<ulong>, TSketch>(looksPure)
				).Compile();
		}

		public HyperGraphDecoder<TSketch> Create(TSketch sketch)
		{
			return new HyperGraphDecoder<TSketch>(Initialize, Decode, sketch);
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
		readonly public Action<TSketch, ListQueue<ulong>, List<ulong>, List<ulong>> _decode;
		public List<ulong> AddKeys;
		public readonly List<ulong> RemoveKeys;
		HashSet<ulong> _decodedKeys;

		public HyperGraphDecoder(
			Action<TSketch, int, ListQueue<ulong>> initialize,
			Action<TSketch, ListQueue<ulong>, List<ulong>, List<ulong>> decode,
			TSketch sketch)
		{
			_initialize = initialize;
			_decode = decode;
			Sketch = sketch;
			AddKeys = new List<ulong>(Sketch.Size());
			RemoveKeys = new List<ulong>(Sketch.Size());

		}

		DecodingState _state = DecodingState.NotStarted;
		public DecodingState DecodingState => _state;

		public void Decode()
		{
			ListQueue<ulong> pure = new ListQueue<ulong>();
			_initialize(Sketch, Sketch.Size(), pure);
			_decode(Sketch, pure, AddKeys, RemoveKeys);
			_decodedKeys = new HashSet<ulong>(AddKeys);
			_decodedKeys.SymmetricExceptWith(RemoveKeys);
			if (Sketch.IsEmpty()) _state = DecodingState.Success;
			else _state = DecodingState.Failed;
		}

		public void OuterDecode(ListQueue<ulong> pure, List<ulong> addKeys, List<ulong> removeKeys)
		{
			_decode(Sketch, pure, addKeys, removeKeys);
			if (Sketch.IsEmpty()) _state = DecodingState.Success;
			else _state = DecodingState.Failed;
		}


		public HashSet<ulong> GetDecodedValues()
		{
			return new HashSet<ulong>(_decodedKeys);
		}
	}

}
