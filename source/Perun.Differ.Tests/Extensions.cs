using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Differ.DotNet.Tests
{
    internal static class Extensions
    {
        public static List<PropertyInfo> GetPropertiesFlat(this Type type, PropertyInfo propInfo = null)
        {
            var props = new List<PropertyInfo>();

            if (type.IsSimple())
            {
                props.Add(propInfo);
                return props;
            }

            if (type.IsComplex())
            {
                foreach (var prop in type.GetProperties())
                {
                    props.AddRange(GetPropertiesFlat(prop.PropertyType, prop));
                }

                return props;
            }

            if (type.IsIterable())
            {
                props.AddRange(GetPropertiesFlat(type.GetIterableType()));

                return props;
            }

            return props;
        }

        public static void AssertIterable<T>(
            IEnumerable<T> leftL,
            IEnumerable<T> rightL,
            IEnumerable<Difference> differences
        )
        {
            for (var i = 0; i < Math.Max(leftL.Count(), rightL.Count()); i++)
            {
                var left = leftL.ElementAtOrDefault(i);
                var right = rightL.ElementAtOrDefault(i);
                var difference = differences.ElementAtOrDefault(i);

                Assert.Equal(left, difference.OldValue);
                Assert.Equal(right, difference.NewValue);
            }
        }
    }
}