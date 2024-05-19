using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftAntimalwareEngine;
using SymmetricDifferenceFinder.Decoders.HPW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SymmetricDifferenceFinder.Improvements
{

	public class MassagerFactory : IDecoderFactory<XORTable>
	{
		HPWDecoderFactory<XORTable> _decoderFactory;
		IEnumerable<HashingFunction> _hfs;
		Func<ulong, ulong> _getCLose;
		public MassagerFactory(IEnumerable<Expression<HashingFunction>> hfs, HPWDecoderFactory<XORTable> factory)
		{
			_decoderFactory = factory;
			_hfs = hfs.Select(f => f.Compile()).ToList();
		}
		public IDecoder Create(XORTable sketch)
		{
			return new Massager(_decoderFactory.Create(sketch), _hfs);
		}
	}

	public class Massager : IDecoder
	{
		HPWDecoder<XORTable> _HPWDecoder;
		XORTable _table;
		Random _random = new Random();
		int _size;
		IEnumerable<Func<ulong, ulong>> _hfs;
		HashSet<ulong> _decodedValues;

		public DecodingState DecodingState => _HPWDecoder.DecodingState;

		public Massager(HPWDecoder<XORTable> HPWDecoder, IEnumerable<HashingFunction> hfs)
		{
			_table = HPWDecoder.Sketch;
			_size = _table.Size();
			_hfs = hfs;

			_HPWDecoder = HPWDecoder;
			_decodedValues = _HPWDecoder.GetDecodedValues();
		}

		HashSet<ulong> ClearSet(HashSet<ulong> set)
		{
			return set.Where(
				x => _decodedValues.Contains(RandomDataFactory.NextInString(x)) && _decodedValues.Contains(RandomDataFactory.BeforeInString(x))
				).ToHashSet();

		}
		public void Decode()
		{
			int nMassages = _size;
			_HPWDecoder.MaxNumberOfIterations = 10;
			int count = 0;

			ulong currentHash = 0;
			HashSet<ulong> valuesPossibleToPick = new();

			_HPWDecoder.Decode();
			valuesPossibleToPick.UnionWith(_decodedValues);

			int nValues = valuesPossibleToPick.Count;

			while (nMassages > count)
			{
				var pickedValues = valuesPossibleToPick.Where(_ => _random.Next(2) < 1);

				//Console.WriteLine(_HPWDecoder.GetDecodedValues().Count);
				List<ulong> values = new();
				foreach (var value in pickedValues)
				{
					var next = RandomDataFactory.NextInString(value);
					var before = RandomDataFactory.BeforeInString(value);

					bool nextIsDecoded = _decodedValues.Contains(next);
					bool beforeIsDecoded = _decodedValues.Contains(before);

					if (nextIsDecoded == false) values.Add(next);
					if (beforeIsDecoded == false) values.Add(before);

					if (nextIsDecoded == true && beforeIsDecoded == true) valuesPossibleToPick.Remove(next);
				}

				var possiblePures = new HashSet<ulong>();
				foreach (var hf in _hfs)
				{
					foreach (var value in values)
					{
						var hash = hf(value);
						_table.Toggle(hash, value);
						possiblePures.Add(hash);
					}
				}

				HashSet<ulong> newlyDecodedValues = new();
				_HPWDecoder.OuterDecode(possiblePures, new HashSet<ulong>(), newlyDecodedValues);

				foreach (var value in newlyDecodedValues)
				{
					if (_decodedValues.Contains(value))
					{
						_decodedValues.Remove(value);
						if (valuesPossibleToPick.Contains(value))
						{
							valuesPossibleToPick.Remove(value);
						}
						valuesPossibleToPick.Add(RandomDataFactory.NextInString(value));
						valuesPossibleToPick.Add(RandomDataFactory.BeforeInString(value));
					}
					else
					{
						_decodedValues.Add(value);
						valuesPossibleToPick.Add(value);
					}
				}

				newlyDecodedValues.Clear();


				foreach (var hf in _hfs)
				{
					foreach (var value in values)
					{
						var hash = hf(value);
						_table.Toggle(hash, value);
						possiblePures.Add(hash);
					}
				}
				_HPWDecoder.OuterDecode(possiblePures, new HashSet<ulong>(), newlyDecodedValues);
				foreach (var value in newlyDecodedValues)
				{
					if (_decodedValues.Contains(value))
					{
						_decodedValues.Remove(value);
						if (valuesPossibleToPick.Contains(value))
						{
							valuesPossibleToPick.Remove(value);
						}
						valuesPossibleToPick.Add(RandomDataFactory.NextInString(value));
						valuesPossibleToPick.Add(RandomDataFactory.BeforeInString(value));
					}
					else
					{
						_decodedValues.Add(value);
						valuesPossibleToPick.Add(value);
					}
				}
				newlyDecodedValues.Clear();


				if (_HPWDecoder.DecodingState == DecodingState.Success)
				{
					break;
				}

				count++;

				if (count % (_size / 100) == 0)
				{
					//Console.WriteLine($"{count} {valuesPossibleToPick.Count} {_decodedValues.Count}");
					valuesPossibleToPick = ClearSet(valuesPossibleToPick);
					//Console.WriteLine($"{count} {valuesPossibleToPick.Count} {_decodedValues.Count}");
					if (nValues == valuesPossibleToPick.Count) break;
					nValues = valuesPossibleToPick.Count;


				}
			}

		}


		public HashSet<ulong> GetDecodedValues()
		{
			return _decodedValues;
		}
	}
}
