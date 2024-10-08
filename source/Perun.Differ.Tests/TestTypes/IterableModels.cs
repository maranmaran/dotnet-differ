﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualBasic;

namespace Differ.DotNet.Tests.TestTypes
{
    public class SimpleIterableTypes
    {
        public Array Array { get; set; }
        public string[] ArrayGeneric { get; set; }

        public ISet<string> SetGeneric { get; set; }
        public HashSet<string> SetTyped { get; set; }

        public IList List { get; set; }
        public List<string> ListGenericTyped { get; set; }
        public IList<string> ListGeneric { get; set; }

        public IEnumerable Enumerable { get; set; }
        public IEnumerable<string> EnumerableGeneric { get; set; }

        public ICollection Collection { get; set; }
        public ICollection<string> CollectionGeneric { get; set; }
        public Collection<string> CollectionGenericTyped { get; set; }
    }

    public class ComplexIterableTypes
    {
        public Array Array { get; set; }
        public ComplexType[] ArrayGeneric { get; set; }

        public ISet<ComplexType> SetGeneric { get; set; }
        public HashSet<ComplexType> SetTyped { get; set; }

        public IList List { get; set; }
        public List<ComplexType> ListGenericTyped { get; set; }
        public IList<ComplexType> ListGeneric { get; set; }

        public IEnumerable Enumerable { get; set; }
        public IEnumerable<ComplexType> EnumerableGeneric { get; set; }

        public ICollection Collection { get; set; }
        public Collection CollectionTyped { get; set; }
        public ICollection<ComplexType> CollectionGeneric { get; set; }
        public Collection<ComplexType> CollectionGenericTyped { get; set; }
    }

    public class NestedSimpleIterableTypes
    {
        public string[][] ArrayGeneric { get; set; }

        public ISet<ISet<string>> SetGeneric { get; set; }
        public IList<IList<string>> ListGeneric { get; set; }

        public IEnumerable<IEnumerable<string>> EnumerableGeneric { get; set; }
        public ICollection<ICollection<string>> CollectionGeneric { get; set; }
    }

    public class NestedComplexIterableTypes
    {
        public ComplexType[][] ArrayGeneric { get; set; }

        public ISet<ISet<ComplexType>> SetGeneric { get; set; }
        public IList<IList<ComplexType>> ListGeneric { get; set; }

        public IEnumerable<IEnumerable<ComplexType>> EnumerableGeneric { get; set; }
        public ICollection<ICollection<ComplexType>> CollectionGeneric { get; set; }
    }

    public class ComplexIterableWithId
    {
        [DiffCollectionId("Id")]
        public List<ComplexWithId> Iterable { get; set; }
    }

    public class ComplexNestedIterableWithId
    {
        [DiffCollectionId("Id")]
        public List<List<ComplexWithId>> Iterable { get; set; }
    }

    public class ComplexDeeplyNestedIterableWithId
    {
        [DiffCollectionId("IdLevel1")]
        public List<Level1> Iterable { get; set; }

        public class Level1
        {
            [IgnoreInDiff]
            public string IdLevel1 { get; set; }

            [DiffCollectionId("IdLevel2")]
            public List<Level2> Iterable { get; set; }
        }

        public class Level2
        {
            [IgnoreInDiff]
            public string IdLevel2 { get; set; }

            [DiffCollectionId("Id")]
            public List<ComplexWithId> Iterable { get; set; }
        }
    }

    public record ComplexWithId
    {
        public ComplexWithId(string id, string value)
        {
            Id = id;
            Value = value;
        }

        [IgnoreInDiff]
        public string Id { get; set; }

        public string Value { get; set; }
    };
}