using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Tables
{
	public struct IBLTTable : ITable
	{
		private IBLTCell[] _table;

		public IBLTTable(IBLTCell[] table)
		{
			_table = table;
		}

		public IBLTTable(int size)
		{
			_table = new IBLTCell[size];
		}

		public struct IBLTCell
		{
			public int count;
			public ulong hashSum;
			public ulong keySum;
		}
		public void Add(Hash key, Key value)
		{
			_table[key].count++;
			_table[key].hashSum += key;
			_table[key].keySum += value;
		}

		public void Remove(Hash key, Key value)
		{
			_table[key].count--;
			_table[key].hashSum -= key;
			_table[key].keySum -= value;
		}

		public IBLTTable GetSymmetricDifference(IBLTTable other)
		{


		}

		public int Size()
		{
			return _table.Length;
		}

	}
}
