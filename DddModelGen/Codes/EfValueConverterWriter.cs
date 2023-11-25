namespace DddModelGen.Codes;

internal static class EfValueConverterWriter
{
    public static string Write(string? ns, string accessibility, string name, string type, string atomType)
    {
        return $$"""
{{Constants.SharedCodeHeader}}

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

{{(ns is null ? "" : "namespace " + ns + ";")}}

{{accessibility}} partial class {{name}} : ValueConverter<{{type}}, {{atomType}}>
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

{{accessibility}} static class {{name}}Extensions
{
    public static Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<{{type}}> HasStronglyTypedValueConversion(
        this Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<{{type}}> propertyBuilder) =>
            propertyBuilder.HasConversion<{{name}}>();

    public static Microsoft.EntityFrameworkCore.Metadata.Builders.PropertiesConfigurationBuilder<{{type}}> HaveStronglyTypedValueConversion(
        this Microsoft.EntityFrameworkCore.Metadata.Builders.PropertiesConfigurationBuilder<{{type}}> propertyBuilder) =>
            propertyBuilder.HaveConversion<{{name}}>();
}
""";


    }
}
