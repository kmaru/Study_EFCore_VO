namespace DddModelGen.Codes;

internal static class JsonConverterAttributeWriter
{
    public static string Write()
    {
        return $$"""
{{Constants.SharedCodeHeader}}

using System;

namespace {{Constants.NamespaceName}};

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class {{Constants.JsonConverterAttributeClassName}} : Attribute
{
    public {{Constants.JsonConverterAttributeClassName}}(Type type)
    {
        Type = type;
    }

    public Type Type { get; }
}
""";
    }
}
