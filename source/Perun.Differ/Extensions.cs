using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

        internal static bool IsIterable(this Type type)
        {
            if (type.IsArray || type == typeof(IEnumerable) || type.GetInterfaces().Contains(typeof(IEnumerable)))
            {
                return true;
            }

            return false;
        }

        internal static Type GetIterableType(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }

            var genericArguments = type.GetGenericArguments();

            if (genericArguments.Length == 2)
            {
                var dictionaryType = typeof(IDictionary<,>).MakeGenericType(genericArguments);
                if (dictionaryType.IsAssignableFrom(type))
                {
                    return typeof(KeyValuePair<,>).MakeGenericType(genericArguments);
                }
            }

            return genericArguments.FirstOrDefault();
        }

        internal static string ToCamelCase(this string val)
        {
            return $"{char.ToLowerInvariant(val[0])}{val.Substring(1)}";
        }
    }
}