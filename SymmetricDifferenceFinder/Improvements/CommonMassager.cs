using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements
{
	public class HyperGraphMassagerFactory<TStringFactory, TSketch> : IDecoderFactory<TSketch>
		where TSketch : struct, IHyperGraphDecoderSketch<TSketch>
		where TStringFactory : struct, IStringFactory
	{
		HyperGraphDecoderFactory<TSketch> _decoderFactory;
		IEnumerable<HashingFunction> _hfs;

		public HyperGraphMassagerFactory(IEnumerable<Expression<HashingFunction>> hfs, HyperGraphDecoderFactory<TSketch> decoderFactory)
		{
			_decoderFactory = decoderFactory;
			_hfs = hfs.Select(hf => hf.Compile()).ToList();
		}

		public IDecoder Create(TSketch sketch)
		{
			return new HyperGraphMassager<TStringFactory, TSketch>(_decoderFactory.Create(sketch), _hfs);
		}
	}


	public class HyperGraphMassager<TStringFactory, TSketch> : IDecoder
		where TStringFactory : struct, IStringFactory
		where TSketch : struct, IHyperGraphDecoderSketch<TSketch>
	{

		public DecodingState DecodingState => DecodingState.NotStarted;
		public HyperGraphDecoder<TSketch> _decoder;
		public HyperGraph _hyperGraph;
		public BinPackingAttackFinder<Cache<NextOracle<TStringFactory>>> _nextAttackVectorFinder;
		public BinPackingAttackFinder<Cache<BeforeOracle<TStringFactory>>> _beforeAttackVectorFinder;
		public readonly List<HashingFunction> _hfs;

		(ulong, ulong) ProbabilityToSelect = (1, 2);

		public HyperGraphMassager(HyperGraphDecoder<TSketch> decoder, IEnumerable<HashingFunction> hfs)
		{
			_decoder = decoder;
			_hfs = hfs.ToList();
			_hyperGraph = new HyperGraph((ulong)_decoder.Sketch.Size());
			_nextAttackVectorFinder = new(_hfs, _hyperGraph);
			_beforeAttackVectorFinder = new(_hfs, _hyperGraph);
		}

		public void RemoveAndToPure(ListQueue<ulong> pure, List<ulong> values)
		{
			foreach (var hf in _hfs)
			{
				foreach (var value in values)
				{
					var hash = hf(value);
					_decoder.Sketch.Remove(hash, value);
					if (_decoder.Sketch.GetCount(hash) == 1)
					{
						pure.Add(hash);
					}
				}
			}
		}


		public void RemoveAndToPure(ListQueue<ulong> pure, ulong value)
		{
			foreach (var hf in _hfs)
			{
				var hash = hf(value);
				_decoder.Sketch.Remove(hash, value);
				_nextAttackVectorFinder.AddNode(value);
				_beforeAttackVectorFinder.AddNode(value);
				if (_decoder.LooksPure(hash, _decoder.Sketch))
				{
					pure.Add(hash);
				}
			}
		}


		public void AddAndToPure(ListQueue<ulong> pure, List<ulong> values)
		{
			foreach (var hf in _hfs)
			{
				foreach (var value in values)
				{
					var hash = hf(value);
					_decoder.Sketch.Add(hash, value);
					if (_decoder.Sketch.GetCount(hash) == 1)
					{
						pure.Add(hash);
					}
				}
			}
		}

		public void DecodeSimple()
		{
			_decoder.Decode();
			int count = 0;
			Random random = new Random();
			while (true)
			{

				if (_decoder.DecodingState == DecodingState.Success) break;
				if (count > _decoder.Sketch.Size()) break;
				if (count % (1000) == 0)
				{
					Console.WriteLine($"{count} {_decoder.AddKeys.Count}");
				}

				TStringFactory factory = default(TStringFactory);
				var valuesToRemove = _decoder.AddKeys.Where(_ => random.NextDouble() < 0.5).SelectMany(x => factory.GetPossibleNext(x).Concat(factory.GetPossibleBefore(x))).ToList();

				var pure = new ListQueue<ulong>();
				RemoveAndToPure(pure, valuesToRemove);
				List<ulong> addKeys = new List<ulong>();
				_decoder.OuterDecode(pure, addKeys, new List<ulong>(), _decoder.Sketch.Size());
				AddAndToPure(pure, valuesToRemove);
				_decoder.OuterDecode(pure, addKeys, new List<ulong>(), _decoder.Sketch.Size());

				foreach (var item in addKeys)
				{
					_decoder.AddKeys.Add(item);
					_decoder.AddKeys = _decoder.AddKeys.Distinct().ToList();
				}

				count++;
			}


		}



		public void BinPackingDecode()
		{
			_decoder.Decode();

			foreach (var node in _decoder.RemoveKeys)
			{
				_beforeAttackVectorFinder.AddNode(node);
				_nextAttackVectorFinder.AddNode(node);
			}

			for (int round = 0; round < 1000; round++)
			{
				if (_decoder.DecodingState == DecodingState.Success) break;
				ListQueue<ulong> pures = new();
				var sketch = _decoder.Sketch;
				for (ulong i = 0; i < (ulong)_decoder.Sketch.Size(); i++)
				{
					if (sketch.GetCount(i) != 2) continue;

					var bucket = _hyperGraph.GetBucket(i);

					if (bucket.Count == 0) continue;

					foreach (var value in bucket)
					{
						sketch.Remove(i, value);
						bool answer = (_decoder.LooksPure(i, sketch));
						ulong secondValue = sketch.Get(i);
						sketch.Add(i, value);

						if (answer)
						{
							RemoveAndToPure(pures, value);
							RemoveAndToPure(pures, secondValue);
							_decoder.RemoveKeys.Add(value);
							_decoder.RemoveKeys.Add(secondValue);
							break;
						}
					}

				}

				List<ulong> removed = new();
				List<ulong> added = new();
				_decoder.OuterDecode(pures, removed, added, pures.Count() * _hfs.Count());

				foreach (var node in added)
				{
					_beforeAttackVectorFinder.AddNode(node);
					_nextAttackVectorFinder.AddNode(node);
					_decoder.RemoveKeys.Add(node);
				}


				//foreach (var node in removed)
				//{
				//	_beforeAttackVectorFinder.RemoveNode(node);
				//	_nextAttackVectorFinder.RemoveNode(node);
				//	_decoder.AddKeys.Add(node);
				//}

				if (removed.Count == 0 && added.Count == 0)
				{
					break;
				}
			}

		}

		public void Decode()
		{
			BinPackingDecode();
		}


		public HashSet<ulong> GetDecodedValues()
		{
			/*
			var answer = new HashSet<ulong>();
			answer.UnionWith(_addValues.Values);
			answer.SymmetricExceptWith(_removedValues.Values);
			return answer;
			*/
			return _decoder.GetDecodedValues();
		}
	}
}
