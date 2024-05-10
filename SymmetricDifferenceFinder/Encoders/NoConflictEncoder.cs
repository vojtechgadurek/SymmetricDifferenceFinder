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

		int _nPartitions = 16;

		(int, int)[] _partitions;

		readonly int hashingFunctionNumber;

		public NoConflictEncoder(TTable table, IEnumerable<Action<Key[], Hash[], int, int>> hashToBufferFunctions, int bufferSize)
		{
			_table = table;
			_hashToBufferFunctions = hashToBufferFunctions.ToList();
			hashingFunctionNumber = _hashToBufferFunctions.Count();
			ResizeBuffer(bufferSize);

		}

		public void SetPortions(int partitions)
		{
			_nPartitions = partitions;
			ResizePartitions();
		}

		public void ResizeBuffer(int newSize)
		{
			_hashBuffer = new Hash[hashingFunctionNumber][];
			for (int i = 0; i < hashingFunctionNumber; i++)
			{
				_hashBuffer[i] = new Hash[newSize];
			}
			ResizePartitions();

		}
		public void ResizePartitions()
		{
			_partitions = new (int, int)[_nPartitions];
			int partitionLength = _hashBuffer.Length / _nPartitions;
			int lastPartitionLength = partitionLength + _hashBuffer.Length % partitionLength;
			for (int i = 0; i < _nPartitions - 1; i++)
			{
				_partitions[i] = (i * partitionLength, partitionLength);
			}
			_partitions[_nPartitions - 1] = ((_nPartitions - 1) * partitionLength, lastPartitionLength);

		}

		public void EncodeParallel(ulong[] buffer, int nItemsInBuffer)
		{
			if (_hashBuffer.Length < nItemsInBuffer) ResizeBuffer(buffer.Length);

			Parallel.For(0, _hashToBufferFunctions.Count, (hf) =>
			{

				Parallel.For(0, _nPartitions, (i) => _hashToBufferFunctions[hf](buffer, _hashBuffer[hf], _partitions[i].Item1, _partitions[i].Item2));
				for (int i = 0; i < nItemsInBuffer; i++)
				{
					_table.Add((uint)_hashBuffer[hf][i], buffer[i]);
				}
			}
			);

		}

		public TTable GetTable()
		{
			return _table;
		}
	}
}
