﻿namespace DddModelGen.Codes
{
    internal static class EfValueConverterWriter
    {
        public static string Write(string? ns, string name, string type, string atomType)
        {
            return $$"""
// <auto-generated>
// THIS (.cs) FILE IS GENERATED. DO NOT CHANGE IT.
// </auto-generated>
#nullable enable

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

{{(ns is null ? "" : "namespace " + ns + ";")}}

partial class {{name}} : ValueConverter<{{type}}, {{atomType}}>
{
    public {{name}}()
        : this(null)
    {
    }

    public {{name}}(ConverterMappingHints? mappingHints = null)
        : base(id => id.GetAtomicValue(), value => {{type}}.Create(value), mappingHints)
    {
    }
}
""";


        }
    }
}
