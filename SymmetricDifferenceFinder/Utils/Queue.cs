using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Utils
{
	public class ListQueue<T>
	{
		List<T> _values;

		public int Count { get { return _values.Count; } }
		public ListQueue()
		{
			_values = new List<T>();
		}
		public ListQueue(int capacity)
		{
			_values = new List<T>(capacity);
		}

		public void Add(T item)
		{
			_values.Add(item);
		}

		public T Dequeue()
		{
			var last = _values[Count - 1];
			_values.RemoveAt(Count - 1);
			return last;
		}



	}
}
