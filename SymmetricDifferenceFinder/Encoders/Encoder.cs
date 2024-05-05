using LittleSharp.Utils;
using SymmetricDifferenceFinder.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceBenchmarks.Encoders
{

	public record EncoderConfiguration<TTable>(IEnumerable<IHashingFunctionScheme> HashingFunctionSchemes, int TableSize)
		where TTable : struct, ITable;

	public class EncoderFactory<TTable> where TTable : struct, ITable
	{
		readonly IEnumerable<Action<Key[], Hash[], int, int>> _hashToBufferFunctions;
		readonly EncoderConfiguration<TTable> _configuration;
		int _bufferSize = 1024;
		Func<int, TTable> _tableFactory;
		public EncoderFactory(EncoderConfiguration<TTable> configuration, Func<int, TTable> tableFactory)
		{
			_configuration = configuration;
			_hashToBufferFunctions = configuration.HashingFunctionSchemes
				.Select(s => Buffering.BufferFunction(s.Create()))
				.Select(f => f.Compile())
				.ToList();
			_tableFactory = tableFactory;
		}

		public EncoderFactory<TTable> SetBufferSize(int bufferSize)
		{
			_bufferSize = bufferSize;
			return this;
		}

		public Encoder<TTable> Create()
		{
			return Create(_tableFactory(_configuration.TableSize));
		}

		public Encoder<TTable> Create(TTable table)
		{
			if (table.Size() != _configuration.TableSize)
				throw new ArgumentException($"Table size{table.Size()} does not match configuration {_configuration.TableSize}", nameof(table));
			return new Encoder<TTable>(table, _hashToBufferFunctions, _bufferSize);
		}

		public EncoderFactory<TTable> CreateNewFactoryWithDifferentTableSize(int size)
		{
			return new EncoderFactory<TTable>(_configuration with { TableSize = size }, _tableFactory);
		}
	}
	public class Encoder<TTable> where TTable : struct, ITable
	{

		readonly TTable _table;
		readonly IEnumerable<Action<Key[], Hash[], int, int>> _hashToBufferFunctions;

		Hash[] _hashBuffer;

		public Encoder(TTable table, IEnumerable<Action<Key[], Hash[], int, int>> hashToBufferFunctions, int bufferSize)
		{
			_table = table;
			_hashToBufferFunctions = hashToBufferFunctions;
			_hashBuffer = new Hash[bufferSize];
		}

		public void Encode(ulong[] buffer, int nItemsInBuffer)
		{
			foreach (var hashToBufferFunction in _hashToBufferFunctions)
			{
				hashToBufferFunction(buffer, _hashBuffer, 0, nItemsInBuffer);

				for (int i = 0; i < nItemsInBuffer; i++)
				{
					_table.Add((uint)i, _hashBuffer[i]);
				}
			}

		}
	}
}
