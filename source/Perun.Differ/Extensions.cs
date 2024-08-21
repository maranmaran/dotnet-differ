using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Differ.DotNet
{
    internal static class Extensions
    {
        internal static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
            {
                dict.Add(key, value);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reference: https://stackoverflow.com/a/65079923
        /// </summary>
        internal static bool IsSimple(this Type type)
        {
            return type.IsValueType || type.IsPrimitive || TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
        }

        internal static bool IsComplex(this Type type)
        {
            return !IsSimple(type)
                   && !IsIterable(type)
                   && type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase).Any();
        }

        internal static bool IsIterable(this Type type)
        {
            if (type == typeof(string) || IsDictionary(type))
            {
                return false;
            }

            return type.IsArray
                    || type == typeof(IEnumerable)
                    || type.GetInterfaces().Contains(typeof(IEnumerable));
        }

        internal static bool IsDictionary(this Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            var genericTypeDefinition = type.GetGenericTypeDefinition();

            return genericTypeDefinition == typeof(IDictionary<,>) ||
                   type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        internal static bool IsSet(this Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            var genericTypeDefinition = type.GetGenericTypeDefinition();

            return genericTypeDefinition == typeof(ISet<>) ||
                   genericTypeDefinition == typeof(HashSet<>) ||
                   type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISet<>));
        }

        internal static Type GetIterableType(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            var genericArguments = type.GetGenericArguments();

            return genericArguments.FirstOrDefault();
        }

        internal static Type GetDictionaryType(this Type type)
        {
            var genericArguments = type.GetGenericArguments();

            Debug.Assert(typeof(IDictionary<,>)
                    .MakeGenericType(genericArguments)
                    .IsAssignableFrom(type)
            );

            return typeof(KeyValuePair<,>).MakeGenericType(genericArguments);
        }

        internal static string ToCamelCase(this string val)
        {
            return $"{char.ToLowerInvariant(val[0])}{val.Substring(1)}";
        }

        public static object[] ToArray(this IEnumerator enumerator)
        {
            var list = new List<object>();

            while (enumerator.MoveNext())
            {
                var item = enumerator.Current;
                list.Add(item);
            }

            return list.ToArray();
        }
    }
}