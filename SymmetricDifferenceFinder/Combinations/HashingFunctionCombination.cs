using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SymmetricDifferenceFinder.Combinations
{
    public class HashingFunctionCombination
    {
        public record class HashingFunctionConfiguration(
            Func<ulong, ulong, Expression<HashingFunction>> factory,
            IEnumerable<Func<Expression<HashingFunction>, Expression<HashingFunction>>> Modificators
            );

        List<HashingFunctionConfiguration> _hashingFunctions = new List<HashingFunctionConfiguration>();

        public HashingFunctionCombination AddHashingFunction(Func<ulong, ulong, Expression<HashingFunction>> factory, params Func<Expression<HashingFunction>, Expression<HashingFunction>>[] modificators)
        {
            _hashingFunctions.Add(new HashingFunctionConfiguration(factory, modificators));
            return this;
        }

        public Func<int, IEnumerable<Expression<HashingFunction>>> GetFactory()
        {
            var factory = (int size) =>
            {
                var result = new List<Expression<HashingFunction>>();
                foreach (var hf in _hashingFunctions)
                {
                    var hf_ = hf.factory((ulong)size, 0);
                    foreach (var mod in hf.Modificators)
                    {
                        hf_ = mod(hf_);
                    }
                    result.Add(hf_);
                }
                return result;
            };
            return factory;
        }


        public Func<int, IEnumerable<Expression<HashingFunction>>> GetNoConflictFactory()
        {
            var factory = (int size) =>
            {
                var result = new List<Expression<HashingFunction>>();
                for (int i = 0; i < _hashingFunctions.Count; i++)
                {
                    var length = (ulong)size / (ulong)_hashingFunctions.Count;
                    var hf_ = _hashingFunctions[i].factory(length, (ulong)i * length);
                    foreach (var mod in _hashingFunctions[i].Modificators)
                    {
                        hf_ = mod(hf_);
                    }
                    result.Add(hf_);
                }
                return result;
            };
            return factory;
        }




    }
}
