using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.Graphs
{
	//THIS CLASS IS BUGGY - read all comments

	public interface IBool
	{
		bool Value();
	}
	public struct True : IBool
	{
		public bool Value() => true;
	}

	public struct False : IBool
	{
		public bool Value() => false;
	}


	public class EndpointsLocalizer<TOracle, TSwitch>
	where TSwitch : struct, IBool
	where TOracle : struct, IOracle
	{
		static readonly TSwitch removableNodes = default;
		public class Node
		{
			public readonly ulong Id;
			public int Count = 0;

			public int WatchedBy = 0;

			public Node(ulong id, int count, int watchedBy)
			{
				Id = id;
				Count = count;
				WatchedBy = watchedBy;
			}

			public bool IsEndpoint() => WatchedBy == 0 && Count > 0;

			public static Node NewActiveNode(ulong id) => new Node(id, 1, 0);
			public static Node NewInactiveNode(ulong id) => new Node(id, 0, 0);

		}
		TOracle _oracle = default;
		public Dictionary<ulong, Node> Nodes = new Dictionary<ulong, Node>();
		public readonly HashSet<ulong> EndPoints = new HashSet<ulong>();
		List<(bool added, ulong id)> _changes = new();


		public void AddNode(ulong id)
		{
			if (Nodes.ContainsKey(id)) Nodes[id].Count++;
			else Nodes.Add(id, Node.NewActiveNode(id));

			if (Nodes[id].Count < 1) return;
			foreach (var node in _oracle.GetClose(id))
			{
				WatchNode(node);
			}

			AdjustEndpointState(id);
		}

		void AdjustEndpointState(ulong id)
		{
			var node = Nodes[id];
			if (node.IsEndpoint() && !EndPoints.Contains(id))
			{
				_changes.Add((true, id));
				EndPoints.Add(id);
			}
			if (!node.IsEndpoint() && EndPoints.Contains(id))
			{
				_changes.Add((false, id));
				EndPoints.Remove(id);
			}
		}

		void WatchNode(ulong id)
		{
			if (!Nodes.ContainsKey(id))
			{
				Nodes.Add(id, Node.NewInactiveNode(id));
			};
			var node = Nodes[id];
			node.WatchedBy++;
			AdjustEndpointState(id);
		}

		public void RemoveNode(ulong id)
		{
			if (!removableNodes.Value()) return;
			//if (!Nodes.ContainsKey(id)) Nodes[id] = Node.NewInactiveNode(id);
			Nodes[id].Count--;

			//THIS IS BUG is should be Nodes[id].Count < 0 not Nodes[id].Count < 1
			//WE PLAN TO DO SOME REWRITE
			//AS WE INVESTIGATE
			//BUT THIS FOR SOME REASON CAUSES BETTER RECOVERY
			if (Nodes[id].Count < 0) return;
			foreach (var node in _oracle.GetClose(id))
			{
				UnWatchNode(node);
			}
			AdjustEndpointState(id);

		}

		void UnWatchNode(ulong id)
		{
			Nodes[id].WatchedBy--;
			AdjustEndpointState(id);
		}


		public bool ContainsNode(ulong id)
		{
			if (!Nodes.ContainsKey(id)) return false;
			if (Nodes[id].Count > 0) return true;
			return false;
		}

		public bool IsEndpoint(ulong id)
		{
			return EndPoints.Contains(id);
		}

		public (bool added, ulong id)? GetChanged()
		{
			if (_changes.Count == 0) return null;
			var answer = _changes[_changes.Count - 1];
			_changes.RemoveAt(_changes.Count - 1);
			return answer;
		}

	}
}
