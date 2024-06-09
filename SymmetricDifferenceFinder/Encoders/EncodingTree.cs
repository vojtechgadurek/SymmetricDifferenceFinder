using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Encoders
{
	public interface IEncodingTree
	{
		void Encode(Key[] keys, int start, int count);
	}

	public interface IWriteNode<TValue>
	{
		void Write(Hash[] hashes, TValue[] values, int start, int count);
	}

	public interface IHashNode
	{
		void Hash(Key[] key, Hash[] hashes, int start, int count);
	}

	public interface IWriteAction<TValue>
	{
		void Invoke(Hash[] hashes, TValue[] values, int start, int count);
	}


	public struct WriteNode<TValue, TWriteAction> : IWriteNode<TValue>
		where TWriteAction : struct, IWriteAction<TValue>
	{
		static readonly TWriteAction writeAction = default;
		public void Write(ulong[] hashes, TValue[] values, int start, int count)
		{
			writeAction.Invoke(hashes, values, start, count);
		}
	}

	public struct HashNode : IHashNode
	{
		readonly Action<Key[], Hash[], int, int> hash;
		public HashNode(Action<Key[], Hash[], int, int> hash)
		{
			this.hash = hash;
		}
		public void Hash(Key[] keys, Hash[] hashes, int start, int count)
		{
			hash(keys, hashes, start, count);
		}
	}

}