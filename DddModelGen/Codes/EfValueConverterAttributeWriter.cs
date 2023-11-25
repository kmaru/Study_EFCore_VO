namespace DddModelGen.Codes
{
    internal static class EfValueConverterAttributeWriter
    {
        public static string Write()
        {
            return $$"""
{{Constants.SharedCodeHeader}}

using System;

namespace {{Constants.NamespaceName}};

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class {{Constants.EfConverterAttributeClassName}} : Attribute
{
    public {{Constants.EfConverterAttributeClassName}}(Type type)
    {
        Type = type;
    }

    public Type Type { get; }
}
""";
        }
    }
}
