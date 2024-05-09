using SymmetricDifferenceFinderTests.Decoder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinderTests.Decoder
{
	static class HashingFunctions
	{
		public static Expression<Func<ulong, ulong>> GetModuloWithOffset(ulong offset, ulong size) => (x) => (x + offset) % size;

	}

	public static class HashingFunctionsCombinations
	{
		static public List<(ulong, IEnumerable<Expression<Func<ulong, ulong>>>)> GetValues(ulong size) =>
			new List<(ulong, IEnumerable<Expression<Func<ulong, ulong>>>)>()
			{
				(size, new[] { HashingFunctions.GetModuloWithOffset(0, size) }),
				(size, new[] { HashingFunctions.GetModuloWithOffset(0, size), HashingFunctions.GetModuloWithOffset(1, size)}),
				(size, new[] { HashingFunctions.GetModuloWithOffset(0, size), HashingFunctions.GetModuloWithOffset(0, 1) })
			};
	}

	public static class DecoderSketchCombinations
	{

		static public List<(Func<int, ITable> tableFactory, Func<ITable, IEnumerable<Expression<Func<ulong, ulong>>>, IDecoder> decoderFactory)> GetValues()
			=> new List<(Func<int, ITable>, Func<ITable, IEnumerable<Expression<Func<ulong, ulong>>>, IDecoder>)>()
			{
				((int size) => new IBLTTable(size),
				(ITable sketch, IEnumerable<Expression<Func<ulong, ulong>>> hfs) => (new HyperGraphDecoderFactory<IBLTTable>(hfs).Create((IBLTTable)sketch))
				),
				((int size) => new XORTable(size),
				(ITable sketch, IEnumerable<Expression<Func<ulong, ulong>>> hfs) => (new HPWDecoderFactory<XORTable>(hfs).Create((XORTable)sketch))
				)
			};
	}
}

