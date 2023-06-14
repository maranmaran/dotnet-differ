using System.Collections.Generic;
using System.Linq;

namespace Perun.Differ
{
    internal static class AttributeDiffModifier
    {
        internal static DiffCollection ApplyAttributes(DiffCollection collection)
        {
            var keepPaths = collection.KeepPaths.ToList();
            var ignorePaths = collection.IgnorePaths.ToList();

            AddDiffsToKeep(collection.Diffs, collection.KeepDiffs);
            RemoveDiffsToIgnore(collection.Diffs, ignorePaths, keepPaths);

            return collection;
        }

        private static Dictionary<string, Difference> AddDiffsToKeep(
            Dictionary<string, Difference> differences,
            List<Difference> keepDiffs
        )
        {
            foreach (var keepDiff in keepDiffs)
            {
                // relevant, keep it in diff
                if (!differences.ContainsKey(keepDiff.FullPath))
                {
                    differences.Add(keepDiff.FullPath, keepDiff);
                }
            }

            return differences;
        }

        private static Dictionary<string, Difference> RemoveDiffsToIgnore(
            Dictionary<string, Difference> differences,
            List<string> ignorePaths,
            List<string> keepPaths
            )
        {
            var ignoreKeys = new List<string>();
            foreach (var ignorePath in ignorePaths)
            {
                var ignorePathSplit = ignorePath.Split('.');

                ignoreKeys.AddRange(
                    differences.Keys.Where(key =>
                    {
                        var keySplit = key.Split('.');

                        var startsWith = true;
                        for (var i = 0; i < ignorePathSplit.Length; i++)
                        {
                            if (i >= keySplit.Length)
                            {
                                break;
                            }

                            var ignoreNode = ignorePathSplit[i];
                            var keyNode = keySplit[i];

                            startsWith &= ignoreNode?.ToLower() == keyNode?.ToLower();
                        }

                        return startsWith;
                    })
                );
            }

            ignoreKeys = ignoreKeys.Except(keepPaths).ToList();

            foreach (var ignoreKey in ignoreKeys)
            {
                differences.Remove(ignoreKey);
            }

            return differences;
        }
    }
}