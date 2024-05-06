using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinderTests.Utils
{
	public struct OverwriteTable : ITable
	{
		public ulong[] _table;

		public OverwriteTable(ulong[] table)
		{
			_table = table;
		}

		public OverwriteTable(int size)
		{
			_table = new ulong[size];
		}

		public void Add(ulong key, ulong value)
		{
			_table[key] = value;
		}

		public int Size()
		{
			return _table.Length;
		}
	}
}
