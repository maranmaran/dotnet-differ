using System.Collections.Generic;

namespace DotNet.Differ.Tests.TestTypes
{
    public class SimpleIgnoreModel
    {
        [IgnoreInDiff]
        public string IgnoreMe { get; set; }
    }

    public class IterableSimpleIgnoreModel
    {
        [IgnoreInDiff]
        public IEnumerable<string> IgnoreMe { get; set; }
    }

    public class IterableComplexIgnoreModel
    {
        [IgnoreInDiff]
        public IEnumerable<ComplexType> IgnoreMe { get; set; }
    }

    public class ComplexIgnoreModel
    {
        [IgnoreInDiff]
        public ComplexType IgnoreMe { get; set; }
    }
}