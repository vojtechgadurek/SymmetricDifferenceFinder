using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using HashingFunctions = System.Collections.Generic.IEnumerable<System.Linq.Expressions.Expression<System.Func<ulong, ulong>>>;

namespace SymmetricDifferenceFinder.Decoders.HyperGraph
{
	public static class HyperGraphDecoderMainLoop
	{
		public static Expression<Action<TSketch, List<ulong>>> GetDecode<TSketch>(
			Expression<Func<ulong, TSketch, bool>> IsPure,
			Expression<Action<ulong, TSketch, List<ulong>>> RemoveAndAddIfPure
			)
			where TSketch : IHyperGraphDecoderSketch<TSketch>
		{
			var f = CompiledActions.Create<TSketch, List<ulong>>(out var sketch_, out var pure_);
			f.S.While(
			pure_.V.Call<int>("Count") > 0,

				new Scope()
					.This(out var S)
					//Get the last element of pure list
					.DeclareVariable(
						out var pureBucketIndex,
						pure_.V.Call<ulong>("Get", pure_.V.Call<int>("Count") - 1)
					)
					//Remove last element of pure list
					.AddExpression(pure_.V.Call<int>("RemoveAt", pure_.V.Call<int>("Count") - 1))
					.IfThen(
						S.Function(IsPure, pureBucketIndex.V, sketch_.V),
						new Scope().GoToEnd(S))
					.Action(RemoveAndAddIfPure, pureBucketIndex.V, sketch_.V, pure_.V)
			);
			return f.Construct();
		}

		public static Expression<Func<ulong, TSketch, bool>> GetIsPure<TSketch>(HashingFunctions hashingFunctions)
			where TSketch : ISketch<TSketch>
		{
			var f = CompiledFunctions.Create<ulong, TSketch, bool>(out var key_, out var sketch_);
			f.S.Assign(f.Output, false)
				.DeclareVariable(out var count_, sketch_.V.Call<int>("GetCount", key_.V))
				.IfThen(!(count_.V == 1 | count_.V == -1), new Scope().GoToEnd(f.S))
				.DeclareVariable(out var value_, sketch_.V.Call<ulong>("Get", key_.V))
				.DeclareVariable(out var hashCheck_, sketch_.V.Call<ulong>("GetHashCheck", key_.V))
				.IfThen(count_.V == -1, new Scope().Assign(hashCheck_, -hashCheck_.V))
				.Assign(f.Output,
					hashingFunctions
						.Select(h => f.S.Function(h, value_.V))
						.Select(k => k == hashCheck_.V)
						.Aggregate((x, y) => x | y)
						);
			return f.Construct();

		}


		public static Expression<Action<int, TSketch, List<int>>> GetRemoveAndIfPure<TSketch>(HashingFunctions hashingFunctions)
			where TSketch : ISketch<TSketch>
		{
			var a = CompiledActions.Create<int, TSketch, List<int>>(out var key_, out var sketch_, out var pures_);

			a.S
				.DeclareVariable(out var value_, sketch_.V.Call<ulong>("Get", key_.V))
				.DeclareVariable(out var count_, sketch_.V.Call<int>("GetCount", key_.V))
				.Macro(out var keys, hashingFunctions.Select(h => a.S.Function(h, value_.V)))
				.Macro(
					out var RemoveAndAddToPure,
					(SmartExpression<ulong> key, Scope scope, string action) =>
						{
							scope.AddExpression(pures_.V.Call<NoneType>("Add", key));
							scope.AddExpression(sketch_.V.Call<NoneType>(action, key, value_.V));
						}
					)
				.IfThen(
				count_.V == 1,
				new Scope().This(out var q)
				)
				.IfThen(
				count_.V == -1,
				new Scope().This(out var l)
				);

			foreach (var key in keys)
			{
				RemoveAndAddToPure(key, q, "Remove");
				RemoveAndAddToPure(key, l, "Add");
			};

			return a.Construct();
		}
	}
}
