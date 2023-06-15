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

        public Difference(string fullPath, string customFullPath, object leftValue, object rightValue)
        {
            FullPath = fullPath;

            var fullPathSplit = FullPath.Split('.');
            FieldName = FullPath.Split('.').LastOrDefault();
            FieldPath = string.Join(".", fullPathSplit.Take(fullPathSplit.Length - 1));

            if (fullPath != customFullPath)
            {
                CustomFullPath = customFullPath;

                var customSplit = CustomFullPath.Split('.');

                CustomFieldName = CustomFullPath.Split('.').LastOrDefault();
                CustomFieldPath = string.Join(".", customSplit.Take(customSplit.Length - 1));
            }

            LeftValue = leftValue;
            RightValue = rightValue;
        }
    }
}