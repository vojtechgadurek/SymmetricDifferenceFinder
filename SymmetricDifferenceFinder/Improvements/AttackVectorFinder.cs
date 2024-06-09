using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Diagnostics.Tracing.Parsers.AspNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements;




public class AttackVectorFinder<TOracle>
	where TOracle : struct, IOracle
{
	public HyperGraph Attacks;
	EndpointsLocalizer<TOracle> _endpoints = new EndpointsLocalizer<TOracle>();
	TOracle _oracle = default;
	List<Func<ulong, ulong>> _hfs;

	public AttackVectorFinder(int size, List<Func<ulong, ulong>> hfs)
	{
		Attacks = new HyperGraph((ulong)size);
		_hfs = hfs;
	}

	public AttackVectorFinder(int size, List<Func<ulong, ulong>> hfs, HyperGraph hyperGraph)
	{
		Attacks = hyperGraph;
		_hfs = hfs;
	}
	public void AddNode(ulong id)
	{
		_endpoints.AddNode(id);

		AdjustChanges();
	}

	void AdjustChanges()
	{
		while (true)
		{
			var changed = _endpoints.GetChanged();
			if (changed is null) return;
			var (added, node) = ((bool, ulong))changed;

			var closeNodes = _oracle.GetClose(node);

			if (added == true)
			{
				foreach (var id in closeNodes)
				{
					Attacks.AddEdge(id, _hfs.Select(f => f(id)).ToArray());
				}
			}
			else
			{
				foreach (var id in closeNodes)
				{
					Attacks.RemoveEdge(id, _hfs.Select(f => f(id)).ToArray());
				}
			}
		}
	}
	public void RemoveNode(ulong id)
	{
		ulong[] close = _oracle.GetClose(id);
		_endpoints.RemoveNode(id);
		AdjustChanges();
	}

	public HashSet<ulong> GetBucket(ulong id)
	{
		return Attacks.GetBucket(id);
	}
}
