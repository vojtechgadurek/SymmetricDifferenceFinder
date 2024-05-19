using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements
{
	public class Massager : IDecoder
	{
		HPWDecoder<XORTable> _HPWDecoder;
		XORTable _table;
		Random _random = new Random();
		int _size;
		IEnumerable<Func<ulong, ulong>> _hfs;
		public DecodingState DecodingState => _HPWDecoder.DecodingState;
		private readonly HashingFunction _getClose;

		public Massager(HPWDecoder<XORTable> HPWDecoder, HashingFunction getCLose, XORTable table, IEnumerable<HashingFunction> hfs)
		{
			_size = table.Size();
			_table = table;
			_hfs = hfs;
			_getClose = getCLose;
		}

		public void Decode()
		{
			int numberOfMassing = _size;

			while (numberOfMassing > 0)
			{
				var values = _HPWDecoder.GetDecodedValues().Where(_ => _random.NextDouble() < 0.05).Select(_getClose).ToList();
				foreach (var hf in _hfs)
				{
					foreach (var value in values)
					{
						_table.Toggle(hf(value), value);
					}
				}
				_HPWDecoder.Decode();
				foreach (var hf in _hfs)
				{
					foreach (var value in values)
					{
						_table.Toggle(hf(value), value);
					}
				}
			}

		}


		public HashSet<ulong> GetDecodedValues()
		{
			return _HPWDecoder.GetDecodedValues();
		}
	}
}
