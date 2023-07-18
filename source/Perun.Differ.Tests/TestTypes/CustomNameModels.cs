using System.Collections.Generic;

namespace Differ.DotNet.Tests.TestTypes
{
    internal class CustomNameModel
    {
        [DiffPropertyName("ACustomName")]
        public string A { get; set; }

        public string B => "MyCustomName";
    }

    internal class CustomNameReflectionModel
    {
        [DiffPropertyName("B", fromPropertyValue: true)]
        public string A { get; set; }

        public string B => "MyCustomName";
    }

    internal class CustomNameNullValueReflectionModel
    {
        [DiffPropertyName("B", fromPropertyValue: true)]
        public double? A { get; set; }

        public string B => "MyCustomName";
    }

    internal class CustomNameNullValueReflectionModelNested
    {
        public List<CustomNameNullValueReflectionModel> Items { get; set; }
    }

    internal class CustomNameNestedModel
    {
        [DiffPropertyName("BCustomName")]
        public CustomNameModel B { get; set; }
    }

    internal class CustomNameNestedReflectionModel
    {
        [DiffPropertyName("B.B", fromPropertyValue: true)]
        public CustomNameModel B { get; set; }
    }

    internal class CustomNameDeepNestedReflectionModel
    {
        [DiffPropertyName("B.B.B", fromPropertyValue: true)]
        public CustomNameNestedReflectionModel B { get; set; }
    }
}