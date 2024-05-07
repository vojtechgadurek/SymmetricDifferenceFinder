using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using SymmetricDifferenceFinder.Decoders;
using SymmetricDifferenceFinder.Decoders.HPW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Tables
{
	public struct XORTable : ITable, IHPWSketch<XORTable>
	{
		ulong[] _table;


		int ISketch<XORTable>.Size => Size();
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

		public void Toggle(Hash key, Key value)
		{
			_table[key] ^= value;
		}

		public int Size()
		{
			return _table.Length;
		}

		public ulong Get(ulong key)
		{
			return _table[key];
		}


		public bool IsEmpty()
		{
			return _table.All(x => x == 0);
		}

		public XORTable SymmetricDifference(XORTable other)
		{
			if (other.Size() != Size())
			{
				throw new InvalidOperationException("Sizes of tables are not same");
			}

			var table = new ulong[Size()];
			table.CopyTo(table, 0);

			XORTable merged = new XORTable(table);

			for (ulong i = 0; i < (ulong)Size(); i++)
			{
				merged.Toggle(i, other.Get(i));
			}

			return merged;
		}
	}
}
