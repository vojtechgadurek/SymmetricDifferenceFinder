using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SymmetricDifferenceFinder.Improvements.StringFactories
{
    public struct KMerStringFactory : IStringFactory
    {
        public const int Size = 31;

        public int NPossibleNext => 4;

        public int NPossibleBefore => 4;

        static Random _random = new();

        public ulong[] GetPossibleBefore(ulong value)
        {
            value >>>= 2;
            ulong sizeMask = (1UL << (Size * 2)) - 1UL;

            var answer = new ulong[4];
            for (ulong i = 0; i < 4; i++)
            {
                answer[i] = (((sizeMask & (value << 2)) | i) << 2) | 0b11;
            }
            return answer;
        }

        public ulong[] GetPossibleNext(ulong value)
        {
            value >>>= 2;
            ulong sizeMask = (1UL << (Size * 2)) - 1UL;
            var answer = new ulong[4];
            for (ulong i = 0; i < 4; i++)
            {
                answer[i] = ((sizeMask & (value >> 2)) | (i << (Size * 2 - 2)) << 2) | 0b11;
            }
            return answer;
        }


        public ulong GetRandom()
        {
            ulong value = 0;

            ulong sizeMask = (1UL << (Size * 2)) - 1UL;
            while (value == 0)
            {
                value = (ulong)_random.NextInt64();
            }
            return ((value & sizeMask) << 2) | 0b11;

        }
    }
}
