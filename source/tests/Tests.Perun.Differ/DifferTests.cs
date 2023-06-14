﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoBogus;
using Microsoft.VisualBasic;
using Perun.Differ;
using Tests.Perun.Differ.TestTypes;
using Xunit;

namespace Tests.Perun.Differ
{
    public class DifferTests
    {
        [Fact]
        public void Simple_Diffs()
        {
            var faker = new AutoFaker<SimpleTypes>();
            var left = faker.Generate();
            var right = faker.Generate();

            var diffs = ObjectDiffer.Diff(left, right).ToList();
            var diffsMap = diffs.ToDictionary(
                x => x.FullPath,
                x => x,
                StringComparer.InvariantCultureIgnoreCase
            );

            var props = typeof(SimpleTypes).GetPropertiesFlat();
            Assert.Equal(props.Count, diffs.Count);

            foreach (var prop in props)
            {
                var leftVal = prop.GetValue(left);
                var rightVal = prop.GetValue(right);
                var diff = diffsMap[prop.Name];

                Assert.Equal(leftVal, diff.OldValue);
                Assert.Equal(rightVal, diff.NewValue);
            }
        }

        [Fact]
        public void Complex_Diffs()
        {
            var faker = new AutoFaker<ComplexType>();
            var left = faker.Generate();
            var right = faker.Generate();

            var diffs = ObjectDiffer.Diff(left, right).ToList();
            var diffsMap = diffs.ToDictionary(
                x => x.FullPath,
                x => x,
                StringComparer.InvariantCultureIgnoreCase
            );

            var props = typeof(ComplexType).GetPropertiesFlat();
            Assert.Equal(props.Count, diffs.Count);

            foreach (var prop in props)
            {
                var leftVal = prop.GetValue(left);
                var rightVal = prop.GetValue(right);
                var diff = diffsMap[prop.Name];

                Assert.Equal(leftVal, diff.OldValue);
                Assert.Equal(rightVal, diff.NewValue);
            }
        }

        [Fact]
        public void NestedComplex_Diffs()
        {
            var faker = new AutoFaker<NestedComplexType>();
            var left = faker.Generate();
            var right = faker.Generate();

            var diffs = ObjectDiffer.Diff(left, right).ToList();

            var props = typeof(NestedComplexType).GetPropertiesFlat();
            Assert.Equal(props.Count, diffs.Count);

            var nestedProp = props.First();
            var nestedDiff = diffs.First();
            var nestedLeft = left.Nested;
            var nestedRight = right.Nested;

            var leftVal = nestedProp.GetValue(nestedLeft);
            var rightVal = nestedProp.GetValue(nestedRight);

            Assert.Equal(leftVal, nestedDiff.OldValue);
            Assert.Equal(rightVal, nestedDiff.NewValue);
        }

