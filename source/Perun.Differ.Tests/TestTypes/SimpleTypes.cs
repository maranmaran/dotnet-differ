using System;
using System.Net;

namespace Differ.DotNet.Tests.TestTypes
{
    public class SimpleTypes
    {
        public string String { get; set; }
        public Uri Uri { get; set; }
        public int Int { get; set; }
        public int? NullableInt { get; set; }
        public Guid Guid { get; set; }
        public Guid? NullableGuid { get; set; }
        public HttpStatusCode Enum { get; set; }
        public HttpStatusCode? NullableEnum { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime? NullableDateTime { get; set; }
        public TimeSpan DateTimeOffsetPrimitive { get; set; }
        public TimeSpan? NullableTimeSpan { get; set; }
    }
}