using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Diagnostics.Tracing.Analysis;
using Microsoft.Diagnostics.Tracing.Parsers.IIS_Trace;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftAntimalwareEngine;
using SymmetricDifferenceFinder.Decoders.HPW;
using SymmetricDifferenceFinder.Improvements.Oracles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SymmetricDifferenceFinder.Improvements
{

	public class MassagerFactory<TStringFactory, TPipeline> : IDecoderFactory<XORTable>
		where TStringFactory : struct, IStringFactory
		where TPipeline : struct, IPipeline
	{
		HPWDecoderFactory<XORTable> _decoderFactory;
		IEnumerable<HashingFunction> _hfs;
		public MassagerFactory(IEnumerable<Expression<HashingFunction>> hfs, HPWDecoderFactory<XORTable> factory)
		{
			_decoderFactory = factory;
			_hfs = hfs.Select(f => f.Compile()).ToList();
		}
		public IDecoder Create(XORTable sketch)
		{
			return new Massager<TStringFactory, TPipeline>(_decoderFactory.Create(sketch), _hfs);
		}
	}

	public class Massager<TStringFactory, TPipeline> : IDecoder
		where TStringFactory : struct, IStringFactory
		where TPipeline : struct, IPipeline
	{
		HPWDecoder<XORTable> _HPWDecoder;
		XORTable _table;
		Random _random = new Random();
		int _size;
		List<Func<ulong, ulong>> _hfs;
		HashSet<ulong> _decodedValues;

		PickOneRandomly<Cache<Pipeline<BeforeOracle<TStringFactory>, TPipeline>>> _before = default;
		PickOneRandomly<Cache<Pipeline<BeforeOracle<TStringFactory>, TPipeline>>> _next = default;


		EndpointsLocalizer<Cache<Pipeline<BeforeOracle<TStringFactory>, TPipeline>>, False> beforeLocalizer = new();
		EndpointsLocalizer<Cache<Pipeline<BeforeOracle<TStringFactory>, TPipeline>>, False> nextLocalizer = new();

		public DecodingState DecodingState => _HPWDecoder.DecodingState;

		public Massager(HPWDecoder<XORTable> HPWDecoder, IEnumerable<HashingFunction> hfs)
		{
			_table = HPWDecoder.Sketch;
			_size = _table.Size();
			_hfs = hfs.ToList();

			_HPWDecoder = HPWDecoder;
			_decodedValues = _HPWDecoder.GetDecodedValues();
		}






		public void ClearingStep()
		{

		}

		public void VeryRandomAttackDecode()
		{
			void ToggleValues(HashSet<ulong> possiblePures, HashSet<ulong> nextPures, List<ulong> values)
			{
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
				_HPWDecoder.OuterDecode(possiblePures, nextPures, newlyDecodedValues);

				foreach (var value in newlyDecodedValues)
				{
					if (_decodedValues.Contains(value))
					{
						_decodedValues.Remove(value);
					}
					else
					{
						_decodedValues.Add(value);
					}
				}
				newlyDecodedValues.Clear();
			}

			var selectedValues = _decodedValues.Where(_ => _random.Next(2) == 0).ToList();



			var values = selectedValues.Select(_before.GetRandom).Concat(selectedValues).Select(_next.GetRandom).ToList();

		}



		void ToggleValues(HashSet<ulong>? possiblePures, List<ulong> values)
		{
			foreach (var hf in _hfs)
			{
				foreach (var value in values)
				{
					var hash = hf(value);
					_table.Toggle(hash, value);
					if (possiblePures is not null)
					{
						possiblePures.Add(hash);
					}
				}
			}

			ToggleDecodedValues(values);
		}

		void Prune(double probability)
		{
			ToggleValues(null, _decodedValues.Where(_ => _random.NextDouble() > probability).ToList());
		}


		List<ulong> GetCloseToEndpoints(double probability)
		{
			List<ulong> values = new();
			foreach (var value in beforeLocalizer.EndPoints)
			{
				values.Add(_before.GetRandom(value));
			}
			foreach (var value in nextLocalizer.EndPoints)
			{
				values.Add(_next.GetRandom(value));
			}
			return values.Where(_ => _random.NextDouble() > probability).ToList();
		}


		List<ulong> GetCloseToDecoded(double probability)
		{
			List<ulong> values = new();
			foreach (var value in _decodedValues)
			{
				values.Add(_before.GetRandom(value));
			}
			foreach (var value in _decodedValues)
			{
				values.Add(_next.GetRandom(value));
			}
			return values.Where(_ => _random.NextDouble() > probability).ToList();
		}


		void ToggleValues(HashSet<ulong> possiblePures, HashSet<ulong> nextPures, List<ulong> values)
		{
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
			_HPWDecoder.OuterDecode(possiblePures, nextPures, newlyDecodedValues);

			foreach (var value in newlyDecodedValues)
			{
				if (_decodedValues.Contains(value))
				{
					_decodedValues.Remove(value);

					nextLocalizer.RemoveNode(value);
					beforeLocalizer.RemoveNode(value);
				}
				else
				{
					_decodedValues.Add(value);
					nextLocalizer.AddNode(value);
					beforeLocalizer.AddNode(value);

				}
			}
			newlyDecodedValues.Clear();
		}
		public void RandomAttackDecode(bool probe)
		{

			int nMassages = _size / 100;
			_HPWDecoder.MaxNumberOfIterations = 10;
			int count = 0;


			var possiblePures = new HashSet<ulong>();
			var nextPures = new HashSet<ulong>();


			while (nMassages > count)
			{
				//var pickedValues = valuesPossibleToPick.Where(_ => _random.Next(2) < 1);
				//var pickedValues = Enumerable.Range(0, _size).Select(i => _hyperGraph.GetBucket((ulong)i).FirstOrDefault()).Where(x => x != default);
				//Console.WriteLine(_HPWDecoder.GetDecodedValues().Count);
				List<ulong> values = new();
				//foreach (var value in pickedValues)
				//{
				//	var next = _next.GetRandom(value);
				//	var before = _before.GetRandom(value);

				//	bool nextIsDecoded = _decodedValues.Contains(next);
				//	bool beforeIsDecoded = _decodedValues.Contains(before);

				//	if (nextIsDecoded == false) values.Add(next);
				//	if (beforeIsDecoded == false) values.Add(before);

				//	//if (nextIsDecoded == true && beforeIsDecoded == true) valuesPossibleToPick.Remove(next);
				//}

				foreach (var value in beforeLocalizer.EndPoints)
				{
					values.Add(_before.GetRandom(value));
				}
				foreach (var value in nextLocalizer.EndPoints)
				{
					values.Add(_next.GetRandom(value));
				}


				values = values.Where(_ => _random.NextDouble() > 0.5).ToList();

				List<ulong> valuesNext = new();
				foreach (var value in values)
				{
					valuesNext.Add(_before.GetRandom(value));
				}
				foreach (var value in values)
				{
					valuesNext.Add(_next.GetRandom(value));
				}


				values = values.Concat(valuesNext).Concat(nextLocalizer.EndPoints).Concat(beforeLocalizer.EndPoints).ToList();



				//There are two for reason
				//We add some values in the first
				//that are removed in the second

				ToggleValues(possiblePures, nextPures, values);
				possiblePures = nextPures;
				nextPures = new HashSet<ulong>();

				ToggleValues(possiblePures, nextPures, values);

				possiblePures = nextPures;
				nextPures = new HashSet<ulong>();

				if (_HPWDecoder.DecodingState == DecodingState.Success)
				{
					break;
				}

				count++;

				if (count % 100 == 0 && probe)
				{
					ToggleValues(new(), new(), _decodedValues.Where(_ => _random.Next(10) == 0).ToList());
				}

				if (count % 100 == 0)
				{

					//Console.WriteLine($"{beforeLocalizer.EndPoints.Count()} {nextLocalizer.EndPoints.Count()}");

					//Console.WriteLine(beforeLocalizer.Nodes.Where(x => x.Value.IsEndpoint()).Count());

					beforeLocalizer = new();
					nextLocalizer = new();
					foreach (var value in _decodedValues)
					{
						beforeLocalizer.AddNode(value);
						nextLocalizer.AddNode(value);
					}
					//Console.WriteLine($"{beforeLocalizer.EndPoints.Count()} {nextLocalizer.EndPoints.Count()}");
					//Console.WriteLine($"{count} {values.Count()} {_decodedValues.Count}");
					//Console.WriteLine($"{_count} {valuesPossibleToPick.Count} {_decodedValues.Count}");
				}
			}
		}

		public void FindPure(HashSet<ulong> pure)
		{
			for (ulong i = 0; i < (ulong)_size; i++)
			{
				pure.Add(i);
			}
		}
		public void ToggleDecodedValues(IEnumerable<ulong> values)
		{
			foreach (var value in values)
			{
				if (_decodedValues.Contains(value))
				{
					_decodedValues.Remove(value);
					//beforeLocalizer.RemoveNode(value);
					//nextLocalizer.RemoveNode(value);
				}
				else
				{
					_decodedValues.Add(value);
					//beforeLocalizer.AddNode(value);
					//nextLocalizer.AddNode(value);
				}
			}
		}
		public void Decode()
		{
			_HPWDecoder.MaxNumberOfIterations = 100;
			_HPWDecoder.Decode();
			_HPWDecoder.MaxNumberOfIterations = 10;

			//foreach (var value in _decodedValues)
			//{
			//	beforeLocalizer.AddNode(value);
			//	nextLocalizer.AddNode(value);
			//};

			HashSet<ulong> pure = new HashSet<ulong>();
			HashSet<ulong> nextPure = new HashSet<ulong>();
			HashSet<ulong> decodedValues = new HashSet<ulong>();

			int maxRounds = 1000;
			for (int i = 0; i < maxRounds; i++)
			{
				if (_HPWDecoder.DecodingState == DecodingState.Success)
				{
					//Console.WriteLine(i);
					break;
				}
				//BinPackingDecode();

				List<ulong> values;
				if (i % 7 < 0)
				{
					values = GetCloseToEndpoints(0.5);
				}
				else
				{
					values = GetCloseToDecoded(0.5);
				}

				ToggleValues(pure, values);

				_HPWDecoder.OuterDecode(pure, nextPure, decodedValues);
				ToggleDecodedValues(decodedValues);
				decodedValues.Clear();

				ToggleValues(nextPure, values);

				pure.Clear();

				_HPWDecoder.OuterDecode(nextPure, pure, decodedValues);
				ToggleDecodedValues(decodedValues);
				decodedValues.Clear();

				nextPure.Clear();


				if (i < maxRounds * 0.8 && _random.NextDouble() <= 0.01 && _HPWDecoder.DecodingState != DecodingState.Success)
				{
					Prune(0.1);
					//beforeLocalizer = new();
					//nextLocalizer = new();
					//foreach (var value in _decodedValues)
					//{
					//	beforeLocalizer.AddNode(value);
					//	nextLocalizer.AddNode(value);
					//};
				}

				if (i == maxRounds - 1)
				{
					//Console.WriteLine(i);
				}
			}
		}
		//public void Decode()
		//{
		//	_HPWDecoder.Decode();
		//	foreach (var value in _decodedValues)
		//	{
		//		beforeLocalizer.AddNode(value);
		//		nextLocalizer.AddNode(value);
		//	}
		//	for (int i = 0; i < 10; i++)
		//	{
		//		if (_HPWDecoder.DecodingState == DecodingState.Success) break;
		//		//BinPackingDecode();
		//		RandomAttackDecode(true);
		//	}
		//	RandomAttackDecode(false);


		//}

		public HashSet<ulong> GetDecodedValues()
		{
			return _decodedValues;
		}
	}
}
