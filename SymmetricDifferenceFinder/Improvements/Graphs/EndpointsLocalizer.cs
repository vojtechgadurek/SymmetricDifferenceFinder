using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.Graphs
{
	public class EndpointsLocalizer<TOracle>
	where TOracle : struct, IOracle
	{
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
		Dictionary<ulong, Node> _nodes = new Dictionary<ulong, Node>();
		HashSet<ulong> _endPoints = new HashSet<ulong>();
		List<(bool added, ulong id)> _changes = new();


		public void AddNode(ulong id)
		{
			if (_nodes.ContainsKey(id)) _nodes[id].Count++;
			else _nodes.Add(id, Node.NewActiveNode(id));

			foreach (var node in _oracle.GetClose(id))
			{
				WatchNode(node);
			}

			AdjustEndpointState(id);
		}

		void AdjustEndpointState(ulong id)
		{
			var node = _nodes[id];
			if (node.IsEndpoint() && !_endPoints.Contains(id))
			{
				_changes.Add((true, id));
				_endPoints.Add(id);
			}
			if (!node.IsEndpoint() && _endPoints.Contains(id))
			{
				_changes.Add((false, id));
				_endPoints.Remove(id);
			}
		}

		void WatchNode(ulong id)
		{
			if (!_nodes.ContainsKey(id))
			{
				_nodes.Add(id, Node.NewInactiveNode(id));
			};
			var node = _nodes[id];
			node.WatchedBy++;
			AdjustEndpointState(id);
		}

		public void RemoveNode(ulong id)
		{
			_nodes[id].Count--;
			foreach (var node in _oracle.GetClose(id))
			{
				UnWatchNode(node);
			}
			AdjustEndpointState(id);

		}

		void UnWatchNode(ulong id)
		{
			var node = _nodes[id];
			_nodes[id].WatchedBy--;
			AdjustEndpointState(id);
		}


		public bool ContainsNode(ulong id)
		{
			if (!_nodes.ContainsKey(id)) return false;
			if (_nodes[id].Count > 0) return true;
			return false;
		}

		public bool IsEndpoint(ulong id)
		{
			return _endPoints.Contains(id);
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
