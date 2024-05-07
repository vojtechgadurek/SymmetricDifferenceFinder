using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsWPF;
using SymmetricDifferenceFinder.Decoders.Common;

namespace SymmetricDifferenceFinder.Decoders.HyperGraph
{
	public class HyperGraphDecoderFactory<TSketch>
		where TSketch : IHyperGraphDecoderSketch<TSketch>
	{
		readonly public Expression<Func<ulong, TSketch, bool>> IsPure;
		readonly public Action<TSketch, int, List<ulong>> Initialize;
		readonly public Action<TSketch, List<ulong>, List<ulong>> Decode;
		public HyperGraphDecoderFactory(Expression<Func<ulong, TSketch, bool>> isPure, IEnumerable<Expression<HashingFunction>> hashingFunctions)
		{
			IsPure = isPure;
			Decode = HyperGraphDecoderMainLoop.GetDecode(IsPure, HyperGraphDecoderMainLoop.GetRemoveAndAddToPure<TSketch>(hashingFunctions)).Compile();
			Initialize = DecodingHelperFunctions.GetInitialize(DecodingHelperFunctions.GetAddIfLooksPure<List<ulong>, TSketch>(isPure)).Compile();
		}

		public HyperGraphDecoder<TSketch> Create(TSketch sketch)
		{
			return new HyperGraphDecoder<TSketch>(Initialize, Decode, sketch);
		}
	}

	public class HyperGraphDecoder<TSketch> : IDecoder where TSketch : IHyperGraphDecoderSketch<TSketch>
	{
		TSketch _sketch;
		readonly public Action<TSketch, int, List<ulong>> _initialize;
		readonly public Action<TSketch, List<ulong>, List<ulong>> _decode;
		readonly List<ulong> _decodedKeys;

		public HyperGraphDecoder(Action<TSketch, int, List<ulong>> initialize, Action<TSketch, List<ulong>, List<ulong>> decode, TSketch sketch)
		{
			_initialize = initialize;
			_decode = decode;
			_sketch = sketch;
			_decodedKeys = new List<ulong>(_sketch.Size);

		}

		public DecodingState State => DecodingState.NotStarted;

		public DecodingState DecodingState => throw new NotImplementedException();



		public void Decode()
		{
			List<ulong> pure = new List<ulong>();
			_initialize(_sketch, _sketch.Size, pure);
			_decode(_sketch, pure, _decodedKeys);
		}

		public HashSet<ulong> GetDecodedValues()
		{
			return new HashSet<ulong>(_decodedKeys);
		}
	}

}
