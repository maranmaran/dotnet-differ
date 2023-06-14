using System.Collections.Generic;
using Perun.Differ;

namespace Tests.Perun.Differ.TestTypes
{
    public class SimpleKeepModel
    {
        [KeepInDiff]
        public string NoDiffKeepMe { get; set; }

        public string NoDiff { get; set; }
    }

    public class IterableSimpleKeepModel
    {
        [KeepInDiff]
        public IEnumerable<string> NoDiffKeepMe { get; set; }

        public IEnumerable<string> NoDiff { get; set; }
    }

    public class IterableComplexKeepModel
    {
        [KeepInDiff]
        public IEnumerable<ComplexType> NoDiffKeepMe { get; set; }

        public IEnumerable<ComplexType> NoDiff { get; set; }
    }

    public class ComplexKeepModel
    {
        [KeepInDiff]
        public ComplexType NoDiffKeepMe { get; set; }

        public ComplexType NoDiff { get; set; }
    }
}