using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.Oracles
{

	public struct CanonicalOrder : IPipeline
	{

		public ulong Other(ulong value)
		{
			ulong newValue = 0;
			ulong mask = 0b11;
			for (ulong i = 0; 31 > i; i++)
			{
				newValue <<= 2;
				newValue |= (0b11 - (value & mask));
				value >>= 2;
			}
			return newValue;
		}

		public ulong Modify(ulong value)
		{
			//return value;
			ulong other = Other(value);
			if (other > value) return value;
			return other;
		}

	}

}
