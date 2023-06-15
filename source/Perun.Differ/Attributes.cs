using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DotNet.Differ
{
    /// <summary>
    /// Keeps property and subsequent children in audit diff even if no change was made.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class KeepInDiffAttribute : Attribute
    {
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
        Ignore = 2
    }

    internal sealed class DiffCollection
    {
        public Dictionary<string, Difference> Diffs { get; set; } = new Dictionary<string, Difference>();

        public List<Difference> KeepDiffs { get; set; } = new List<Difference>();
        public HashSet<string> KeepPaths { get; set; } = new HashSet<string>();

        public HashSet<string> IgnorePaths { get; set; } = new HashSet<string>();
    }
}