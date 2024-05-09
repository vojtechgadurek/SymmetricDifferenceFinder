﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Encoders
{
	public interface IEncoder
	{
		public void Encode(ulong[] buffer, int nItemsInBuffer);
	}
}
