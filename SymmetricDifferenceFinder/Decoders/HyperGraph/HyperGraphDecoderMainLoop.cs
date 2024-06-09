using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using SymmetricDifferenceFinder.Utils;
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
		public static Expression<Action<TSketch, ListQueue<ulong>, List<ulong>, List<ulong>>> GetDecode<TSketch>(
			Expression<Func<ulong, TSketch, bool>> IsPure,
			Expression<Action<ulong, TSketch, ListQueue<ulong>, List<ulong>, List<ulong>>> RemoveAndAddIfPure
			)
			where TSketch : IHyperGraphDecoderSketch<TSketch>
		{
			var f = CompiledActions.Create<TSketch, ListQueue<ulong>, List<ulong>, List<ulong>>(out var sketch_, out var pure_, out var addKeys_, out var removeKeys_);

			f.S
				.While(
			pure_.V.Property<int>("Count") > 0,
				new Scope()
					.This(out var S)
					//Get the last element of pure list
					.DeclareVariable(
						out var pureBucketIndex,
						pure_.V.Call<ulong>("Dequeue")
						)
					//Remove last element of pure list
					.IfThen(
						!S.Function(IsPure, pureBucketIndex.V, sketch_.V),
						new Scope().GoToEnd(S))
					.Action(RemoveAndAddIfPure, pureBucketIndex.V, sketch_.V, pure_.V, addKeys_.V, removeKeys_.V)
			);

			return f.Construct();
		}

		public static Expression<Action<TSketch, ListQueue<ulong>, List<ulong>, List<ulong>, int>> GetDecodedLimitedSteps<TSketch>(
		Expression<Func<ulong, TSketch, bool>> IsPure,
		Expression<Action<ulong, TSketch, ListQueue<ulong>, List<ulong>, List<ulong>>> RemoveAndAddIfPure
		)
		where TSketch : IHyperGraphDecoderSketch<TSketch>
		{
			var f = CompiledActions.Create<TSketch, ListQueue<ulong>, List<ulong>, List<ulong>, int>(out var sketch_, out var pure_, out var addKeys_, out var removeKeys_, out var numberOfSteps_);

			f.S
				.While(
			pure_.V.Property<int>("Count") > 0,
				new Scope()
					.IfThen(numberOfSteps_.V <= 0, new Scope().GoToEnd(f.S))
					.Assign(numberOfSteps_, numberOfSteps_.V - 1)
					.This(out var S)
					//Get the last element of pure list
					.DeclareVariable(
						out var pureBucketIndex,
						pure_.V.Call<ulong>("Dequeue")
						)
					//Remove last element of pure list
					.IfThen(
						!S.Function(IsPure, pureBucketIndex.V, sketch_.V),
						new Scope().GoToEnd(S))
					.Action(RemoveAndAddIfPure, pureBucketIndex.V, sketch_.V, pure_.V, addKeys_.V, removeKeys_.V)
			);

			return f.Construct();
		}



		public static Expression<Action<ulong, TSketch, ListQueue<ulong>, List<ulong>, List<ulong>>> GetRemoveAndAddToPure<TSketch>(HashingFunctions hashingFunctions)
			where TSketch : ISketch<TSketch>
		{
			var a = CompiledActions.Create<ulong, TSketch, ListQueue<ulong>, List<ulong>, List<ulong>>(out var key_, out var sketch_, out var pures_, out var addKeys_, out var removeKeys_);

			a.S
				.DeclareVariable(out var value_, sketch_.V.Call<ulong>("Get", key_.V))
				.DeclareVariable(out var count_, sketch_.V.Call<int>("GetCount", key_.V))
				.Macro(out var keys, hashingFunctions.Select(h => a.S.Function(h, value_.V)))
				.Macro(
					out var RemoveAndAddToPure,
					((SmartExpression<ulong> key, Scope scope, string action) v) =>
					{
						v.scope.AddExpression(pures_.V.Call<NoneType>("Add", v.key));
						v.scope.AddExpression(sketch_.V.Call<NoneType>(v.action, v.key, value_.V));
					}
					)
				.IfThen(
				count_.V == 1,
				new Scope().This(out var q)
					.BuildAction(RemoveAndAddToPure, keys.Select(key => (key, q, "Remove")))
					.AddExpression(addKeys_.V.Call<NoneType>("Add", value_.V))
				)
				.IfThen(
				count_.V == -1,
				new Scope().This(out var l)
					.BuildAction(RemoveAndAddToPure, keys.Select(key => (key, l, "Add")))
					.AddExpression(removeKeys_.V.Call<NoneType>("Add", value_.V))
				);

			return a.Construct();
		}
		public static Expression<Action<ulong, TSketch, ListQueue<ulong>, List<ulong>, List<ulong>>> GetOnlyRemoveAddToPure<TSketch>(HashingFunctions hashingFunctions)
		where TSketch : ISketch<TSketch>
		{
			var a = CompiledActions.Create<ulong, TSketch, ListQueue<ulong>, List<ulong>, List<ulong>>(out var key_, out var sketch_, out var pures_, out var addKeys_, out var removeKeys_);

			a.S
				.DeclareVariable(out var value_, sketch_.V.Call<ulong>("Get", key_.V))
				.DeclareVariable(out var count_, sketch_.V.Call<int>("GetCount", key_.V))
				.Macro(out var keys, hashingFunctions.Select(h => a.S.Function(h, value_.V)))
				.Macro(
					out var RemoveAndAddToPure,
					((SmartExpression<ulong> key, Scope scope, string action) v) =>
					{
						v.scope.AddExpression(pures_.V.Call<NoneType>("Add", v.key));
						v.scope.AddExpression(sketch_.V.Call<NoneType>(v.action, v.key, value_.V));
					}
					)
				.IfThen(
				count_.V == 1,
				new Scope().This(out var q)
					.BuildAction(RemoveAndAddToPure, keys.Select(key => (key, q, "Remove")))
					.AddExpression(addKeys_.V.Call<NoneType>("Add", value_.V))
				);
			return a.Construct();
		}



	}
}
