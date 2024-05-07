using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Decoders.HPW
{
	public interface IHPWSketch<T> : ISketch<T>
	{
		void Toggle(Hash hash, Key value);
	}
}
