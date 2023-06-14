using System.Collections.Generic;
using Perun.Differ;

namespace Tests.Perun.Differ.TestTypes
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