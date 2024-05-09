open LittleSharp
open FlashHash
open FlashHash.SchemesAndFamilies
open SymmetricDifferenceFinder
open SymmetricDifferenceFinder.Decoders
open SymmetricDifferenceFinder.Encoders

// For more information see https://aka.ms/fsharp-console-apps


type OneTestData<'TTable, 'TSketch> = {
    encoder: IEncoder
    decoder: IDecoder
    data: uint64 Set
    x: uint64
}



let RunOneTest<'TTable, 'TSketch> (testData: OneTestData<'TTable,'TSketch>) = testData.encoder.Encode(testData.data, testData.data.Count) |> ignore 