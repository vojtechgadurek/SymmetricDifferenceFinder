using Microsoft.Diagnostics.Tracing.Parsers.ApplicationServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SymmetricDifferenceFinder.Combinations;

namespace SymmetricDifferenceFinder.Combinations
{

    public record HashFunctionTemplate(double Size = 1, double Filter = 0)
    {

    }

    public class HashingFunctionCombinations
    {
        public static HashingFunctionCombination GetFromSameFamily(int number, IHashFunctionFamily family, params Func<Expression<HashingFunction>, Expression<HashingFunction>>[] modificators)
        {
            var answer = new HashingFunctionCombination();
            for (int i = 0; i < number; i++)
            {
                answer.AddHashingFunction((size, offset) => family.GetScheme(size, offset).Create(), modificators);
            }
            return answer;
        }

        public static HashingFunctionCombination GetFromSameFamilyLastWeaker(int number, IHashFunctionFamily family)
        {
            var answer = new HashingFunctionCombination();
            for (int i = 0; i < number - 1; i++)
            {
                answer.AddHashingFunction((size, offset) => family.GetScheme(size, offset).Create());
            }

            answer.AddHashingFunction((size, offset) => family.GetScheme(size, offset).Create(),
                (h) => Utils.HashingFunctionFilter.Filter(h, (7, 29), 0));

            return answer;
        }

        public static HashingFunctionCombination GetFromMultipleFamilies(IHashFunctionFamily[] families, params Func<Expression<HashingFunction>, Expression<HashingFunction>>[] modificators)
        {
            var answer = new HashingFunctionCombination();
            foreach (var family in families)
            {
                answer.AddHashingFunction((size, offset) => family.GetScheme(size, offset).Create(), modificators);
            }
            return answer;
        }

        public static Expression<Func<ulong, ulong>> Quadratic(Expression<Func<ulong, ulong>> hashFunction)
        {
            var f = CompiledFunctions.Create<ulong, ulong>(out var input_);
            f.S.Assign(f.Output, f.S.Function(hashFunction, input_.V * input_.V));
            return f.Construct();
        }
    }
}
