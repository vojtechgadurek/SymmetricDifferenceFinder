using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Encoders
{
	public interface IEncoderFactory<TTable> where TTable : ITable
	{
		IEncoder Create(TTable table);
	}
}
