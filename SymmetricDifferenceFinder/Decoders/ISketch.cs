﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Decoders
{
	public interface ISketch<T>
	{
		ulong Get(Hash key);

		int Size { get; }
		T SymmetricDifference(T other);

		bool IsEmpty();
	}

}
