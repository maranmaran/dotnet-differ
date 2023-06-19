using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Differ.DotNet
{
    [PublicAPI]
    public sealed class DifferDotNet
    {
        /// <summary>
        /// Compares two objects of type T and returns an enumerable collection of Difference objects representing the differences between the objects.
        /// </summary>
        /// <typeparam name="T">The type of the objects to compare.</typeparam>
        /// <param name="left">The left object to compare.</param>
        /// <param name="right">The right object to compare.</param>
        /// <returns>An enumerable collection of Difference objects representing the differences between the left and right objects.</returns>
        public static IEnumerable<Difference> Diff<T>(T left, T right) where T : class
        {
            var differences = DiffRecursive(
                string.Empty,
                string.Empty,
                typeof(T),
                left,
                right,
                new DiffCollection(),
                DiffActions.Default
            );

            differences = AttributeApplier.ApplyAttributes(differences);

            return differences.Diffs.OrderBy(x => x.Key).Select(x => x.Value);
        }

        private static DiffCollection DiffRecursive<T>(
            string path,
            string customPath,
            Type type,
            T leftObj,
            T rightObj,
            DiffCollection diffs,
            DiffActions actions
        )
        {
            if (HandleSimpleType(path, customPath, type, leftObj, rightObj, diffs, actions))
            {
                return diffs;
            }

            if (HandleIterable(path, customPath, type, leftObj, rightObj, diffs, actions))
            {
                return diffs;
            }

            HandleComplex(path, customPath, type, leftObj, rightObj, diffs, actions);

            return diffs;
        }

        private static bool HandleComplex<T>(
            string path,
            string customPath,
            Type type,
            T leftObj, T rightObj,
            DiffCollection diffs,
            DiffActions actions
        )
        {
            if (!type.IsComplex())
            {
                return false;
            }

            // Check all the properties of the object
            foreach (var prop in type.GetProperties())
            {
                var name = prop.Name.ToCamelCase();
                var customName = (prop.GetCustomAttribute<DiffPropertyName>()?.Name ?? name);

                var fullPath = path + name;
                var customFullPath = customPath + customName;

                var resetActions = actions;

                // Check for IgnoreInDiff attribute
                if (prop.GetCustomAttribute<IgnoreInDiffAttribute>() != null)
                {
                    if (!diffs.IgnorePaths.Contains(fullPath))
                        diffs.IgnorePaths.Add(fullPath);

                    actions |= DiffActions.Ignore;
                }

                // Check for KeepInDiff attribute
                if (prop.GetCustomAttribute<KeepInDiffAttribute>() != null)
                {
                    if (!diffs.KeepPaths.Contains(fullPath))
                        diffs.KeepPaths.Add(fullPath);

                    actions |= DiffActions.Keep;
                }

                // Recurse on sub-objects
                DiffRecursive(
                    fullPath + ".",
                    customFullPath + ".",
                    prop.PropertyType,
                    leftObj != null ? prop.GetValue(leftObj) : null,
                    rightObj != null ? prop.GetValue(rightObj) : null,
                    diffs,
                    actions
                );

                actions = resetActions;
            }

            return true;
        }

        private static bool HandleIterable<T>(
            string path,
            string customPath,
            Type type,
            T leftObj, T rightObj,
            DiffCollection diffs,
            DiffActions actions
        )
        {
            if (!type.IsIterable())
            {
                return false;
            }

            var leftArr = (leftObj as IEnumerable)?.GetEnumerator().ToArray() ?? Array.Empty<object>();
            var rightArr = (rightObj as IEnumerable)?.GetEnumerator().ToArray() ?? Array.Empty<object>();

            for (var i = 0; i < Math.Max(leftArr.Length, rightArr.Length); i++)
            {
                var curL = i < leftArr.Length ? leftArr[i] : null;
                var curR = i < rightArr.Length ? rightArr[i] : null;

                DiffRecursive(
                    path + $"{i}.",
                    customPath + $"{i}.",
                    type.GetIterableType() ?? curL?.GetType() ?? curR?.GetType(),
                    curL,
                    curR,
                    diffs,
                    actions
                );
            }

            return true;
        }

        private static bool HandleSimpleType<T>(
            string path,
            string customPath,
            Type type,
            T leftObj, T rightObj,
            DiffCollection diffs,
            DiffActions actions
        )
        {
            if (!type.IsSimple())
            {
                return false;
            }

            var diff = new Difference(
                path.Substring(0, path.Length - 1),
                customPath.Substring(0, customPath.Length - 1),
                leftObj,
                rightObj
            );

            if (actions.HasFlag(DiffActions.Keep))
            {
                diffs.KeepDiffs.Add(diff);
            }

            var exitCond = actions.HasFlag(DiffActions.Ignore)
                || leftObj == null && rightObj == null
                || diffs.Diffs.ContainsKey(diff.FullPath)
                || leftObj?.Equals(rightObj) == true;
            if (exitCond)
            {
                return true;
            }

            diffs.Diffs.Add(diff.FullPath, diff);
            return true;
        }
    }
}