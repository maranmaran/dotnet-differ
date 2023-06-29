using System.Collections.Generic;

namespace Differ.DotNet.Tests.TestTypes
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

    public class SimpleOptionalKeepModel
    {
        [KeepInDiff(IgnoreIfNoSiblingOrChildDiff = true)]
        public string NoDiffKeepMe { get; set; }

        public string NoDiff { get; set; }
    }

    public class ComplexOptionalKeepModel
    {
        public ComplexOptionalKeepModel()
        {
        }

        public ComplexOptionalKeepModel(ComplexOptionalKeepModel a)
        {
            KeepMeOnlyIfSiblingIsDiffed = a.KeepMeOnlyIfSiblingIsDiffed;
            Sibling = a.Sibling;
        }

        [KeepInDiff(IgnoreIfNoSiblingOrChildDiff = true)]
        public string KeepMeOnlyIfSiblingIsDiffed { get; set; }

        public string Sibling { get; set; }
    }

    public class IterableComplexOptionalKeepModel
    {
        public List<ComplexOptionalKeepModel> Iterable { get; set; }
    }
}