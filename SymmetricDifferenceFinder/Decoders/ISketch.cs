using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using HashingFunctions = System.Collections.Generic.IEnumerable<System.Linq.Expressions.Expression<System.Func<ulong, ulong>>>;


namespace SymmetricDifferenceFinder.Decoders
{
	public interface ISketch<TSketch>
	{
		ulong Get(Hash key);

		int Size();
		TSketch SymmetricDifference(TSketch other);

		bool IsEmpty();

		// !!! ISketch has to also declare this method !!!
		// public static Expression<Func<ulong, TSketch, bool>> GetLooksPure(HashingFunctions hashingFunctions);
		// ToDo implement compiler error via analyzers 
	}
}
