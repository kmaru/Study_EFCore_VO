namespace DddModelGen.Codes
{
    internal static class MainAttributeWriter
    {
        public static string Write()
        {
            return $$"""
{{Constants.SharedCodeHeader}}

using System;

namespace {{Constants.NamespaceName}};

[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class {{Constants.MainAttributeClassName}} : Attribute
{
    public {{Constants.MainAttributeClassName}}(Type type)
    {
        Type = type;
    }

    public Type Type { get; }

    public bool EfValueConverter { get; set; } = false;

    public string? EfValueConverterNamespace { get; set; } = null;
}
""";
        }
    }
}
