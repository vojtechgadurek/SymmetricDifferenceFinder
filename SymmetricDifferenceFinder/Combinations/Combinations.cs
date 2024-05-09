using Microsoft.Diagnostics.Tracing.Stacks;
using SymmetricDifferenceFinder.Decoders.HPW;
using SymmetricDifferenceFinder.Decoders.HyperGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Combinations
{
	public static class Combinations
	{
		public static CombinationConfiguration<IBLTTable, IBLTTable> IBLT =
			new CombinationConfiguration<IBLTTable, IBLTTable>()
			.SetTableFactory(x => new IBLTTable(x))
			.SetDecoderFactoryFactory(hfs => new HyperGraphDecoderFactory<IBLTTable>(hfs))
			.SetTableToSketch(x => x);

		public static CombinationConfiguration<XORTable, XORTable> CombinationConfiguration =
			new CombinationConfiguration<XORTable, XORTable>()
			.SetTableFactory(x => new XORTable(x))
			.SetDecoderFactoryFactory(hfs => new HPWDecoderFactory<XORTable>(hfs))
			.SetTableToSketch(x => x);
	}
}
