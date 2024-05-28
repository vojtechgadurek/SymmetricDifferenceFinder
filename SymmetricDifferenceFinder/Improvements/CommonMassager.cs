using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements
{
	public interface IStringFactory
	{
		List<ulong> GetPossibleNext(ulong value);
		List<ulong> GetPossibleBefore(ulong value);
	}

	struct KMerStringFactory : IStringFactory
	{
		static int size = 31;
		public List<ulong> GetPossibleBefore(ulong value)
		{
			ulong sizeMask = (1UL << (size * 2)) - 1UL;

			var answer = new List<ulong>();
			for (ulong i = 0; i < 4; i++)
			{
				answer.Add((sizeMask & (value << 2)) | i);
			}
			return answer;
		}

		public List<ulong> GetPossibleNext(ulong value)
		{
			ulong sizeMask = (1UL << (size * 2)) - 1UL;
			var answer = new List<ulong>();
			for (ulong i = 0; i < 4; i++)
			{
				answer.Add((sizeMask & (value >> 2)) | i << (size * 2 - 2));
			}
			return answer;
		}
	}


	public static class StringDataFactory<TStringFactory>
		where TStringFactory : struct, IStringFactory
	{
		public static HashSet<ulong> GetRandomStringData(int nItems, int stringLength)
		{
			Random random = new Random();
			TStringFactory stringFactory = default(TStringFactory);
			HashSet<ulong> data = new HashSet<ulong>();
			for (int i = 0; i < nItems / stringLength; i++)
			{
				ulong value = (ulong)random.NextInt64();
				for (int j = 0; j < stringLength; j++)
				{
					value = stringFactory.GetPossibleNext(value)[0];
					data.Add(value);
				}
			}
			return data;
		}
	}

	struct NumberStringFactory : IStringFactory
	{
		public List<ulong> GetPossibleBefore(ulong value)
		{
			var answer = value - 1;
			if (answer == 0)
			{
				answer = ulong.MaxValue;
			}
			return new() { answer };
		}

		public List<ulong> GetPossibleNext(ulong value)
		{
			var answer = value + 1;
			if (answer == 0)
			{
				answer = 1;
			}
			return new() { answer };
		}
	}

	public class StringSet<TString>
		where TString : struct, IStringFactory
	{
		IStringFactory _stringFactory = default(TString);
		public HashSet<ulong> Values = new();
		HashSet<ulong> _toClear = new();
		readonly public HashSet<ulong> ValuesNotOnPath = new();

		XORShift _random = new XORShift(new Random());

		public IEnumerable<ulong> SelectRandom((ulong success, ulong whole) ratio)
		{
			return
				ValuesNotOnPath
					.SelectMany(_stringFactory.GetPossibleBefore)
					.Where(_ => _random.Next() % ratio.whole < ratio.success)
				.Concat(
					ValuesNotOnPath.SelectMany(_stringFactory.GetPossibleNext)
					.Where(_ => _random.Next() % ratio.whole < ratio.success));
		}

		public void ChangedValues(IEnumerable<ulong> changedValues)
		{
			_toClear.Clear();
			foreach (var value in changedValues)
			{
				foreach (var possibleNext in _stringFactory.GetPossibleNext(value))
				{
					if (Values.Contains(possibleNext))
					{
						_toClear.Add(possibleNext);
					}
				}
				foreach (var possibleNext in _stringFactory.GetPossibleBefore(value))
				{
					if (Values.Contains(possibleNext))
					{
						_toClear.Add(possibleNext);
					}
				}
			}
			foreach (var value in _toClear)
			{
				if (IsValueOnPath(value))
				{
					ValuesNotOnPath.Remove(value);
				}
				else
				{
					ValuesNotOnPath.Add(value);
				}

				;
			}
			_toClear.Clear();
		}

		public bool IsValueOnPath(ulong value)
		{
			var possibleNext = _stringFactory.GetPossibleNext(value);
			if (!possibleNext.Any(x => Values.Contains(x))) return false;
			var possibleBefore = _stringFactory.GetPossibleBefore(value);
			if (!possibleBefore.Any(x => Values.Contains(x))) return false;
			return true;
		}

	}

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
		public StringSet<TStringFactory> _addValues;
		public StringSet<TStringFactory> _removedValues;
		public HyperGraphDecoder<TSketch> _decoder;
		public readonly IEnumerable<HashingFunction> _hfs;

		(ulong, ulong) ProbabilityToSelect = (1, 2);

		public HyperGraphMassager(HyperGraphDecoder<TSketch> decoder, IEnumerable<HashingFunction> hfs)
		{
			_addValues = new();
			_removedValues = new();
			_decoder = decoder;
			_hfs = hfs;

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
				_decoder.OuterDecode(pure, addKeys, new List<ulong>());
				AddAndToPure(pure, valuesToRemove);
				_decoder.OuterDecode(pure, addKeys, new List<ulong>());

				foreach (var item in addKeys)
				{
					_decoder.AddKeys.Add(item);
					_decoder.AddKeys = _decoder.AddKeys.Distinct().ToList();
				}

				count++;
			}


		}

		public void Decode()
		{
			DecodeSimple();
		}

		public void HardDecode()
		{
			_decoder.Decode();
			_addValues.Values.UnionWith(_decoder.AddKeys);
			_removedValues.Values.UnionWith(_decoder.RemoveKeys);
			_addValues.ChangedValues(_decoder.AddKeys);
			_removedValues.ChangedValues(_decoder.RemoveKeys);


			int count = 0;
			while (true)
			{
				if (count % (1000) == 0)
				{
					Console.WriteLine($"{count} {_addValues.Values.Count} {_addValues.ValuesNotOnPath.Count}");
				}
				if (_decoder.DecodingState == DecodingState.Success) break;
				if (count > _decoder.Sketch.Size()) break;
				var valuesToRemove = _addValues.SelectRandom(ProbabilityToSelect).ToList();
				var valuesToAdd = _removedValues.SelectRandom(ProbabilityToSelect).ToList();

				var listPure = new ListQueue<ulong>();
				AddAndToPure(listPure, valuesToAdd);
				RemoveAndToPure(listPure, valuesToRemove);

				List<ulong> addedValues = new();
				List<ulong> removedValues = new();

				valuesToRemove.Sort();


				_decoder.OuterDecode(listPure, addedValues, removedValues);
				AddAndToPure(listPure, valuesToRemove);
				RemoveAndToPure(listPure, valuesToAdd);
				_decoder.OuterDecode(listPure, addedValues, removedValues);

				_addValues.Values.UnionWith(addedValues);
				var toRemove = _addValues.Values.Intersect(removedValues).ToList();
				_addValues.Values.ExceptWith(toRemove);


				_addValues.ChangedValues(addedValues.Concat(toRemove).ToList());


				_removedValues.Values.UnionWith(removedValues);
				_removedValues.ChangedValues(removedValues);


				count++;
				if (_addValues.ValuesNotOnPath.Count == 0 && _removedValues.ValuesNotOnPath.Count == 0) break;
			}

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
