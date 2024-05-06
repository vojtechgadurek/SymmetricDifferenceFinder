using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using HashingFunctions = System.Collections.Generic.List<System.Linq.Expressions.Expression<System.Func<ulong, ulong>>>;


namespace SymmetricDifferenceFinder.Decoders
{
	public static class DecodingHelperFunctions
	{
		public static Expression<Func<Hash, TSketch, bool>> GetLooksPure<TSketch>(HashingFunctions hashingFunctions)
		where TSketch : ISketch
		{
			var f = CompiledFunctions.Create<Hash, TSketch, bool>(out var hash_, out var table_);

			// Tests whether such hash is pure
			// Let h_i be a hash function
			// If exists h_i such that h_i(key) == hash, then key is pure
			f.S
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
			f.S.Assign(f.Output, false);
			return f.Construct();
		}

		public static Expression<Action<ulong, TSet, TSketch>> AddIfLooksPure<TSet, TSketch>(Expression<Func<ulong, TSketch, bool>> looksPure)

			where TSketch : ISketch
		{
			var a = CompiledActions.Create<ulong, TSet, TSketch>(out var hash_, out var set_, out var table_);
			a.S
				.Function(looksPure, hash_.V, table_.V, out var check)
				.IfThen(check, new Scope().AddExpression(set_.V.Call<NoneType>("Add", hash_.V)))
				;
			return a.Construct();
		}

		public static Expression<Action<TSketch, int, TSet>> Initialize<TSketch, TSet>(
			Expression<Action<ulong, TSet, TSketch>> AddIfLooksPure
		)
			where TSketch : ISketch
		{
			var a = CompiledActions.Create<TSketch, int, TSet>(out var table_, out var size_, out var set_);
			a.S.DeclareVariable<ulong>(out var i_, 0)
				.While(
					i_.V < size_.V.Convert<ulong>(),
					new Scope()
						.Action(AddIfLooksPure, i_.V, set_.V, table_.V)
						.Assign(i_, i_.V + 1))
				;
			return a.Construct();
		}
	}
}
