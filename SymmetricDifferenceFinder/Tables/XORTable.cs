using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using SymmetricDifferenceFinder.Decoders;
using SymmetricDifferenceFinder.Decoders.HPW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using HashingFunctions = System.Collections.Generic.IEnumerable<System.Linq.Expressions.Expression<System.Func<ulong, ulong>>>;


namespace SymmetricDifferenceFinder.Tables
{
	public struct XORTable : ITable, IHPWSketch<XORTable>
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

		static public Expression<Func<Hash, XORTable, bool>> GetLooksPure(HashingFunctions hashingFunctions)
		{
			var f = CompiledFunctions.Create<Hash, XORTable, bool>(out var hash_, out var table_);

			// Tests whether such hash is pure
			// Let h_i be _a hash function
			// If exists h_i such that h_i(key) == hash, then key is pure
			f.S
				.Assign(f.Output, false)
				.DeclareVariable(out var value_, table_.V.Call<Key>("Get", hash_.V))
				.IfThen(
					value_.V == 0,
					new Scope().GoToEnd(f.S)
					)
				;

			foreach (var hashFunc in hashingFunctions)
			{
				f.S.Function(hashFunc, value_.V, out var testedHash_)
					.IfThen(testedHash_ == hash_.V,
						new Scope()
						.Assign(f.Output, true)
						.GoToEnd(f.S)
						);
			}
			return f.Construct();
		}

	}
}
