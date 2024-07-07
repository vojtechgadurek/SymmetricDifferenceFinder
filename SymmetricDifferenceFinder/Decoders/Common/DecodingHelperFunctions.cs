using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using HashingFunctions = System.Collections.Generic.IEnumerable<System.Linq.Expressions.Expression<System.Func<ulong, ulong>>>;


namespace SymmetricDifferenceFinder.Decoders.Common
{
	public static class DecodingHelperFunctions
	{

		public static Expression<Action<ulong, TSet, TSketch>> GetAddIfLooksPure<TSet, TSketch>(Expression<Func<ulong, TSketch, bool>> looksPure)

			where TSketch : ISketch<TSketch>
		{
			var a = CompiledActions.Create<ulong, TSet, TSketch>(out var hash_, out var set_, out var table_);
			a.S//.Print("Start->")
					.Function(looksPure, hash_.V, table_.V, out var check)
					.Print(check.ToStringExpression())
					//.Print("<-End")
					.IfThen(check, new Scope().AddExpression(set_.V.Call<NoneType>("Add", hash_.V)))
					;
			return a.Construct();
		}




		public static Expression<Action<TSketch, int, TSet>> GetInitialize<TSketch, TSet>(
			Expression<Func<ulong, TSketch, bool>> looksPure
		)
			where TSketch : ISketch<TSketch>
		{
			var a = CompiledActions.Create<TSketch, int, TSet>(out var table_, out var size_, out var set_);
			a.S.DeclareVariable<ulong>(out var i_, 0)
				.While(
					i_.V < size_.V.Convert<ulong>(),
					new Scope()//.Print("Start->")
						.Function(looksPure, i_.V, table_.V, out var check)
						.IfThen(check, new Scope().AddExpression(set_.V.Call<NoneType>("Add", i_.V)))
						//.Print("<-End")

						//.Print(i_.V.ToStringExpression())
						.Assign(i_, i_.V + 1))
				;
			return a.Construct();
		}


	}
}
