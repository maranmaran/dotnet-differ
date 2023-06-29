using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoBogus;
using Differ.DotNet.Tests.TestTypes;
using Microsoft.VisualBasic;
using Xunit;

namespace Differ.DotNet.Tests
{
    public class DifferTests
    {
        [Fact]
        public void Simple_Diffs()
        {
            var faker = new AutoFaker<SimpleTypes>();
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diffs = DifferDotNet.Diff(left, right).ToList();
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

                Assert.Equal(leftVal, diff.LeftValue);
                Assert.Equal(rightVal, diff.RightValue);
            }
        }

        [Fact]
        public void Complex_Diffs()
        {
            var faker = new AutoFaker<ComplexType>();
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diffs = DifferDotNet.Diff(left, right).ToList();
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

                Assert.Equal(leftVal, diff.LeftValue);
                Assert.Equal(rightVal, diff.RightValue);
            }
        }

        [Fact]
        public void Nulls_Diffs()
        {
            var entity = new AutoFaker<SimpleTypes>().UseSeed(1).Generate();
            var entity2 = new AutoFaker<ComplexType>().UseSeed(1).Generate();
            var entity3 = new AutoFaker<SimpleIterableTypes>().UseSeed(1).Generate();
            var entity4 = new AutoFaker<ComplexIterableTypes>().UseSeed(1).Generate();

            var diffsNone = DifferDotNet.Diff<SimpleTypes>(null, null).ToList();
            Assert.Empty(diffsNone);

            var diffsRight = DifferDotNet.Diff(null, entity).ToList();
            var diffsLeft = DifferDotNet.Diff(entity, null).ToList();
            Assert.NotEmpty(diffsRight);
            Assert.NotEmpty(diffsLeft);

            diffsRight = DifferDotNet.Diff(null, entity2).ToList();
            diffsLeft = DifferDotNet.Diff(entity2, null).ToList();
            Assert.NotEmpty(diffsRight);
            Assert.NotEmpty(diffsLeft);

            diffsRight = DifferDotNet.Diff(null, entity3).ToList();
            diffsLeft = DifferDotNet.Diff(entity3, null).ToList();
            Assert.NotEmpty(diffsRight);
            Assert.NotEmpty(diffsLeft);

            diffsRight = DifferDotNet.Diff(null, entity4).ToList();
            diffsLeft = DifferDotNet.Diff(entity4, null).ToList();
            Assert.NotEmpty(diffsRight);
            Assert.NotEmpty(diffsLeft);
        }

        [Fact]
        public void NestedComplex_Diffs()
        {
            var faker = new AutoFaker<NestedComplexType>();
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diffs = DifferDotNet.Diff(left, right).ToList();

            var props = typeof(NestedComplexType).GetPropertiesFlat();
            Assert.Equal(props.Count, diffs.Count);

            var nestedProp = props.First();
            var nestedDiff = diffs.First();
            var nestedLeft = left.Nested;
            var nestedRight = right.Nested;

            var leftVal = nestedProp.GetValue(nestedLeft);
            var rightVal = nestedProp.GetValue(nestedRight);

            Assert.Equal(leftVal, nestedDiff.LeftValue);
            Assert.Equal(rightVal, nestedDiff.RightValue);
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

            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diffs = DifferDotNet.Diff(left, right).ToList();

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
        public void ComplexIterable_ListIdDefined_DeleteFirstItem_DeleteDiffRatherThanUpdateDiff()
        {
            var faker = new AutoFaker<ComplexIterableWithId>();

            var left = faker.Generate();
            var right = new ComplexIterableWithId
            {
                Iterable = left.Iterable.Select(x => new ComplexWithId(x.Id, x.Value)).ToList()
            };

            right.Iterable.RemoveAt(0);

            var diff = DifferDotNet.Diff(left, right).Single();

            Assert.Null(diff.RightValue);
        }

        [Fact]
        public void ComplexNestedIterable_ListIdDefined_DeleteFirstItem_DeleteDiffRatherThanUpdateDiff()
        {
            var faker = new AutoFaker<ComplexNestedIterableWithId>();

            var left = faker.Generate();
            var right = new ComplexNestedIterableWithId
            {
                Iterable = left.Iterable.Select(x => x.Select(y => new ComplexWithId(y.Id, y.Value)).ToList()).ToList()
            };

            right.Iterable.First().RemoveAt(0);
            right.Iterable.First().Add(new ComplexWithId("Added", "New Value"));

            var diff = DifferDotNet.Diff(left, right).ToArray();

            Assert.Equivalent(2, diff.Length);
            Assert.Null(diff[0].RightValue);
            Assert.Null(diff[1].LeftValue);
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

            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diffs = DifferDotNet.Diff(left, right).ToList();

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
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diffs = DifferDotNet.Diff(left, right).ToList();

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
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diffs = DifferDotNet.Diff(left, right).ToList();

            var expectedPropsInDiffCount = typeof(NestedComplexIterableTypes).GetProperties().Length;
            var actualPropsInDiffCount = diffs.Select(x => x.FieldPath.Split('.').First()).ToHashSet().Count;
            Assert.Equal(expectedPropsInDiffCount, actualPropsInDiffCount);
        }

        [Fact]
        public void OptionalKeepDiff_Simple_NoSiblingChanges_Ignores()
        {
            var faker = new AutoFaker<SimpleOptionalKeepModel>();
            var left = faker.UseSeed(1).Generate();

            var diff = DifferDotNet.Diff(left, left).SingleOrDefault();

            Assert.Null(diff);
        }

        [Fact]
        public void OptionalKeepDiff_Simple_SiblingChanges_DoesNotIgnore()
        {
            var faker = new AutoFaker<SimpleOptionalKeepModel>();
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diffs = DifferDotNet.Diff(left, right);

            Assert.Equal(2, diffs.Count());
        }

        [Fact]
        public void OptionalKeepDiff_Complex_NoSiblingChange_Ignores()
        {
            var faker = new AutoFaker<ComplexOptionalKeepModel>();
            var left = faker.UseSeed(1).Generate();

            var diff = DifferDotNet.Diff(left, left).SingleOrDefault();

            Assert.Null(diff);
        }

        [Fact]
        public void OptionalKeepDiff_Complex_ChildChange_DoesNotIgnore()
        {
            var faker = new AutoFaker<ComplexOptionalKeepModel>();
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            right.Sibling = left.Sibling;

            var diff = DifferDotNet.Diff(left, right);

            Assert.NotNull(diff.SingleOrDefault());
        }

        [Fact]
        public void OptionalKeepDiff_IterableSimple_ChildChange_DoesNotIgnore()
        {
            var fakerComplex = new AutoFaker<ComplexOptionalKeepModel>();
            var complexSet = fakerComplex.Generate(11);

            var left = new IterableComplexOptionalKeepModel { Iterable = complexSet.Select(x => new ComplexOptionalKeepModel(x)).ToList() };
            var right = new IterableComplexOptionalKeepModel { Iterable = complexSet.Select(x => new ComplexOptionalKeepModel(x)).ToList() };

            // Update sibling
            right.Iterable[1] = new ComplexOptionalKeepModel
            {
                KeepMeOnlyIfSiblingIsDiffed = right.Iterable[1].KeepMeOnlyIfSiblingIsDiffed,
                Sibling = "Changed Sibling - keeps KeepMeOnlyIfSiblingIsDiffed"
            };

            // This is path x.1.string
            // Optional keep must not return path.10.string
            // Since we use trie and match "startWith" 1 != 10

            var diff = DifferDotNet.Diff(left, right);

            Assert.Equal(2, diff.Count());
        }

        [Fact]
        public void KeepDiff_Simple_Keeps()
        {
            var faker = new AutoFaker<SimpleKeepModel>();
            var left = faker.UseSeed(1).Generate();

            var diff = DifferDotNet.Diff(left, left).Single();

            Assert.Equal(left.NoDiffKeepMe, diff.LeftValue);
            Assert.Equal(left.NoDiffKeepMe, diff.RightValue);
            Assert.Equal(diff.LeftValue, diff.RightValue);
            Assert.False(diff.IgnoreIfNoOtherDiff);
        }

        [Fact]
        public void KeepDiff_IterableSimple_KeepsAllChildren()
        {
            var faker = new AutoFaker<IterableSimpleKeepModel>();
            var left = faker.UseSeed(1).Generate();

            var diffs = DifferDotNet.Diff(left, left).ToList();

            Assert.Equal(left.NoDiffKeepMe.Count(), diffs.Count);
            Assert.True(diffs.TrueForAll(x => !x.IgnoreIfNoOtherDiff));
        }

        [Fact]
        public void KeepDiff_IterableComplex_KeepsAllChildren()
        {
            var faker = new AutoFaker<IterableComplexKeepModel>();
            var left = faker.UseSeed(1).Generate();

            var diffs = DifferDotNet.Diff(left, left).ToList();

            Assert.Equal(left.NoDiffKeepMe.Count(), diffs.Count);
            Assert.True(diffs.TrueForAll(x => !x.IgnoreIfNoOtherDiff));
        }

        [Fact]
        public void KeepDiff_Complex_KeepsAllChildren()
        {
            var faker = new AutoFaker<ComplexKeepModel>();
            var left = faker.UseSeed(1).Generate();

            var diffs = DifferDotNet.Diff(left, left).ToList();

            var expectedDiffCount = left.NoDiffKeepMe.GetType().GetProperties().Length;
            Assert.Equal(expectedDiffCount, diffs.Count);
            Assert.True(diffs.TrueForAll(x => !x.IgnoreIfNoOtherDiff));
        }

        [Fact]
        public void IgnoreDiff_Simple_Ignores()
        {
            var faker = new AutoFaker<SimpleIgnoreModel>();
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diff = DifferDotNet.Diff(left, right).ToList();

            Assert.Empty(diff);
        }

        [Fact]
        public void IgnoreDiff_IterableSimple_IgnoresAllChildren()
        {
            var faker = new AutoFaker<IterableSimpleIgnoreModel>();
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diff = DifferDotNet.Diff(left, right).ToList();

            Assert.Empty(diff);
        }

        [Fact]
        public void IgnoreDiff_IterableComplex_IgnoresAllChildren()
        {
            var faker = new AutoFaker<IterableComplexIgnoreModel>();
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diff = DifferDotNet.Diff(left, right).ToList();

            Assert.Empty(diff);
        }

        [Fact]
        public void IgnoreDiff_Complex_IgnoresAllChildren()
        {
            var faker = new AutoFaker<ComplexIgnoreModel>();
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diff = DifferDotNet.Diff(left, right).ToList();

            Assert.Empty(diff);
        }

        [Fact]
        public void KeepParentIgnoreChild_IgnoresChildKeepsOther()
        {
            var faker = new AutoFaker<KeepParentIgnoreChildModel>();
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diff = DifferDotNet.Diff(left, right).ToList();

            Assert.Equal(4, diff.Count);
        }

        [Fact]
        public void IgnoreParentKeepChild_KeepsChildIgnoresOther()
        {
            var faker = new AutoFaker<IgnoreParentKeepChildModel>();
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diffs = DifferDotNet.Diff(left, right).ToList();

            Assert.Equal(4, diffs.Count);
            Assert.True(diffs.Aggregate(true, (acc, cur) => acc &= cur.FieldName.EndsWith("keepMe")));
        }

        [Fact]
        public void CustomNameDefined_RegisteredInDiff()
        {
            var faker = new AutoFaker<CustomNameModel>();
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diff = DifferDotNet.Diff(left, right).Single();

            Assert.Equal("ACustomName", diff.CustomFieldName);
            Assert.Equal("a", diff.FieldName);
        }

        [Fact]
        public void NestedCustomNameDefined_RegisteredInDiff()
        {
            var faker = new AutoFaker<CustomNameNestedModel>();
            var left = faker.UseSeed(1).Generate();
            var right = faker.UseSeed(2).Generate();

            var diff = DifferDotNet.Diff(left, right).Single();

            Assert.Equal("BCustomName.ACustomName", diff.CustomFullPath);
            Assert.Equal("BCustomName", diff.CustomFieldPath);
            Assert.Equal("ACustomName", diff.CustomFieldName);

            Assert.Equal("b.a", diff.FullPath);
            Assert.Equal("b", diff.FieldPath);
            Assert.Equal("a", diff.FieldName);
        }
    }
}