        [Fact]
        public void SimpleIterable_Diffs()
        {
            var faker = new AutoFaker<SimpleIterableTypes>();
            faker.RuleForType(typeof(Array), x => x.Random.WordsArray(3).ToArray() as Array);
            faker.RuleForType(typeof(IList), x => x.Random.WordsArray(3).ToList() as IList);
            faker.RuleForType(typeof(IEnumerable), x => x.Random.WordsArray(3).ToList() as IEnumerable);
            faker.RuleForType(typeof(ICollection), x => x.Random.WordsArray(3).ToList() as ICollection);
            faker.RuleForType(typeof(IDictionary), x => x.Make(3, () =>
                    new KeyValuePair<string, string>(x.Random.Word(), x.Random.Word()))
                    .ToDictionary(x => x.Key, x => x.Value) as IDictionary
            );
            faker.RuleForType(typeof(Collection), x => new Collection
            {
                x.Random.Word(),
                x.Random.Word(),
                x.Random.Word(),
            });

            var left = faker.Generate();
            var right = faker.Generate();

            var diffs = ObjectDiffer.Diff(left, right).ToList();

            var expectedPropsInDiffCount = typeof(SimpleIterableTypes).GetProperties().Length;
            var actualPropsInDiffCount = diffs.Select(x => x.FieldPath.Split('.').First()).ToHashSet().Count;
            Assert.Equal(expectedPropsInDiffCount, actualPropsInDiffCount);

            var diffsLookup = diffs.ToLookup(
                x => x.FullPath.Split('.')[0].ToLower(),
                x => x
            );

            Extensions.AssertIterable(
                left.CollectionGeneric,
                right.CollectionGeneric,
                diffsLookup[nameof(SimpleIterableTypes.CollectionGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.ListGeneric,
                right.ListGeneric,
                diffsLookup[nameof(SimpleIterableTypes.ListGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.ArrayGeneric,
                right.ArrayGeneric,
                diffsLookup[nameof(SimpleIterableTypes.ArrayGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.DictionaryGeneric,
                right.DictionaryGeneric,
                diffsLookup[nameof(SimpleIterableTypes.DictionaryGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.EnumerableGeneric,
                right.EnumerableGeneric,
                diffsLookup[nameof(SimpleIterableTypes.EnumerableGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.ListGenericTyped,
                right.ListGenericTyped,
                diffsLookup[nameof(SimpleIterableTypes.ListGenericTyped).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.CollectionGenericTyped,
                right.CollectionGenericTyped,
                diffsLookup[nameof(SimpleIterableTypes.CollectionGenericTyped).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.DictionaryGenericTyped,
                right.DictionaryGenericTyped,
                diffsLookup[nameof(SimpleIterableTypes.DictionaryGenericTyped).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.SetGeneric,
                right.SetGeneric,
                diffsLookup[nameof(SimpleIterableTypes.SetGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.SetTyped,
                right.SetTyped,
                diffsLookup[nameof(SimpleIterableTypes.SetTyped).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                (IEnumerable<string>)left.Collection,
                (IEnumerable<string>)right.Collection,
                diffsLookup[nameof(SimpleIterableTypes.Collection).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                (IEnumerable<string>)left.List,
                (IEnumerable<string>)right.List,
                diffsLookup[nameof(SimpleIterableTypes.List).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                (IEnumerable<string>)left.Array,
                (IEnumerable<string>)right.Array,
                diffsLookup[nameof(SimpleIterableTypes.Array).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                (IEnumerable<string>)left.Enumerable,
                (IEnumerable<string>)right.Enumerable,
                diffsLookup[nameof(SimpleIterableTypes.Enumerable).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                (IEnumerable<KeyValuePair<string, string>>)left.Dictionary,
                (IEnumerable<KeyValuePair<string, string>>)right.Dictionary,
                diffsLookup[nameof(SimpleIterableTypes.Dictionary).ToLower()].ToList()
            );
        }

        [Fact]
        public void ComplexIterable_Diffs()
        {
            var complexFaker = new AutoFaker<ComplexType>();

            var faker = new AutoFaker<ComplexIterableTypes>();
            faker.RuleForType(typeof(Array), x => complexFaker.Generate(3).ToArray() as Array);
            faker.RuleForType(typeof(IList), x => complexFaker.Generate(3).ToList() as IList);
            faker.RuleForType(typeof(IEnumerable), x => complexFaker.Generate(3).ToList() as IEnumerable);
            faker.RuleForType(typeof(ICollection), x => complexFaker.Generate(3).ToList() as ICollection);
            faker.RuleForType(typeof(IDictionary), x => x.Make(3, () =>
                    new KeyValuePair<ComplexType, ComplexType>(complexFaker.Generate(), complexFaker.Generate()))
                .ToDictionary(x => x.Key, x => x.Value) as IDictionary
            );
            faker.RuleForType(typeof(Collection), x => new Collection
            {
                complexFaker.Generate(),
                complexFaker.Generate(),
                complexFaker.Generate(),
            });

            var left = faker.Generate();
            var right = faker.Generate();

            var diffs = ObjectDiffer.Diff(left, right).ToList();

            var expectedPropsInDiffCount = typeof(ComplexIterableTypes).GetProperties().Length;
            var actualPropsInDiffCount = diffs.Select(x => x.FieldPath.Split('.').First()).ToHashSet().Count;
            Assert.Equal(expectedPropsInDiffCount, actualPropsInDiffCount);

            var diffsLookup = diffs.ToLookup(
                x => x.FullPath.Split('.')[0].ToLower(),
                x => x
            );

            Extensions.AssertIterable(
                left.CollectionGeneric.Select(x => x.String),
                right.CollectionGeneric.Select(x => x.String),
                diffsLookup[nameof(ComplexIterableTypes.CollectionGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.ListGeneric.Select(x => x.String),
                right.ListGeneric.Select(x => x.String),
                diffsLookup[nameof(ComplexIterableTypes.ListGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.ArrayGeneric.Select(x => x.String),
                right.ArrayGeneric.Select(x => x.String),
                diffsLookup[nameof(ComplexIterableTypes.ArrayGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.DictionaryGeneric,
                right.DictionaryGeneric,
                diffsLookup[nameof(ComplexIterableTypes.DictionaryGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.EnumerableGeneric.Select(x => x.String),
                right.EnumerableGeneric.Select(x => x.String),
                diffsLookup[nameof(ComplexIterableTypes.EnumerableGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.ListGenericTyped.Select(x => x.String),
                right.ListGenericTyped.Select(x => x.String),
                diffsLookup[nameof(ComplexIterableTypes.ListGenericTyped).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.CollectionGenericTyped.Select(x => x.String),
                right.CollectionGenericTyped.Select(x => x.String),
                diffsLookup[nameof(ComplexIterableTypes.CollectionGenericTyped).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.DictionaryGenericTyped,
                right.DictionaryGenericTyped,
                diffsLookup[nameof(ComplexIterableTypes.DictionaryGenericTyped).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.SetGeneric.Select(x => x.String),
                right.SetGeneric.Select(x => x.String),
                diffsLookup[nameof(ComplexIterableTypes.SetGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.SetTyped.Select(x => x.String),
                right.SetTyped.Select(x => x.String),
                diffsLookup[nameof(ComplexIterableTypes.SetTyped).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                ((IEnumerable<ComplexType>)left.Collection).Select(x => x.String),
                ((IEnumerable<ComplexType>)right.Collection).Select(x => x.String),
                diffsLookup[nameof(ComplexIterableTypes.Collection).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                ((IEnumerable<ComplexType>)left.List).Select(x => x.String),
                ((IEnumerable<ComplexType>)right.List).Select(x => x.String),
                diffsLookup[nameof(ComplexIterableTypes.List).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                ((IEnumerable<ComplexType>)left.Array).Select(x => x.String),
                ((IEnumerable<ComplexType>)right.Array).Select(x => x.String),
                diffsLookup[nameof(ComplexIterableTypes.Array).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                ((IEnumerable<ComplexType>)left.Enumerable).Select(x => x.String),
                ((IEnumerable<ComplexType>)right.Enumerable).Select(x => x.String),
                diffsLookup[nameof(ComplexIterableTypes.Enumerable).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                (IEnumerable<KeyValuePair<ComplexType, ComplexType>>)left.Dictionary,
                (IEnumerable<KeyValuePair<ComplexType, ComplexType>>)right.Dictionary,
                diffsLookup[nameof(ComplexIterableTypes.Dictionary).ToLower()].ToList()
            );
        }

        [Fact]
        public void NestedSimpleIterable_Diffs()
        {
            var faker = new AutoFaker<NestedSimpleIterableTypes>();
            var left = faker.Generate();
            var right = faker.Generate();

            var diffs = ObjectDiffer.Diff(left, right).ToList();

            var expectedPropsInDiffCount = typeof(NestedSimpleIterableTypes).GetProperties().Length;
            var actualPropsInDiffCount = diffs.Select(x => x.FieldPath.Split('.').First()).ToHashSet().Count;
            Assert.Equal(expectedPropsInDiffCount, actualPropsInDiffCount);

            var diffsLookup = diffs.ToLookup(
                x => x.FullPath.Split('.')[0].ToLower(),
                x => x
            );

            Extensions.AssertIterable(
                left.CollectionGeneric.SelectMany(x => x),
                right.CollectionGeneric.SelectMany(x => x),
                diffsLookup[nameof(SimpleIterableTypes.CollectionGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.ListGeneric.SelectMany(x => x),
                right.ListGeneric.SelectMany(x => x),
                diffsLookup[nameof(SimpleIterableTypes.ListGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.ArrayGeneric.SelectMany(x => x),
                right.ArrayGeneric.SelectMany(x => x),
                diffsLookup[nameof(SimpleIterableTypes.ArrayGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.EnumerableGeneric.SelectMany(x => x),
                right.EnumerableGeneric.SelectMany(x => x),
                diffsLookup[nameof(SimpleIterableTypes.EnumerableGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.SetGeneric.SelectMany(x => x),
                right.SetGeneric.SelectMany(x => x),
                diffsLookup[nameof(SimpleIterableTypes.SetGeneric).ToLower()].ToList()
            );

            Extensions.AssertIterable(
                left.DictionaryGeneric,
                right.DictionaryGeneric,
                diffsLookup[nameof(SimpleIterableTypes.DictionaryGeneric).ToLower()].ToList()
            );
        }

        [Fact]
        public void NestedComplexIterable_Diffs()
        {
            var faker = new AutoFaker<NestedComplexIterableTypes>();
            var left = faker.Generate();
            var right = faker.Generate();

            var diffs = ObjectDiffer.Diff(left, right).ToList();

            var expectedPropsInDiffCount = typeof(NestedComplexIterableTypes).GetProperties().Length;
            var actualPropsInDiffCount = diffs.Select(x => x.FieldPath.Split('.').First()).ToHashSet().Count;
            Assert.Equal(expectedPropsInDiffCount, actualPropsInDiffCount);
        }

        [Fact]
        public void KeepDiff_Simple_Keeps()
        {
            var faker = new AutoFaker<SimpleKeepModel>();
            var left = faker.Generate();

            var diff = ObjectDiffer.Diff(left, left).Single();

            Assert.Equal(left.NoDiffKeepMe, diff.OldValue);
            Assert.Equal(left.NoDiffKeepMe, diff.NewValue);
            Assert.Equal(diff.OldValue, diff.NewValue);
        }

        [Fact]
        public void KeepDiff_IterableSimple_KeepsAllChildren()
        {
            var faker = new AutoFaker<IterableSimpleKeepModel>();
            var left = faker.Generate();

            var diffs = ObjectDiffer.Diff(left, left).ToList();

            Assert.Equal(left.NoDiffKeepMe.Count(), diffs.Count);
        }

        [Fact]
        public void KeepDiff_IterableComplex_KeepsAllChildren()
        {
            var faker = new AutoFaker<IterableComplexKeepModel>();
            var left = faker.Generate();

            var diffs = ObjectDiffer.Diff(left, left).ToList();

            Assert.Equal(left.NoDiffKeepMe.Count(), diffs.Count);
        }

        [Fact]
        public void KeepDiff_Complex_KeepsAllChildren()
        {
            var faker = new AutoFaker<ComplexKeepModel>();
            var left = faker.Generate();

            var diffs = ObjectDiffer.Diff(left, left).ToList();

            var expectedDiffCount = left.NoDiffKeepMe.GetType().GetProperties().Length;
            Assert.Equal(expectedDiffCount, diffs.Count);
        }

        [Fact]
        public void IgnoreDiff_Simple_Ignores()
        {
            var faker = new AutoFaker<SimpleIgnoreModel>();
            var left = faker.Generate();
            var right = faker.Generate();

            var diff = ObjectDiffer.Diff(left, right).ToList();

            Assert.Empty(diff);
        }

        [Fact]
        public void IgnoreDiff_IterableSimple_IgnoresAllChildren()
        {
            var faker = new AutoFaker<IterableSimpleIgnoreModel>();
            var left = faker.Generate();
            var right = faker.Generate();

            var diff = ObjectDiffer.Diff(left, right).ToList();

            Assert.Empty(diff);
        }

        [Fact]
        public void IgnoreDiff_IterableComplex_IgnoresAllChildren()
        {
            var faker = new AutoFaker<IterableComplexIgnoreModel>();
            var left = faker.Generate();
            var right = faker.Generate();

            var diff = ObjectDiffer.Diff(left, right).ToList();

            Assert.Empty(diff);
        }

        [Fact]
        public void IgnoreDiff_Complex_IgnoresAllChildren()
        {
            var faker = new AutoFaker<ComplexIgnoreModel>();
            var left = faker.Generate();
            var right = faker.Generate();

            var diff = ObjectDiffer.Diff(left, right).ToList();

            Assert.Empty(diff);
        }

        [Fact]
        public void KeepParentIgnoreChild_IgnoresChildKeepsOther()
        {
            var faker = new AutoFaker<KeepParentIgnoreChildModel>();
            var left = faker.Generate();
            var right = faker.Generate();

            var diff = ObjectDiffer.Diff(left, right).ToList();

            Assert.Equal(4, diff.Count);
        }

        [Fact]
        public void IgnoreParentKeepChild_KeepsChildIgnoresOther()
        {
            var faker = new AutoFaker<IgnoreParentKeepChildModel>();
            var left = faker.Generate();
            var right = faker.Generate();

            var diffs = ObjectDiffer.Diff(left, right).ToList();

            Assert.Equal(4, diffs.Count);
            Assert.True(diffs.Aggregate(true, (acc, cur) => acc &= cur.FieldName.EndsWith("keepMe")));
        }

        [Fact]
        public void CustomNameDefined_RegisteredInDiff()
        {
            var faker = new AutoFaker<CustomNameModel>();
            var left = faker.Generate();
            var right = faker.Generate();

            var diff = ObjectDiffer.Diff(left, right).Single();

            Assert.Equal("ACustomName", diff.CustomFieldName);
            Assert.Equal("a", diff.FieldName);
        }

        [Fact]
        public void NestedCustomNameDefined_RegisteredInDiff()
        {
            var faker = new AutoFaker<CustomNameNestedModel>();
            var left = faker.Generate();
            var right = faker.Generate();

            var diff = ObjectDiffer.Diff(left, right).Single();

            Assert.Equal("BCustomName.ACustomName", diff.CustomFullPath);
            Assert.Equal("BCustomName", diff.CustomFieldPath);
            Assert.Equal("ACustomName", diff.CustomFieldName);

            Assert.Equal("b.a", diff.FullPath);
            Assert.Equal("b", diff.FieldPath);
            Assert.Equal("a", diff.FieldName);
        }
    }
}