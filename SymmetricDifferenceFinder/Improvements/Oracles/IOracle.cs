using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.Oracles
{
	public interface IOracle
	{
		ulong[] GetClose(ulong id);
		int Size();
	}
}
