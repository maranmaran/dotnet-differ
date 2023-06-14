using Perun.Differ;

namespace Tests.Perun.Differ.TestTypes
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