using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Tables
{
	public interface ITable
	{
		void Add(uint key, ulong value);
		int Size();
	}
}
