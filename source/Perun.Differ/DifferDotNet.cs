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
                null,
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
            PropertyInfo prop,
            Type type,
            T leftObj,
            T rightObj,
            DiffCollection diffs,
            DiffActions actions
        )
        {
            if (HandleSimpleType(path, customPath, prop, type, leftObj, rightObj, diffs, actions))
            {
                return diffs;
            }

            if (HandleDictionary(path, customPath, prop, type, leftObj, rightObj, diffs, actions))
            {
                return diffs;
            }

            if (HandleIterable(path, customPath, prop, type, leftObj, rightObj, diffs, actions))
            {
                return diffs;
            }

            HandleComplex(path, customPath, prop, type, leftObj, rightObj, diffs, actions);

            return diffs;
        }

        private static bool HandleComplex<T>(
            string path,
            string customPath,
            PropertyInfo curProp,
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
                var customName = GetCustomPropertyName<T>(type, rightObj, leftObj, prop) ?? name;

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
                if (prop.GetCustomAttribute<KeepInDiffAttribute>() is { } keepAtt)
                {
                    actions |= keepAtt.IgnoreIfNoSiblingOrChildDiff
                        ? DiffActions.KeepOptional
                        : DiffActions.Keep;
                }

                // Recurse on sub-objects
                DiffRecursive(
                    fullPath + ".",
                    customFullPath + ".",
                    prop,
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

        private static bool HandleDictionary<T>(
            string path,
            string customPath,
            PropertyInfo prop,
            Type type,
            T leftObj, T rightObj,
            DiffCollection diffs,
            DiffActions actions
        )
        {
            if (!type.IsDictionary())
            {
                return false;
            }

            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];

            var leftDict = leftObj as IDictionary;
            var rightDict = rightObj as IDictionary;

            // Combine the keys from both dictionaries
            var allKeys = new HashSet<object>();
            if (leftDict != null)
            {
                foreach (var key in leftDict.Keys)
                {
                    allKeys.Add(key);
                }
            }
            if (rightDict != null)
            {
                foreach (var key in rightDict.Keys)
                {
                    allKeys.Add(key);
                }
            }

            // Compare values associated with each key
            foreach (var key in allKeys)
            {
                var leftValue = leftDict != null && leftDict.Contains(key) ? leftDict[key] : null;
                var rightValue = rightDict != null && rightDict.Contains(key) ? rightDict[key] : null;

                // Construct paths using the dictionary key
                var keyStr = key.ToString();
                var fullPath = $"{path}{keyStr}";
                var customFullPath = $"{customPath}{keyStr}";

                DiffRecursive(
                    fullPath + ".",
                    customFullPath + ".",
                    prop,
                    valueType,
                    leftValue,
                    rightValue,
                    diffs,
                    actions
                );
            }

            return true;
        }

        private static bool HandleIterable<T>(
            string path,
            string customPath,
            PropertyInfo prop,
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
            var underlyingType = type.GetIterableType()
                                 ?? leftArr.FirstOrDefault()?.GetType()
                                 ?? rightArr.FirstOrDefault()?.GetType();

            // Check for DiffArrayIdPropertyName attribute
            PropertyInfo idProperty = null;
            if (prop != null && prop.GetCustomAttribute<DiffCollectionId>() is { } idAttr)
            {
                idProperty = underlyingType?.GetProperty(idAttr.Name);
            }

            // key based
            if (idProperty != null)
            {
                var leftMap = leftArr.ToDictionary(idProperty.GetValue);
                var rightMap = rightArr.ToDictionary(idProperty.GetValue);

                var mapPairs = leftMap.Concat(rightMap).GroupBy(x => x.Key)
                    .ToDictionary(
                        x => x.Key,
                        x => (
                            Left: leftMap.ContainsKey(x.Key) ? leftMap[x.Key] : null,
                            Right: rightMap.ContainsKey(x.Key) ? rightMap[x.Key] : null
                        )
                    );

                for (var i = 0; i < mapPairs.Count; i++)
                {
                    var pair = mapPairs.ElementAt(i);
                    var curL = pair.Value.Left;
                    var curR = pair.Value.Right;

                    DiffRecursive(
                        path + $"{i}.",
                        customPath + $"{i}.",
                        prop,
                        underlyingType,
                        curL,
                        curR,
                        diffs,
                        actions
                    );
                }

                return true;
            }

            // index based
            for (var i = 0; i < Math.Max(leftArr.Length, rightArr.Length); i++)
            {
                var curL = i < leftArr.Length ? leftArr[i] : null;
                var curR = i < rightArr.Length ? rightArr[i] : null;

                DiffRecursive(
                    path + $"{i}.",
                    customPath + $"{i}.",
                    prop,
                    underlyingType,
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
            PropertyInfo prop,
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

            if (actions.HasFlag(DiffActions.Keep) || actions.HasFlag(DiffActions.KeepOptional))
            {
                diff.IgnoreIfNoOtherDiff = actions.HasFlag(DiffActions.KeepOptional);
                diff.Keep = true;
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

        private static string GetCustomPropertyName<T>(Type type, T left, T right, PropertyInfo prop)
        {
            var attr = prop?.GetCustomAttribute<DiffPropertyName>();
            if (attr is null)
            {
                return null;
            }

            if (attr.FromPropertyValue == false)
            {
                return attr.Name;
            }

            var segments = attr.Name.Split('.');
            var nestedProp = prop;
            var nestedType = type;
            object nestedLeft = left;
            object nestedRight = right;

            for (var i = 0; i < segments.Length; i++)
            {
                nestedProp = nestedType?.GetProperty(segments[i]);
                if (nestedProp is null)
                {
                    return attr.Name;
                }

                nestedType = nestedProp.PropertyType;
                if (nestedType.IsIterable())
                {
                    return attr.Name;
                }

                if (i < segments.Length - 1)
                {
                    nestedLeft = TryGetValue(nestedProp, nestedLeft);
                    nestedRight = TryGetValue(nestedProp, nestedRight);
                }
            }

            var customName = (TryGetValue(nestedProp, nestedRight) ?? TryGetValue(nestedProp, nestedLeft))?.ToString();

            return customName ?? attr.Name;
        }

        [CanBeNull]
        private static object TryGetValue(PropertyInfo prop, object obj)
        {
            return obj is not null ? prop.GetValue(obj) : null;
        }
    }
}