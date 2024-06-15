using Iced.Intel;
using LittleSharp.Utils;
using SymmetricDifferenceFinder.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Encoders
{

	public record EncoderConfiguration<TTable>(IEnumerable<IHashingFunctionScheme> HashingFunctionSchemes, int TableSize)
		where TTable : struct, ITable;

	public class EncoderFactory<TTable> : IEncoderFactory<TTable> where TTable : struct, ITable
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
				throw new ArgumentException($"Table Size{table.Size()} does not match configuration {_configuration.TableSize}", nameof(table));
			return new Encoder<TTable>(table, _hashToBufferFunctions, _bufferSize);
		}

		public EncoderFactory<TTable> CreateNewFactoryWithDifferentTableSize(int size)
		{
			return new EncoderFactory<TTable>(_configuration with { TableSize = size }, _tableFactory);
		}

		IEncoder IEncoderFactory<TTable>.Create(TTable table)
		{
			return Create(table);
		}
	}
	public class Encoder<TTable> : IEncoder, IParallelEncoder where TTable : struct, ITable
	{

		readonly TTable _table;
		readonly IEnumerable<Action<Key[], Hash[], int, int>> _hashToBufferFunctions;

		Hash[] _hashBuffer;

		int _nPartitions = 16;

		(int, int)[] _partitions;

		public Encoder(TTable table, IEnumerable<Action<Key[], Hash[], int, int>> hashToBufferFunctions, int bufferSize)
		{
			_table = table;
			_hashToBufferFunctions = hashToBufferFunctions;
			ResizeBuffer(bufferSize);
		}

		public void SetPortions(int partitions)
		{
			_nPartitions = partitions;
			ResizePartitions();
		}

		void ResizeBuffer(int newSize)
		{
			_hashBuffer = new Hash[newSize];
			ResizePartitions();

		}
		void ResizePartitions()
		{
			_partitions = new (int, int)[_nPartitions];
			int partitionLength = _hashBuffer.Length / _nPartitions;
			int lastPartitionLength = partitionLength + _hashBuffer.Length % _nPartitions;
			for (int i = 0; i < _nPartitions - 1; i++)
			{
				_partitions[i] = (i * partitionLength, partitionLength);
			}
			_partitions[_nPartitions - 1] = ((_nPartitions - 1) * partitionLength, lastPartitionLength);

		}

		public void Encode(ulong[] buffer, int nItemsInBuffer)
		{
			if (_hashBuffer.Length < nItemsInBuffer) ResizeBuffer(buffer.Length);

			foreach (var hashToBufferFunction in _hashToBufferFunctions)
			{
				hashToBufferFunction(buffer, _hashBuffer, 0, nItemsInBuffer);

				for (int i = 0; i < nItemsInBuffer; i++)
				{
					_table.Add((uint)_hashBuffer[i], buffer[i]);
				}
			}

		}

		public void EncodeParallel(ulong[] buffer, int nItemsInBuffer)
		{
			if (_hashBuffer.Length < nItemsInBuffer) ResizeBuffer(buffer.Length);

			foreach (var hashToBufferFunction in _hashToBufferFunctions)
			{

				Parallel.For(0, _nPartitions, (i) => hashToBufferFunction(buffer, _hashBuffer, _partitions[i].Item1, _partitions[i].Item2));
				for (int i = 0; i < nItemsInBuffer; i++)
				{
					_table.Add((uint)_hashBuffer[i], buffer[i]);
				}
			}

		}

		public TTable GetTable()
		{
			return _table;
		}
	}
}
