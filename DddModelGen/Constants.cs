﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DddModelGen
{
    internal static class Constants
    {
        public const string NamespaceName = "DddModels";

        public const string StronglyTypedValueAttrClassName = "StronglyTypedValueAttribute";

        public const string StronglyTypedValueAttrCode = $$"""
// <auto-generated>
// THIS (.cs) FILE IS GENERATED. DO NOT CHANGE IT.
// </auto-generated>
#nullable enable

using System;

namespace {{Constants.NamespaceName}};

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class {{Constants.StronglyTypedValueAttrClassName}} : Attribute
{
    public {{Constants.StronglyTypedValueAttrClassName}}(Type type)
    {
        Type = type;
    }

    public Type Type { get; }

    public bool EfValueConverter { get; set; } = false;

    public string? EfValueConverterNamespace { get; set; } = null;
}
""";

        public const string StronglyTypedValueEfConverterAttrClassName = "StronglyTypedValueEfConverterAttribute";

        public const string StronglyTypedValueEfConverterAttrCode = $$"""
// <auto-generated>
// THIS (.cs) FILE IS GENERATED. DO NOT CHANGE IT.
// </auto-generated>
#nullable enable

using System;

namespace {{NamespaceName}};

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class {{StronglyTypedValueEfConverterAttrClassName}} : Attribute
{
    public {{StronglyTypedValueEfConverterAttrClassName}}(Type type)
    {
        Type = type;
    }

    public Type Type { get; }
}
""";

    }
}