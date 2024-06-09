using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.Oracles
{
	public struct NextOracle<TStringFactory> : IOracle
		where TStringFactory : struct, IStringFactory
	{
		TStringFactory _stringFactory = default;

		public NextOracle()
		{

		}
		public ulong[] GetClose(ulong id)
		{
			return _stringFactory.GetPossibleNext(id);
		}
	}

	public struct BeforeOracle<TStringFactory> : IOracle
		where TStringFactory : struct, IStringFactory
	{
		TStringFactory _stringFactory = default;

		public BeforeOracle()
		{

		}
		public ulong[] GetClose(ulong id)
		{
			return _stringFactory.GetPossibleBefore(id);
		}
	}
}
