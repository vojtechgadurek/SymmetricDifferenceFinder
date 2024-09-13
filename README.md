# About
This a little library for finding symmetric differences of large sets in small space.
# How to use
Please download also:
- https://github.com/vojtechgadurek/FlashHash
- https://github.com/vojtechgadurek/LittleSharp
- https://github.com/vojtechgadurek/RedaFasta

They should be downloaded in the same folder. The requirement is dotnet 8.0

For more information, check this text:


## Motivation

We aimed to develop a library to test different approaches for solving set recovery problems, especially finding symmetric differences between two sets. We decided we would like both performance and modifiability, even at the cost of speed and ease of development. Performance is essential because the expected use case may deal with massive datasets. We need modifiability to be able to change or replace components easily. The library may be found here: [SymmetricDifferenceFinder](https://github.com/vojtechgadurek/SymmetricDifferenceFinder).

## Overview

SymmetricDifferenceFinder is our project's core library. It is a small framework for building solutions to the set recovery problem. We will now do a little tour to introduce potential users to the library. The most important concept is a Sketch. One should imagine a sketch as some magic set and some operations with them. These operations may be `Toggle(key)` (adds the key if the key is not in the sketch; otherwise, removes the key) or a combination of `Add(key)` and `Remove(key)`.

When we add some value to the sketch, we should imagine this value being there. However, we do not have access to it. If we remove it, it stops being in the Sketch.

Our implementation of Sketches is a little more low-level. We are going to assume this magical set is a set of buckets, and we may access these buckets by keys. This means the general interface is `Toggle(index, key)`.

For some purposes, it may be useful to implement just the `Add(index, key)`; we call those Tables.

For example, Tables implement this interface:

```csharp
public interface ITable
{
    void Add(Hash key, Key value);
    int Size();
}
```

Sketches should also implement a method for finding symmetric differences between the items they hold and other sketches of the same type. This is important as not all sketches may be compatible.

### Encoding-Decoding Pipeline

There are two main phases: encoding and decoding. During encoding, we encode the values to the sketches. There should be one sketch for each set. During decoding, we first take sketches of sets we want to find symmetric differences, merge them, and then run a decoding algorithm on the result of merging. This step may fail. If it fails, we then need to change the parameters (mostly the size of the table or use some better decoding method if available for the data we have) and rerun the algorithm from the start.

### Hash Functions

Hash functions are used in both phases. One must use the same set of hashing functions for encoding and decoding. This set is expected to implement `IEnumerable<Expression<Func<ulong, ulong>>>` interface. This may, for example, be a `List<Expression<Func<ulong, ulong>>>`. Order is not important. Some basic hashing functions are provided by the FlashHash library. One can also implement their own.

### Encoder

Encoders are used in the encoding phase. There are two implementations of Encoders: `Encoder<TTable>` and `NoConflictEncoder<TTable>`. `TTable` has to implement an `ITable` interface and be a value type (this is to get inline). `NoConflictEncoders` expect that no two hashing functions that have intersecting ranges exist. There are two possible interfaces for implementing `IEncoder` and `IParallelEncoder`.

### Decoders

Decoders are used in the decoding phase. They should be given a sketch and then return decoded values and information on whether decoding was successful. There are two possible decoders: `HPWDecoder`, based on the HPW algorithm, and `HyperGraphDecoder`, which uses standard hypergraph decoding. It should be noted that these two decoders may not be used interchangeably because they need different contracts from Sketch.

### HWPDecoder

`HPWDecoder` is based on the HPW algorithm. It allows the removal of items that are not present in the set or that are not going to be present after the encoding ends. Due to its nature, it cannot distinguish whether some element in the symmetric difference of sets $A$ and $B$ was an element of $A$ or an element of $B$.

### HyperGraphDecoder

It uses an algorithm to find the largest $2$ core of the hypergraph. In its base version, it cannot remove items that are not present in the sketch or that will be present. We also implemented an improved version of such an algorithm that supports such operations.

### Decoders with Oracles

Decoders with oracles are useful if one can guess the existence of some other keys in a sketch from the already decoded ones. Our algorithm expects that such data form an ordered graph. Currently, these methods are provided only for `HPWDecoder`, and such an oracle needs to conform to `IStringFactory` and be a structure. They are especially useful when only a few items remain undecoded. Their effectiveness depends largely on how good our guesses are. However, they are currently more of an experimental feature with a very limited API.

### Data Sources

The library implements multiple possible data sources. `RedaFasta` may be used to provide a MaskedSuperString file as a data source and multiple random data generators are available for testing purposes.

## Building a Custom Solution

This section will describe how to use this library to create a custom solution to set recovery problems. For example, we would like to improve a solution using `HyperGraphDecoder` using a space-saving dictionary.

### Creating a Table

A table needs to conform to the `ITable` interface. We are going to implement a simpler version of the table. We expect that items will only be removed from the Sketch during the decoding phase. This is true if we have two sets $A$ and $B$ and $B \subseteq A$.

```csharp
// Tables need to be structs
struct IBLTTableImprovedMemory: ITable
{
    ulong[] _keySums;
    SpaceSavingDictionary _counts;    
    public IBLTTableImprovedMemory(int size){
        _data = ulong[size];
        _counts = new SpaceSavingDictionary(size);
        _hashSum = new SpaceSavingDictionary(size);
    }
    // Hash is alias for ulong
    // Key is alias for ulong
    // This means writing ulong or Hash or Key is equivalent
    void Add(Hash key, Key value){
        _keySums[key] += value;
        _counts.Add(1);
    }
}
```

### Choosing Hash Functions

After choosing a table or writing its implementation, one should select a set of hashing functions. This set should implement an `IEnumerable<T>` interface, where `T` is `Expression<Func<ulong, ulong>>`. The hash function range should not exceed the table size; otherwise, the program fails during runtime.

```csharp
using FlashHash;
// Simplest solution is this
ulong size = 10;
// This creates an array holding one hash function using modulo
// This approach has a problem with performance
// One should rather use FlashHash library
Expression<Func<ulong, ulong>>[] hfs = [(x) => x % size];
// Solution using FlashHash library
// This solution gets us two Linear Congruence hash function
// with not conflicting ranges
hfs = 
[ 
    HashingFunctionProvider
        .Get(typeof(LinearCongruenceFamily), size / 2, 0).Create(),
    HashingFunctionProvider
        // 5 sets an offset to the hash function
        // It means all values of hashes are going to be increased by 5
        .Get(typeof(LinearCongruenceFamily), size / 2, 5).Create(),
]
```

### Choosing Encoder

The basic library currently offers two encoders: `Encoder` and `NoConflictEncoder`. `NoConflictEncoder` expects that no hash functions intersect in ranges. This allows for some optimizations during encoding; namely, it will enable parallel writing to the table.

```csharp
Expression<Func<ulong, ulong>>[] hfs;
int size = 10;
var config = 
    EncoderConfiguration<IBLTTableImprovedMemory>(hfs, size);
// Factory does most of the compiling, the user
// seeking performance should reuse Factory as much
// as possible
var factory = EncoderFactory<IBLTTableImprovedMemory>(
    config, 
    // This is table factory
    (size) => new IBLTTableImprovedMemory(size)
)
IDecoder decoder = factory.Create()
```

### Using Encoder

The user must write their method to wrap a data source and encoder. Here, we provide an example of using `RedaFasta` to read the files in [SomeFormat].

```csharp
// Opening a [SomeFormat] file
string fastaFilePath = "test.test";
var config = FastaFile.Open(new StreamReader(fastaFilePath));
var reader = new FastaFileReader(
    config.kMerSize, config.nCharsInFile, config.textReader
    );

// Some instances implementing the IEncoder interface
IEncoder encoder;

// It may be a good idea to set the buffer to some size 
// that is large enough
var buffer = new ulong[1024 * 1024];
while (true)
{
    var data = reader.BorrowBuffer();
    if (data is null)
    {
        break;
    }
    // ParallelEncode may speed the encoding
    encoder.ParallelEncode(buffer, data.Size);
    reader.RecycleBuffer(data);
}

encoder.GetTable() // Returns table
```

### Sketch

We should revisit the definition of `IBLTTableImprovedMemory`. We decided to use `HyperGraphDecoder`. `HyperGraphExpect` a sketch to implement `IHyperGraphDecoderSketch<TSketch>`. Now, we have two choices: implement a brand new Sketch and some method transforming Table to the new Sketch, or extend `IBLTTableImpro

vedMemory` to implement `ISketch` interface. We are going to use the second way. We will not implement it entirely, but to show important parts.

```csharp
struct IBLTTableImprovedMemory: 
    ITable,
    IHyperGraphDecoderSketch<IBLTTableImprovedMemory>
{
    ulong[] _keySums;
    SpaceSavingDictionary _counts;    
    public IBLTTableImprovedMemory(int size){
        _data = ulong[size];
        _counts = new SpaceSavingDictionary(size);
        _hashSum = new SpaceSavingDictionary(size);
    }
    void Add(Hash key, Key value){
        _keySums[key] += value;
        _counts.Add(1);
    }
    // This allows the decoding algorithm 
    // to remove items from buckets
    void Remove(Hash key, Key value){
        _keySums[key] -= value;
        _counts.Add(-1);
    } 
    int GetCount(Hash key) => _counts.Get(key);
    // Now, there is a little bit a tricky part
    // we need to implement public looks pure
    // if this method is not defined decoder fails during runtime
    static Expression<Func<ulong, IBLTTable, bool>>
    GetLooksPure(HashingFunctions hashingFunctions)
    {
        var f = CompiledFunctions
                .Create<ulong, IBLTTable, bool>(
                    out var key_, 
                    out var sketch_);
                    
        f.S.Assign(f.Output, false)
            .DeclareVariable(
                out var count_,
                sketch_.V.Call<int>("GetCount", key_.V)
                )
            .IfThen(
                // Bucket is pure when there is only one item 
                // hashed to it
                !(count_.V == 1),
                new Scope().GoToEnd(f.S)
                )
        return f.Construct();
    }
}
```

### Choosing Decoder

If we decided to implement an `IHyperGraphDecoderSketch<TSketch>`, we might use only a decoder that can use such Sketch. There is only one implementation, and this is currently a `HyperGraphDecoder`. We first need to merge the two sketches of the two Sets; we would like to find symmetric differences.

```csharp
// Using a hypergraph decoder
IBLTTableImprovedMemory sketch1;
IBLTTableImprovedMemory sketch2;
// Now we want to merge these into sketches
// Every sketch needs to implement SymmetricDifference()
// So, imagine we implemented such a method
// This implementation could loop something like this:
public IBLTTable SymmetricDifference(IBLTTable other)
{
    if (other.Size() != Size())
    {
        throw new InvalidOperationException(
            "Sketches do not have same sizes"
            );
    }
    IBLTCell[] data = new IBLTCell[Size()];
    _table.CopyTo(data, 0);
    // This means if some a was encoded in A and also B
    // Then it is not encoded in the merged sketch
    for (int i = 0; i < data.Length; i++)
    {
        IBLTCell otherCell = other.GetCell((ulong)i);
        _table[i].KeySum -= otherCell.KeySum;
        _count.Add(i, -otherCell.GetCount(i));
    }
    return new IBLTTable(data);
}
// Now we just need to merge the two sketches
sketch1.SymmetricDifference(sketch2);
```

After we get a merged sketch, we may create a decoder and use it.

```csharp
IEnumerable<Expression<Func<ulong, ulong>> hfs;
var factory = HypergraphFactory(hfs);
var decoder = factory.Create(sketch);
decoder.Decode()
// We may examine the decoding state by
decoder.EncodingState
// This property may have many possible states
// there are three most useful:
// - *Success*
// - *Failure*
// - *Shutdown* (have not finished) 

decoder.GetDecodedValues() 
// Returns a hash set of values in the symmetric difference
// We may get some decoded values even
// when the algorithm is in a fail state.
```

## Buffers Size

The size of buffers may influence performance greatly, especially when using the `RedaFasta` library to read MaskedSuperString files. Something between $1024 \cdot 16$ and $1024 \cdot 1024$ is optimal for our machine. The work with buffers may be parallelized via Tasks to small sizes of buffers that may lead to too large overhead for calling those. For encoders, a number of partitions may be set by calling `SetPartitions(int n)`.

```csharp
Encoder encoder = ...;
// Changes the number of partitions to 8 
encoder.SetPartitions(8);
```

We wrote some basic benchmarks that may be modified to find the optimal parameter setting for concrete machines. However, they were not meant for this purpose in the first place, and some work is to be done.

## Future Work

We would like to write a console application using this library to find Symmetric Differences between files and, more exactly, MaskedSuperString files to provide a more seamless experience for possible users with no need to build their own Symmetric Difference Finder.

Writing code with LittleSharp is still an unpleasant experience. However, we think it is much better than writing code just with Expression Tree. There is a lot of potential for improving LittleSharp and, with it, the overall readability of the code written using LittleSharp.

The abstraction is currently a little bit problematic as it supports only symmetric differences, but IBLT may support a finding in which set each element lies. This would allow Oracle decoders to write using IBLT.

Oracle Decoders are very experimental features, and they would benefit greatly from fewer random constants in the code and greater parametricity. In simple words, some refactoring is required.
  
