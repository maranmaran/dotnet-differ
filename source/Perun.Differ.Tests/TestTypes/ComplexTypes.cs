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

    public class ComplexTypeWithToStringOverride
    {
        public string String { get; set; }

        public static string Name()
        {
            return nameof(ComplexTypeWithToStringOverride);
        }

        public override string ToString()
        {
            return Name();
        }
    }
}