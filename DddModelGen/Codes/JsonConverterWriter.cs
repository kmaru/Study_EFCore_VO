namespace DddModelGen.Codes;

internal class JsonConverterWriter
{
    public static string Write(string? ns, string accessibility, string name, string type, string atomType)
    {
        return $$"""
{{Constants.SharedCodeHeader}}

using System.Text.Json;
using System.Text.Json.Serialization;

{{(ns is null ? "" : "namespace " + ns + ";")}}

{{accessibility}} partial class {{name}} : JsonConverter<{{type}}>
{
    public override {{type}} Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var converter = options.GetConverter(typeof({{atomType}})) as JsonConverter<{{atomType}}>;
        if (converter is null)
        {
            throw new JsonException($"{typeof({{atomType}})} converter not found.");
        }
        var value = converter.Read(ref reader, typeof({{atomType}}), options);
        return {{type}}.Create(value);
    }

    public override void Write(Utf8JsonWriter writer, {{type}} value, JsonSerializerOptions options)
    {
        var converter = options.GetConverter(typeof({{atomType}})) as JsonConverter<{{atomType}}>;
        if (converter is null)
        {
            throw new JsonException($"{typeof({{atomType}})} converter not found.");
        }
        converter.Write(writer, value.GetAtomicValue(), options);
    }
}
""";


    }
}