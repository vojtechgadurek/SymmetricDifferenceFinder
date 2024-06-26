﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Decoders
{
	public interface IDecoderFactory<TSketch>
	{
		IDecoder Create(TSketch sketch);
	}
}
