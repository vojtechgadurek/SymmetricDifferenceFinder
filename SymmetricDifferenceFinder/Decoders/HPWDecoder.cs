using SimpleSetSketching.Sketchers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using HashingFunctions = System.Collections.Generic.List<System.Linq.Expressions.Expression<System.Func<ulong, ulong>>>;


namespace SymmetricDifferenceFinder.Decoders
{

	public class HPWDecoderFactory<TSketch>
		where TSketch : ISketch
	{
		public readonly Action<Key[], int, List<ulong>, HashSet<ulong>, TSketch> OneDecodeStep;
		public readonly Action<TSketch, int, List<ulong>> InitDecoding;

		public HPWDecoderFactory(HashingFunctions hashingFunctions, Expression<Action<ulong, ulong, TSketch>> toggle)
		{
			var looksPure = DecodingHelperFunctions.GetLooksPure<TSketch>(hashingFunctions);
			var addIfLooksPure = DecodingHelperFunctions.GetAddIfLooksPure<List<ulong>, TSketch>(looksPure);
			var oneDecodeStep = HPWMainDecodingLoop.GetOneDecodingStep(hashingFunctions, toggle, looksPure, addIfLooksPure);
			OneDecodeStep = oneDecodeStep.Compile();
			InitDecoding = DecodingHelperFunctions.GetInitialize(addIfLooksPure).Compile();
		}

		public HPWDecoder<TSketch> Create(TSketch sketch)
		{
			return new HPWDecoder<TSketch>(this, sketch);
		}
	}


	public class HPWDecoder<TSketch>
		where TSketch : ISketch
	{
		readonly HashSet<Key> _decodedValues = new HashSet<Key>();
		readonly TSketch _sketch;
		List<Key> _pure = new List<Key>();
		List<Key> _pureNext = new List<Key>();

		public readonly Action<Key[], int, List<ulong>, HashSet<ulong>, TSketch> OneDecodeStep;
		public readonly Action<TSketch, int, List<ulong>> InitDecoding;
		public readonly int Size;
		int _iteration = 0;

		public DecodingState State { get; private set; }
		readonly ulong[] _pureBuffer;

		public enum DecodingState
		{
			Shotdown,
			Success,
			Failed,
		}

		public HPWDecoder(HPWDecoderFactory<TSketch> factory, TSketch sketch)
		{
			OneDecodeStep = factory.OneDecodeStep;
			InitDecoding = factory.InitDecoding;
			_sketch = sketch;
			Size = sketch.Size;
			_pureBuffer = new ulong[Size];
		}

		public void Decode()
		{
			InitDecoding(_sketch, Size, _pure);

			while (_iteration < Size * 4)
			{

				_pure.CopyTo(_pureBuffer, 0);

				OneDecodeStep(_pureBuffer, _pure.Count, _pureNext, _decodedValues, _sketch);

				_pure = _pureNext;
				_pureNext = new List<ulong>(_pure.Count * 2);
				if (_pure.Count == 0)
				{
					if (_sketch.IsEmpty())
					{
						State = DecodingState.Success;
						return;
					}
					else
					{
						State = DecodingState.Failed;
						return;
					}
				}
				_iteration++;
			}
			State = DecodingState.Shotdown;

		}
	}
}