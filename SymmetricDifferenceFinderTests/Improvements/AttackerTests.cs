using SymmetricDifferenceFinder.Improvements;
using SymmetricDifferenceFinder.Improvements.StringFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinderTests.Improvements
{
	public class AttackerTests
	{
		[Fact]
		public void BasicTest()
		{
			const ulong size = 10;
			AttackVectorFinder<NextOracle<NumberStringFactory>> Attacker = new((int)size, [(x) => x % size]);
			Attacker.AddNode(1);
			Assert.Single(Attacker.GetBucket(2));

			Attacker.AddNode(size + 1);
			Assert.Contains(size + 2, Attacker.GetBucket(2));

			Attacker.RemoveNode(size + 1);

			Assert.DoesNotContain(size + 2, Attacker.GetBucket(2));

			Assert.ThrowsAny<Exception>(() => { Attacker.RemoveNode(3); });

		}
	}
}
