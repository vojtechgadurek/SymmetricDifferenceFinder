using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Encoders
{
	public class NoConflictEncoder<TTable> : IParallelEncoder where TTable : struct, ITable
	{

		readonly TTable _table;
		readonly List<Action<Key[], Hash[], int, int>> _hashToBufferFunctions;

		Hash[][] _hashBuffer;

		int _hashBufferLength;

		int _nPartitions = 4;

		(int, int)[] _partitions;

		readonly int hashingFunctionNumber;

		public NoConflictEncoder(TTable table, IEnumerable<Action<Key[], Hash[], int, int>> hashToBufferFunctions, int bufferSize)
		{
			_table = table;
			_hashToBufferFunctions = hashToBufferFunctions.ToList();
			hashingFunctionNumber = _hashToBufferFunctions.Count();

			ResizeBuffer(bufferSize);

		}

		public void SetPartitions(int partitions)
		{
			_nPartitions = partitions;
			ResizePartitions();
		}

		void ResizeBuffer(int newSize)
		{
			_hashBufferLength = newSize;
			_hashBuffer = new Hash[hashingFunctionNumber][];
			for (int i = 0; i < hashingFunctionNumber; i++)
			{
				_hashBuffer[i] = new Hash[newSize];
			}
			ResizePartitions();

		}
		void ResizePartitions()
		{
			_partitions = new (int, int)[_nPartitions];
			int partitionLength = _hashBufferLength / _nPartitions;
			int lastPartitionLength = partitionLength + _hashBufferLength % _nPartitions;
			for (int i = 0; i < _nPartitions - 1; i++)
			{
				_partitions[i] = (i * partitionLength, partitionLength);
			}
			_partitions[_nPartitions - 1] = ((_nPartitions - 1) * partitionLength, lastPartitionLength);

		}

		public void EncodeParallel(ulong[] buffer, int nItemsInBuffer)
		{
			if (_hashBufferLength < nItemsInBuffer) ResizeBuffer(buffer.Length);

			Parallel.For(0, _hashToBufferFunctions.Count, (hf) =>
			{

				_hashToBufferFunctions[hf](buffer, _hashBuffer[hf], 0, nItemsInBuffer);
				for (int i = 0; i < nItemsInBuffer; i++)
				{
					_table.Add(_hashBuffer[hf][i], buffer[i]);
				}

				//_hashToBufferFunctions[hf](buffer, _hashBuffer[hf], 0, _hashBufferLength);
				//for (int j = 0; j < _hashBufferLength; j++)
				//{
				//	_table.Add(_hashBuffer[hf][j], buffer[j]);
				//}

			});
		}

		public TTable GetTable()
		{
			return _table;
		}
	}
}
