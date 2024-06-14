using SymmetricDifferenceFinder.Improvements;
using SymmetricDifferenceFinder.Improvements.StringFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SymmetricDifferenceFinder.Decoders.HyperGraph;
using SymmetricDifferenceFinder.Improvements.Graphs;
using SymmetricDifferenceFinder.Improvements.Oracles;
namespace SymmetricDifferenceFinderTests.Improvements
{
	public class EndpointsTest
	{
		[Fact]
		public void SimpleTest()
		{
			var endpoints = new EndpointsLocalizer<NextOracle<NumberStringFactory>>();
			endpoints.AddNode(1);
			Assert.True(endpoints.IsEndpoint(1));
			endpoints.AddNode(1);
			Assert.True(endpoints.IsEndpoint(1));
			endpoints.RemoveNode(1);
			Assert.True(endpoints.IsEndpoint(1));
			endpoints.RemoveNode(1);
			Assert.False(endpoints.IsEndpoint(1));

			endpoints.AddNode(1);
			endpoints.AddNode(2);
			endpoints.AddNode(3);

			Assert.True(endpoints.IsEndpoint(1));
			Assert.False(endpoints.IsEndpoint(2));
			Assert.False(endpoints.IsEndpoint(3));

			endpoints.AddNode(4);
			Assert.False(endpoints.IsEndpoint(3));
			Assert.False(endpoints.IsEndpoint(4));

			endpoints.RemoveNode(2);
			Assert.True(endpoints.IsEndpoint(1));
			Assert.False(endpoints.IsEndpoint(2));
			Assert.True(endpoints.IsEndpoint(3));
		}

		[Fact]
		public void SimpleTest2()
		{
			var endpoints = new EndpointsLocalizer<NextOracle<KMerStringFactory>>();
			endpoints.AddNode(1);
			Assert.True(endpoints.IsEndpoint(1));
			endpoints.AddNode(1);
			Assert.True(endpoints.IsEndpoint(1));
			endpoints.RemoveNode(1);
			Assert.True(endpoints.IsEndpoint(1));
			endpoints.RemoveNode(1);
			Assert.False(endpoints.IsEndpoint(1));

			endpoints.AddNode(1);
			endpoints.AddNode(2);
			endpoints.AddNode(3);

			Assert.True(endpoints.IsEndpoint(1));
			Assert.False(endpoints.IsEndpoint(2));
			Assert.False(endpoints.IsEndpoint(3));

			endpoints.AddNode(4);
			Assert.False(endpoints.IsEndpoint(3));
			Assert.False(endpoints.IsEndpoint(4));

			endpoints.RemoveNode(2);
			Assert.True(endpoints.IsEndpoint(1));
			Assert.False(endpoints.IsEndpoint(2));
			Assert.True(endpoints.IsEndpoint(3));
		}
	}
}
