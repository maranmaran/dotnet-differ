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

            differences = AttributeDiffModifier.ApplyAttributes(differences);

            return differences.Diffs.OrderBy(x => x.Key).Select(x => x.Value);
        }

        private static DiffCollection DiffRecursive<T>(
            string path,
            string customPath,
            Type type,
            T oldObj,
            T newObj,
            DiffCollection diffs,
            DiffActions actions
        )
        {
            if (HandleSimpleType(path, customPath, type, oldObj, newObj, diffs, actions))
            {
                return diffs;
            }

            if (HandleIterable(path, customPath, type, oldObj, newObj, diffs, actions))
            {
                return diffs;
            }

            HandleComplex(path, customPath, type, oldObj, newObj, diffs, actions);

            return diffs;
        }

        private static bool HandleComplex<T>(
            string path,
            string customPath,
            Type type,
            T oldObj, T newObj,
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
                    oldObj != null ? prop.GetValue(oldObj) : null,
                    newObj != null ? prop.GetValue(newObj) : null,
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
            T oldObj, T newObj,
            DiffCollection diffs,
            DiffActions actions
        )
        {
            if (!type.IsIterable())
            {
                return false;
            }

            var oldArray = (oldObj as IEnumerable)?.GetEnumerator().ToArray() ?? Array.Empty<object>();
            var newArray = (newObj as IEnumerable)?.GetEnumerator().ToArray() ?? Array.Empty<object>();

            for (var i = 0; i < Math.Max(oldArray.Length, newArray.Length); i++)
            {
                var currentOld = i < oldArray.Length ? oldArray[i] : null;
                var currentNew = i < newArray.Length ? newArray[i] : null;

                DiffRecursive(
                    path + $"{i}.",
                    customPath + $"{i}.",
                    type.GetIterableType() ?? currentOld?.GetType() ?? currentNew?.GetType(),
                    currentOld,
                    currentNew,
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
            T oldObj, T newObj,
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
                oldObj,
                newObj
            );

            if (actions.HasFlag(DiffActions.Keep))
            {
                diffs.KeepDiffs.Add(diff);
            }

            if (actions.HasFlag(DiffActions.Ignore))
            {
                return true;
            }

            var hasDiff = !oldObj?.Equals(newObj) ?? false;
            if (hasDiff && !diffs.Diffs.ContainsKey(diff.FullPath))
            {
                diffs.Diffs.Add(diff.FullPath, diff);
            }

            return true;
        }
    }
}