using System.Collections.Generic;
using Perun.Differ;

namespace Tests.Perun.Differ.TestTypes
{
    public class KeepParentIgnoreChildModel
    {
        [KeepInDiff]
        public IEnumerable<KeepParentIgnoreChild> IterableKeepParentIgnoreChildProp { get; set; }

        [KeepInDiff]
        public KeepParentIgnoreChild KeepParentIgnoreChildProp { get; set; }

        public class KeepParentIgnoreChild
        {
            public string KeepMe { get; set; }

            [IgnoreInDiff]
            public string IgnoreMe { get; set; }
        }
    }

    public class IgnoreParentKeepChildModel
    {
        [IgnoreInDiff]
        public IEnumerable<IgnoreParentKeepChild> IterableIgnoreParentKeepChildProp { get; set; }

        [IgnoreInDiff]
        public IgnoreParentKeepChild IgnoreParentKeepChildProp { get; set; }

        public class IgnoreParentKeepChild
        {
            [KeepInDiff]
            public string KeepMe { get; set; }

            public string IgnoreMe { get; set; }
        }
    }
}