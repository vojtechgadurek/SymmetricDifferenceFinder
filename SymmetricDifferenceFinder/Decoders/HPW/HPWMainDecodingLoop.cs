﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using HashingFunctions = System.Collections.Generic.IEnumerable<System.Linq.Expressions.Expression<System.Func<ulong, ulong>>>;


namespace SymmetricDifferenceFinder.Decoders.HPW
{
	public static class HPWMainDecodingLoop
	{
		public static Expression<Action<ulong[], int, TSet, HashSet<ulong>, TSketch>> GetOneDecodingStep<TSet, TSketch>(
		HashingFunctions hashingFunctions,
		Expression<Action<ulong, ulong, TSketch>> Toggle,
		Expression<Func<ulong, TSketch, bool>> LooksPure,
		Expression<Action<ulong, TSet, TSketch>> AddIfLooksPure
		)
			where TSketch : ISketch<TSketch>
		{
			var f = CompiledActions.Create<ulong[], int, TSet, HashSet<ulong>, TSketch>(
				out var pures_, out var numberOfItems_, out var nextStepPures_, out var answerKeys_, out var sketch_
				);

			f.S.DeclareVariable<int>(out var i_, 0)
				.Macro(out var pures_T, pures_.V.ToTable<ulong>())
				.Macro(out var answerKeys_SET, answerKeys_.V.ToSet<ulong>())
				.While(i_.V < numberOfItems_.V,
					new Scope()
						.This(out var S)
						.AddFinalizer(new Scope().Assign(i_, i_.V + 1))
						//Pure is the index of possibly pure bucket
						.DeclareVariable(out var pure_, pures_T[i_.V].V)
						//Test whether is truly pure
						.IfThen(!S.Function(LooksPure, pure_.V, sketch_.V),
							new Scope().GoToEnd(S)
						)
						//x is value in the value in pure bucket
						.DeclareVariable(out var x_, sketch_.V.Call<ulong>("Get", pure_.V))
						.IfThenElse(
							answerKeys_SET.Contains(x_.V),
							new Scope().AddExpression(answerKeys_SET.Remove(x_.V)),
							new Scope().AddExpression(answerKeys_SET.Add(x_.V)))

						//Toggle out of sketch x_.V 
						.Macro(out var _,
							hashingFunctions
							.Select(h => S.Function(h, x_.V))
							.Select(hash => S.Action(Toggle, hash, x_.V, sketch_.V)).ToList()
							)
						//Add if hashes look pure for next round
						.Macro(out var _,
							hashingFunctions
								.Select(h => S.Function(h, x_.V))
								.Select(v => S.Action(AddIfLooksPure, v, nextStepPures_.V, sketch_.V)).ToList()
						//This is extremely cursed
						//Actions are added to expression list, thus by explicitly calling .ToList()
						// we are adding these expression into S scope
						)


					);
			return f.Construct();
		}
	}
}
