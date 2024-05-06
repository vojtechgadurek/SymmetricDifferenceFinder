using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Decoders
{
	public interface ISketch
	{
		ulong Get(Hash key);

		int Size { get; }
		ISketch SymmetricDifference(ISketch other);

		bool IsEmpty();
	}

}
