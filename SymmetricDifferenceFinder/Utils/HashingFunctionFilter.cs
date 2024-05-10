using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Utils
{
	public static class HashingFunctionFilter
	{
		public static Expression<HashingFunction> Filter(Expression<HashingFunction> hf, (ulong use, ulong notUse) ratio, ulong valueWhenNotUse)
		{
			var f = CompiledFunctions.Create<ulong, ulong>(out var value_);
			f.S.Assign(f.Output, valueWhenNotUse)
				.DeclareVariable(out var hash_, f.S.Function(hf, value_.V))
				.Macro(out var test_, hash_.V % (ratio.use + ratio.notUse))
				.IfThen(test_ < ratio.use, new Scope().Assign(f.Output, hash_.V))
				;
			return f.Construct();
		}
	}
}
