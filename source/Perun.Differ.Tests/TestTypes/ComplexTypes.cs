using System.Collections.Generic;

namespace Differ.DotNet.Tests.TestTypes
{
    public class ComplexType
    {
        public string String { get; set; }
    }

    public class NestedComplexType
    {
        public ComplexType Nested { get; set; }
    }

    public class DictionaryOfComplexType
    {
        [DiffCollectionId("Key")]
        public Dictionary<string, ComplexType> Data { get; set; } = new();
    }
}