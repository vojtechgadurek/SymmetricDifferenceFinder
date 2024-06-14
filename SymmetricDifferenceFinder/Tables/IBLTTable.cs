using BenchmarkDotNet.Attributes;
using LittleSharp.Literals;
using SymmetricDifferenceFinder.Decoders;
using SymmetricDifferenceFinder.Decoders.HPW;
using SymmetricDifferenceFinder.Decoders.HyperGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HashingFunctions = System.Collections.Generic.IEnumerable<System.Linq.Expressions.Expression<System.Func<ulong, ulong>>>;


namespace SymmetricDifferenceFinder.Tables
{
	public struct IBLTTable : ITable, IHyperGraphDecoderSketch<IBLTTable>
	{
		private IBLTCell[] _table;

		static MD5 mD5 = System.Security.Cryptography.MD5.Create();
		static Func<ulong, ulong> ControlHashingFunction = x => BitConverter.ToUInt64(mD5.ComputeHash(BitConverter.GetBytes(x))); //x => x * x * 12342834932434242492 & (1 << 31 - 1); //HashingFunctionProvider.Get(typeof(MultiplyShiftFamily), ulong.MaxValue, 0).Create().Compile();

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
			public int Count;
			public ulong HashSum;
			public ulong KeySum;
		}

		public ulong CallControlHashingFunction(ulong value)
		{
			return ControlHashingFunction(value);
		}

		public void Add(Hash key, Key value)
		{
			_table[key].Count++;
			_table[key].HashSum += CallControlHashingFunction(value);
			_table[key].KeySum += value;
		}

		public void Remove(Hash key, Key value)
		{
			_table[key].Count--;
			_table[key].HashSum -= CallControlHashingFunction(value);
			_table[key].KeySum -= value;
		}


		public int Size()
		{
			return _table.Length;
		}

		public int GetCount(Hash key)
		{
			return _table[key].Count;
		}
		public ulong GetHashCheck(ulong key)
		{
			if (_table[key].Count >= 0)
			{
				return _table[key].HashSum;
			}
			return 0UL - (_table[key].HashSum);
		}

		public ulong Get(ulong key)
		{
			if (_table[key].Count >= 0)
			{
				return _table[key].KeySum;
			}
			return 0UL - (_table[key].KeySum);
		}

		public IBLTTable SymmetricDifference(IBLTTable other)
		{
			if (other.Size() != Size())
			{
				throw new InvalidOperationException("Sketches do not have same sizes");
			}
			IBLTCell[] data = new IBLTCell[Size()];
			_table.CopyTo(data, 0);
			for (int i = 0; i < data.Length; i++)
			{
				IBLTCell otherCell = other.GetCell((ulong)i);
				_table[i].KeySum -= otherCell.KeySum;
				_table[i].HashSum -= otherCell.HashSum;
				_table[i].Count -= otherCell.Count;

			}
			return new IBLTTable(data);
		}

		public IBLTCell GetCell(Hash key)
		{
			return _table[key];
		}

		public bool IsEmpty()
		{
			return _table.All(x => 0 == x.Count && 0 == x.HashSum && 0 == x.KeySum);
		}


		public static Expression<Func<ulong, IBLTTable, bool>> GetLooksPure(HashingFunctions hashingFunctions)
		{
			var f = CompiledFunctions.Create<ulong, IBLTTable, bool>(out var key_, out var sketch_);
			f.S.Assign(f.Output, false)
				.DeclareVariable(out var count_, sketch_.V.Call<int>("GetCount", key_.V))
				.IfThen(!(count_.V == 1 | count_.V == -1), new Scope().GoToEnd(f.S))
				.DeclareVariable(out var value_, sketch_.V.Call<ulong>("Get", key_.V))
				.DeclareVariable(out var hashCheck_, sketch_.V.Call<ulong>("GetHashCheck", key_.V))
				.Assign(f.Output, sketch_.V.Call<ulong>(nameof(CallControlHashingFunction), value_.V) == hashCheck_.V);
			return f.Construct();
		}
	}
}
