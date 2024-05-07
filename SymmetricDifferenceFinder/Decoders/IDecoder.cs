using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SymmetricDifferenceFinder.Decoders.Common;

namespace SymmetricDifferenceFinder.Decoders
{
	public interface IDecoder
	{
		void Decode();
		HashSet<Key> GetDecodedValues();

		DecodingState DecodingState { get; }
	}
}
