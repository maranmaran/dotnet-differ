using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Differ.DotNet
{
    /// <summary>
    /// Keeps property and subsequent children in audit diff even if no change was made.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class KeepInDiffAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether attribute should be ignored if sibling or child diffs exist.
        /// </summary>
        /// <value>
        ///     If <c>true</c> attribute is ignored (not kept), if no sibling or child diffs exist.
        ///     If <c>false</c> values are always kept.
        /// </value>
        public bool IgnoreIfNoSiblingOrChildDiff { get; set; }
    }

    /// <summary>
    /// Ignores property and subsequent children in audit diff.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IgnoreInDiffAttribute : Attribute
    {
    }

    /// <summary>
    /// Switches array diffing from index based to key-value based
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DiffArrayIdPropertyName : Attribute
    {
        public string Name { get; }

        public DiffArrayIdPropertyName(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Sets custom property name in diff.
    /// <see cref="Difference.CustomFieldName"/>
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DiffPropertyName : Attribute
    {
        public string Name { get; }

        public DiffPropertyName(string name)
        {
            Name = name;
        }
    }

    [Flags]
    internal enum DiffActions
    {
        Default = 0,
        Keep = 1,
        Ignore = 2,
        KeepOptional = 4
    }

    internal sealed class DiffCollection
    {
        public Dictionary<string, Difference> Diffs { get; set; } = new(); // (FullPath, Diff)

        public HashSet<Difference> KeepDiffs { get; set; } = new();
        public HashSet<string> IgnorePaths { get; set; } = new(); // FullPath
    }
}