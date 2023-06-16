using System.Linq;
using JetBrains.Annotations;

namespace Differ.DotNet
{
    /// <summary>
    /// Represents found difference between two provided objects
    /// </summary>
    [PublicAPI]
    public sealed class Difference
    {
        public string FullPath { get; set; }
        public string FieldPath { get; set; }
        public string FieldName { get; set; }

        public object LeftValue { get; set; }
        public object RightValue { get; set; }

        public string CustomFullPath { get; set; }
        public string CustomFieldPath { get; set; }
        public string CustomFieldName { get; set; }

        public Difference()
        {
        }

        public Difference(string fullPath, string customFullPath, object leftValue, object rightValue)
        {
            LeftValue = leftValue;
            RightValue = rightValue;

            (FullPath, FieldName, FieldPath) = SetPath(fullPath);
            if (fullPath != customFullPath && customFullPath != null)
            {
                (CustomFullPath, CustomFieldName, CustomFieldPath) = SetPath(customFullPath);
            }
        }

        private (string, string, string) SetPath(string fullPath)
        {
            if (fullPath == null)
            {
                return (null, null, null);
            }

            var pathSplit = fullPath.Split('.');
            var fieldName = pathSplit.LastOrDefault();
            var fieldPath = string.Join(".", pathSplit.Take(pathSplit.Length - 1));

            return (fullPath, fieldName, fieldPath);
        }
    }
}