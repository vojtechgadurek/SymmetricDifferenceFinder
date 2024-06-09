using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.StringFactories
{
	public interface IStringFactory
	{
		int NPossibleNext { get; }
		int NPossibleBefore { get; }
		ulong[] GetPossibleNext(ulong value);
		ulong[] GetPossibleBefore(ulong value);
	}
}
