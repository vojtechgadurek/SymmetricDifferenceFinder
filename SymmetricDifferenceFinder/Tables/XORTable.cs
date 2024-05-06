using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Tables
{
	public struct XORTable : ITable
	{
		ulong[] _table;
		public XORTable(ulong[] table)
		{
			_table = table;
		}

		public XORTable(int size)
		{
			_table = new ulong[size];
		}

		public ulong[] GetUnderlyingTable()
		{
			return _table;
		}


		public void Add(Hash key, Key value)
		{
			_table[key] ^= value;
		}

		public int Size()
		{
			return _table.Length;
		}

	}
}
