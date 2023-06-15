namespace DotNet.Differ.Tests.TestTypes
{
    internal class CustomNameModel
    {
        [DiffPropertyName("ACustomName")]
        public string A { get; set; }
    }

    internal class CustomNameNestedModel
    {
        [DiffPropertyName("BCustomName")]
        public CustomNameModel B { get; set; }
    }
}