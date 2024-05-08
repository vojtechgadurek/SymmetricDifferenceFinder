using System;
using System.Collections;
using System.Collections.Generic;
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

	public class HashingFunctionsCombinations : IEnumerable<object[]>
	{

		public IEnumerator<object[]> GetEnumerator()
		{
			ulong size = 10;
			yield return new object[] { size, HashingFunctions.GetModuloWithOffset(0, size) };

			yield return new object[] { size, HashingFunctions.GetModuloWithOffset(0, size), HashingFunctions.GetModuloWithOffset(1, size) };

			yield return new object[] { size, HashingFunctions.GetModuloWithOffset(0, size), HashingFunctions.GetModuloWithOffset(0, 0) };

		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public class DecoderSketchCombinations
	{

	}
}

