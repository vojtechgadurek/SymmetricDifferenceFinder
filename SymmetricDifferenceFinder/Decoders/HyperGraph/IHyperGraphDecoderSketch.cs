using SymmetricDifferenceFinder.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Decoders.HyperGraph
{
	public interface IHyperGraphDecoderSketch<TSketch> : ISketch<TSketch>
	{
		public void Add(Hash key, Key value);
		public void Remove(Hash key, Key value);

		public void GetCount(Hash key);

		public void GetHashCheck(Hash key);
	}
}
