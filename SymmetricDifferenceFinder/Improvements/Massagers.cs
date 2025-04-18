﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Diagnostics.Tracing.Analysis;
using Microsoft.Diagnostics.Tracing.Parsers.IIS_Trace;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftAntimalwareEngine;
using SymmetricDifferenceFinder.Decoders.HPW;
using SymmetricDifferenceFinder.Improvements.Oracles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SymmetricDifferenceFinder.Improvements
{

    public class MassagerFactory<TStringFactory, TPipeline> : IDecoderFactory<XORTable>
        where TStringFactory : struct, IStringFactory
        where TPipeline : struct, IPipeline
    {
        HPWDecoderFactory<XORTable> _decoderFactory;
        IEnumerable<HashingFunction> _hfs;
        public MassagerFactory(IEnumerable<Expression<HashingFunction>> hfs, HPWDecoderFactory<XORTable> factory)
        {
            _decoderFactory = factory;
            _hfs = hfs.Select(f => f.Compile()).ToList();
        }
        public IDecoder Create(XORTable sketch)
        {
            return new Massager<TStringFactory, TPipeline>(_decoderFactory.Create(sketch), _hfs);
        }
    }

    public class Massager<TStringFactory, TPipeline> : IDecoder
        where TStringFactory : struct, IStringFactory
        where TPipeline : struct, IPipeline
    {
        readonly public HPWDecoder<XORTable> HPWDecoder;
        readonly XORTable _table;
        readonly Random _random = new Random();
        readonly int _size;
        readonly List<Func<ulong, ulong>> _hfs;
        readonly HashSet<ulong> _decodedValues;


        public int NStepsDecoderInitial;
        public int NStepsDecoder;
        public int NStepsRecovery;

        PickOneRandomly<Pipeline<BeforeOracle<TStringFactory>, TPipeline>> _before = new();
        PickOneRandomly<Pipeline<NextOracle<TStringFactory>, TPipeline>> _next = new();

        public DecodingState DecodingState => HPWDecoder.DecodingState;

        public Massager(HPWDecoder<XORTable> HPWDecoder, IEnumerable<HashingFunction> hfs, int nStepsDecoderInitial = 1000, int nStepsDecoder = 10000, int nStepsRecovery = 100)
        {
            _table = HPWDecoder.Sketch;
            _size = _table.Size();
            _hfs = hfs.ToList();

            NStepsDecoderInitial = nStepsDecoderInitial;
            NStepsDecoder = nStepsDecoder;
            NStepsRecovery = nStepsRecovery;



            this.HPWDecoder = HPWDecoder;
            _decodedValues = this.HPWDecoder.GetDecodedValues();
        }
        public void ToggleValues(HashSet<ulong>? possiblePures, List<ulong> values)
        {
            foreach (var hf in _hfs)
            {
                foreach (var value in values)
                {
                    var hash = hf(value);
                    _table.Toggle(hash, value);
                    if (possiblePures is not null)
                    {
                        possiblePures.Add(hash);
                    }
                }
            }
            ToggleDecodedValues(values);
        }

        List<ulong> GetCloseToDecoded(double probability)
        {
            List<ulong> values = new();
            foreach (var value in _decodedValues)
            {
                if (_random.NextDouble() > probability)
                {
                    var x = _before.GetRandom(value);
                    values.Add(x);
                    //values.Add(_before.GetRandom(x));
                }

            }
            foreach (var value in _decodedValues)
            {
                if (_random.NextDouble() > probability)
                {
                    var x = _next.GetRandom(value);
                    values.Add(x);
                    //values.Add(_next.GetRandom(x));
                }
            }
            return values;
        }
        public void FindPure(HashSet<ulong> pure)
        {
            for (ulong i = 0; i < (ulong)_size; i++)
            {
                pure.Add(i);
            }
        }
        public void ToggleDecodedValues(IEnumerable<ulong> values)
        {
            foreach (var value in values)
            {
                if (_decodedValues.Contains(value))
                {
                    _decodedValues.Remove(value);
                }
                else
                {
                    _decodedValues.Add(value);
                }
            }
        }

        public void Massage(List<ulong> values, HashSet<ulong> decodedValues, HashSet<ulong> pure, HashSet<ulong> nextPure)
        {
            ToggleValues(pure, values);

            HPWDecoder.OuterDecode(pure, nextPure, decodedValues);

            //ToggleDecodedValues(decodedValues);
            //decodedValues.Clear();

            ToggleValues(nextPure, values);

            pure.Clear();

            HPWDecoder.OuterDecode(nextPure, pure, decodedValues);
            ToggleDecodedValues(decodedValues);
            //Console.WriteLine(decodedValues.Count);
            decodedValues.Clear();

            nextPure.Clear();
        }
        interface IAction<T>
        {
            void Do(T a);
        }
        public void Decode()
        {
            HPWDecoder.MaxNumberOfIterations = NStepsDecoderInitial;
            HPWDecoder.Decode();
            HPWDecoder.MaxNumberOfIterations = NStepsDecoder;

            HashSet<ulong> pure = HPWDecoder.GetPure();


            HashSet<ulong> nextPure = new HashSet<ulong>();
            HashSet<ulong> decodedValues = new HashSet<ulong>();

            int maxRounds = NStepsRecovery;
            for (int i = 0; i < maxRounds; i++)
            {
                if (HPWDecoder.DecodingState == DecodingState.Success)
                {
                    //Console.WriteLine(i);
                    break;
                }
                //BinPackingDecode();

                List<ulong> values;
                values = GetCloseToDecoded(0.5);

                Massage(values, decodedValues, pure, nextPure);


                if (i == maxRounds - 1)
                {
                    //Console.WriteLine(i);
                }
            }
        }
        public HashSet<ulong> GetDecodedValues()
        {
            return _decodedValues;
        }
    }
}
