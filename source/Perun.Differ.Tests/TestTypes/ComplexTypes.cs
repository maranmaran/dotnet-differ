﻿namespace Differ.DotNet.Tests.TestTypes
{
    public class ComplexType
    {
        public string String { get; set; }
    }

    public class NestedComplexType
    {
        public ComplexType Nested { get; set; }
    }
}