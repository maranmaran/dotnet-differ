using System.Collections.Generic;
using System.Linq;

namespace Differ.DotNet
{
    internal static class AttributeApplier
    {
        internal static DiffCollection ApplyAttributes(DiffCollection collection)
        {
            var trie = CreateTrie(collection);

            AddDiffsToKeep(collection.Diffs, collection.KeepDiffs, trie);
            RemoveDiffsToIgnore(collection.Diffs, collection.IgnorePaths);

            return collection;
        }

        private static Trie<Difference> CreateTrie(DiffCollection collection)
        {
            var hasOptional = collection.KeepDiffs.Any(x => x.IgnoreIfNoOtherDiff);
            var hasIgnore = collection.IgnorePaths.Any();

            var trie = new Trie<Difference>();
            if (hasOptional || hasIgnore)
            {
                foreach (var diff in collection.Diffs.Values)
                {
                    trie.Add(diff.FieldPath, diff);
                }
            }

            return trie;
        }

        private static void AddDiffsToKeep(
            Dictionary<string, Difference> differences,
            HashSet<Difference> keepDiffs,
            Trie<Difference> trie
        )
        {
            foreach (var diff in keepDiffs)
            {
                if (diff.IgnoreIfNoOtherDiff && !trie.Retrieve(diff.FieldPath).Any())
                {
                    continue;
                }

                var added = differences.TryAdd(diff.FullPath, diff);

                if (added) // modify trie for search on new records
                {
                    trie.Add(diff.FullPath, diff);
                }
            }
        }

        private static void RemoveDiffsToIgnore(
            Dictionary<string, Difference> differences,
            HashSet<string> ignorePaths
        )
        {
            var pathsToRemove = ignorePaths.Where(differences.ContainsKey);

            foreach (var path in pathsToRemove)
            {
                differences.Remove(path);
            }
        }
    }
}