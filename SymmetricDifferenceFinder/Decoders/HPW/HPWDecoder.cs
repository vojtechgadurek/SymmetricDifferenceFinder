using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SymmetricDifferenceFinder.Decoders.Common;
using SymmetricDifferenceFinder.Tables;
using HashingFunctions = System.Collections.Generic.IEnumerable<System.Linq.Expressions.Expression<System.Func<ulong, ulong>>>;


namespace SymmetricDifferenceFinder.Decoders.HPW
{


	public class HPWDecoderFactory<TSketch> : IDecoderFactory<TSketch>
		where TSketch : IHPWSketch<TSketch>
	{
		public readonly Action<Key[], int, HashSet<ulong>, HashSet<ulong>, TSketch> OneDecodeStep;
		public readonly Action<TSketch, int, HashSet<ulong>> InitDecoding;

		public HPWDecoderFactory(HashingFunctions hashingFunctions)
		{
			//CreateCallToggle
			var toggleFunctionScheme = CompiledActions.Create<ulong, ulong, TSketch>(out var key, out var value, out var sketch);
			toggleFunctionScheme.S.AddExpression(sketch.V.Call<NoneType>("Toggle", key.V, value.V));
			var toggle = toggleFunctionScheme.Construct();

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
				looksPure = (Expression<Func<ulong, TSketch, bool>>)getLookPureMethodInfo.Invoke(null, new object[] { hashingFunctions })!;
			}
			catch
			{
				throw getLookPureException();
			}


			var addIfLooksPure = DecodingHelperFunctions.GetAddIfLooksPure<HashSet<ulong>, TSketch>(looksPure);
			var oneDecodeStep = HPWMainDecodingLoop.GetOneDecodingStep(hashingFunctions, toggle, looksPure, addIfLooksPure);
			OneDecodeStep = oneDecodeStep.Compile();
			InitDecoding = DecodingHelperFunctions.GetInitialize(addIfLooksPure).Compile();
		}

		public HPWDecoder<TSketch> Create(TSketch sketch)
		{
			return new HPWDecoder<TSketch>(this, sketch);
		}

		IDecoder IDecoderFactory<TSketch>.Create(TSketch sketch)
		{
			return Create(sketch);
		}
	}


	public class HPWDecoder<TSketch> : IDecoder
		where TSketch : IHPWSketch<TSketch>
	{
		readonly HashSet<Key> _decodedValues = new HashSet<Key>();
		readonly TSketch _sketch;
		HashSet<Key> _pure = new HashSet<Key>();
		HashSet<Key> _pureNext = new HashSet<Key>();

		public readonly Action<Key[], int, HashSet<ulong>, HashSet<ulong>, TSketch> OneDecodeStep;
		public readonly Action<TSketch, int, HashSet<ulong>> InitDecoding;
		public readonly int Size;
		int _iteration = 0;

		public DecodingState DecodingState { get; private set; }
		readonly ulong[] _pureBuffer;


		public HPWDecoder(HPWDecoderFactory<TSketch> factory, TSketch sketch)
		{
			OneDecodeStep = factory.OneDecodeStep;
			InitDecoding = factory.InitDecoding;
			_sketch = sketch;
			Size = sketch.Size();
			_pureBuffer = new ulong[Size];
			DecodingState = DecodingState.NotStarted;
		}


		public HashSet<Key> GetDecodedValues()
		{
			return _decodedValues;
		}
		public void Decode()
		{
			InitDecoding(_sketch, Size, _pure);

			while (_iteration < Size * 4)
			{


				OneDecodeStep(_pure.ToArray(), _pure.Count, _pureNext, _decodedValues, _sketch);

				_pure = _pureNext;
				_pureNext = new HashSet<ulong>(_pure.Count * 2);
				if (_pure.Count == 0)
				{
					if (_sketch.IsEmpty())
					{
						DecodingState = DecodingState.Success;
						return;
					}
					else
					{
						DecodingState = DecodingState.Failed;
						return;
					}
				}
				_iteration++;
			}
			DecodingState = DecodingState.Shutdown;

		}
	}
}