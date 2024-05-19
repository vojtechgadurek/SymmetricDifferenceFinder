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
				looksPure, HyperGraphDecoderMainLoop.GetRemoveAndAddToPure<TSketch>(hashingFunctions)
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
		TSketch _sketch;
		readonly public Action<TSketch, int, ListQueue<ulong>> _initialize;
		readonly public Action<TSketch, ListQueue<ulong>, List<ulong>, List<ulong>> _decode;
		readonly List<ulong> _addKeys;
		readonly List<ulong> _removeKeys;
		HashSet<ulong> _decodedKeys;

		public HyperGraphDecoder(
			Action<TSketch, int, ListQueue<ulong>> initialize,
			Action<TSketch, ListQueue<ulong>, List<ulong>, List<ulong>> decode,
			TSketch sketch)
		{
			_initialize = initialize;
			_decode = decode;
			_sketch = sketch;
			_addKeys = new List<ulong>(_sketch.Size());
			_removeKeys = new List<ulong>(_sketch.Size());

		}

		DecodingState _state = DecodingState.NotStarted;
		public DecodingState DecodingState => _state;

		public void Decode()
		{
			ListQueue<ulong> pure = new ListQueue<ulong>();
			_initialize(_sketch, _sketch.Size(), pure);
			_decode(_sketch, pure, _addKeys, _removeKeys);
			_decodedKeys = new HashSet<ulong>(_addKeys);
			_decodedKeys.SymmetricExceptWith(_removeKeys);
			_state = DecodingState.Success;
		}

		public HashSet<ulong> GetDecodedValues()
		{
			return new HashSet<ulong>(_decodedKeys);
		}
	}

}
