using DddModelGen.Codes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.CompilerServices;

namespace DddModelGen;

[Generator(LanguageNames.CSharp)]
public class StronglyTypedValueGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource($"{Constants.MainAttributeClassName}.cs", MainAttributeWriter.Write()));

        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            $"{Constants.NamespaceName}.{Constants.MainAttributeClassName}",
            static (node, token) => true, 
            static (context, token) => context);
        
        context.RegisterSourceOutput(source, Emit);
    }

    private static void Emit(SourceProductionContext context, GeneratorAttributeSyntaxContext source)
    {
        var typeNode = (TypeDeclarationSyntax)source.TargetNode;
        var typeSymbol = (INamedTypeSymbol)source.TargetSymbol;

        var (attrData, _) = Util.GetMainAttributeData(typeSymbol);
        if(attrData is null) return;

        var targetTypeSymbol = Util.GetTypeSymbolFromFirstArgument(attrData);
        if (targetTypeSymbol is null) return;
        var targetTypeFullName = Util.GetFullName(targetTypeSymbol);
        if (targetTypeFullName is null) return;

        if (!IsSupportedType(targetTypeFullName))
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.NotSupportedAtomicType, typeNode.AttributeLists[0].GetLocation(), targetTypeSymbol.Name));
            return;
        }

        var ns = Util.GetFullNamespace(typeSymbol);

        var targetNullable = targetTypeSymbol.IsReferenceType ? "?" : "";
        var selfNullable = typeSymbol.IsReferenceType ? "?" : "";

        var implements = new List<string>
        {
            $"IEqualityOperators<{typeSymbol.Name}, {targetTypeFullName}, bool>"
        };
        if (!typeSymbol.IsRecord) {
            implements.Add($"IEqualityOperators<{typeSymbol.Name}, {typeSymbol.Name}, bool>");
            implements.Add($"IEquatable<{typeSymbol.Name}>");
        }
        if((CanComparisonType(targetTypeFullName)))
        {
            implements.Add($"IComparisonOperators<{typeSymbol.Name}, {targetTypeFullName}, bool>");
            implements.Add($"IComparisonOperators<{typeSymbol.Name}, {typeSymbol.Name}, bool>");
        }

        var code = $$"""
{{Constants.SharedCodeHeader}}

using System.Numerics;

{{(string.IsNullOrWhiteSpace(ns) ? "" : "namespace " + ns + ";")}}

[System.ComponentModel.TypeConverter(typeof({{typeSymbol.Name}}TypeConverter))]
partial {{GetTypeKindDescriptionText(typeSymbol)}} {{typeSymbol.Name}}: {{ string.Join(", ", implements) }} 
{
    private readonly {{targetTypeFullName}} _value;

    private {{typeSymbol.Name}}({{targetTypeFullName}} value)
    {
        {{GetNullCheckCode(targetTypeSymbol)}}
        _value = value;
    }

    public {{targetTypeFullName}} GetAtomicValue() => _value;

    public static implicit operator {{targetTypeFullName}}({{typeSymbol.Name}} value) => value._value;

    public static bool operator ==({{typeSymbol.Name}}{{selfNullable}} left, {{targetTypeFullName}}{{targetNullable}} right) => left{{selfNullable}}._value == right;

    public static bool operator !=({{typeSymbol.Name}}{{selfNullable}} left, {{targetTypeFullName}}{{targetNullable}} right) => left{{selfNullable}}._value != right;
   
{{GetNonRecordMethods(typeSymbol, selfNullable)}}
{{GetComparisonMethods(typeSymbol, targetTypeFullName)}}

    public override string ToString() => _value.ToString();

    static partial void Validate({{targetTypeFullName}} value, List<string> messages);

    public static {{typeSymbol.Name}} Create({{targetTypeFullName}} value)
    {
        var messages = new List<string>();
        Validate(value, messages);
        if (messages.Any()) throw new ArgumentException(string.Join("¥n", messages));
        return new(value);
    }

    private class {{typeSymbol.Name}}TypeConverter: System.ComponentModel.TypeConverter
    {
        private static readonly Type StronglyType = typeof({{typeSymbol.Name}});
        private static readonly Type AtomicType = typeof({{targetTypeFullName}});
        private static readonly System.ComponentModel.TypeConverter AtomicTypeConverter = System.ComponentModel.TypeDescriptor.GetConverter(AtomicType);

        public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == StronglyType || sourceType == AtomicType)
            {
                return true;
            }
            var atomicConverterResult = AtomicTypeConverter.CanConvertFrom(context, sourceType);
            return atomicConverterResult || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(System.ComponentModel.ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
        {
            if (value != null)
            {
                if (value is {{typeSymbol.Name}} result1) return result1;
                if (value is {{targetTypeFullName}} result2) return {{typeSymbol.Name}}.Create(result2);
                if (AtomicTypeConverter.CanConvertFrom(context, value.GetType()))
                {
                    var convertedValue = AtomicTypeConverter.ConvertFrom(context, culture, value);
                    if(convertedValue is {{targetTypeFullName}} cv)
                    {
                        return {{typeSymbol.Name}}.Create(cv);
                    }
                }
            }
            return base.ConvertFrom(context, culture, value!);
        }

        public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext? context, Type? destinationType)
        {
            if (destinationType == StronglyType || destinationType == AtomicType)
            {
                return true;
            }
            var atomicConverterResult = AtomicTypeConverter.CanConvertTo(context, destinationType);
            return atomicConverterResult || base.CanConvertTo(context, destinationType);
        }

        public override object? ConvertTo(System.ComponentModel.ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is {{typeSymbol.Name}} typedValue)
            {
                if (destinationType == StronglyType)
                {
                    return typedValue;
                }

                if (destinationType == AtomicType)
                {
                    return typedValue._value;
                }

                if(AtomicTypeConverter.CanConvertTo(context, destinationType))
                {
                    return AtomicTypeConverter.ConvertTo(context, culture, typedValue._value, destinationType);
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
""";

        context.AddSource(Util.CreateFileName(typeSymbol, "StronglyTypedValueGenerator"), code);

        //------------------

        var createEfConverter = attrData.NamedArguments.Where(d => d.Key == "EfValueConverter").FirstOrDefault().Value.Value as bool? ?? false;
        var efConverterNamespace = attrData.NamedArguments.Where(d => d.Key == "EfValueConverterNamespace").FirstOrDefault().Value.Value as string ?? ns;

        if (createEfConverter)
        {
            var efConverterName = $"{typeSymbol.Name}EfValueConverter";
            var efConverterCode = EfValueConverterWriter.Write(
                efConverterNamespace,
                Util.GetCSharpAccesibilityString(typeSymbol.DeclaredAccessibility),
                efConverterName, 
                typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                targetTypeFullName);
            context.AddSource(Util.CreateFileName(typeSymbol, "EfValueConverter"), efConverterCode);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetComparisonMethods(INamedTypeSymbol typeSymbol, string targetTypeFullName)
    {
        if (!CanComparisonType(targetTypeFullName)) return "";

        return $$"""
    public static bool operator >({{typeSymbol.Name}} left, {{targetTypeFullName}} right) => left._value > right;

    public static bool operator >=({{typeSymbol.Name}} left, {{targetTypeFullName}} right) => left._value >= right;

    public static bool operator <({{typeSymbol.Name}} left, {{targetTypeFullName}} right) => left._value < right;

    public static bool operator <=({{typeSymbol.Name}} left, {{targetTypeFullName}} right) => left._value <= right;

    public static bool operator >({{typeSymbol.Name}} left, {{typeSymbol.Name}} right) => left._value > right._value;

    public static bool operator >=({{typeSymbol.Name}} left, {{typeSymbol.Name}} right) => left._value >= right._value;

    public static bool operator <({{typeSymbol.Name}} left, {{typeSymbol.Name}} right) => left._value < right._value;

    public static bool operator <=({{typeSymbol.Name}} left, {{typeSymbol.Name}} right) => left._value <= right._value;
""";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetNonRecordMethods(INamedTypeSymbol typeSymbol, string selfNullable)
    {
        if (typeSymbol.IsRecord) return "";

        return $$"""
    public static bool operator ==({{typeSymbol.Name}}{{selfNullable}} left, {{typeSymbol.Name}}{{selfNullable}} right) => left{{selfNullable}}._value == right{{selfNullable}}._value;

    public static bool operator !=({{typeSymbol.Name}}{{selfNullable}} left, {{typeSymbol.Name}}{{selfNullable}} right) => left{{selfNullable}}._value != right{{selfNullable}}._value;

    public bool Equals({{typeSymbol.Name}}{{selfNullable}} other) => other is { } target && _value == target._value;

    public override bool Equals(object? obj) => obj is {{typeSymbol.Name}} target && _value == target._value;

    public override int GetHashCode() => _value.GetHashCode();
""";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetNullCheckCode(INamedTypeSymbol typeSymbol) 
        => typeSymbol.IsReferenceType ? "if (value is null) throw new ArgumentNullException(nameof(value));" : "";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetTypeKindDescriptionText(INamedTypeSymbol typeSymbol) =>
        // record なら record を返す
        typeSymbol.TypeKind == TypeKind.Class
            ? typeSymbol.IsRecord ? "record" : "class"
            : typeSymbol.IsRecord ? "record struct" : "struct";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSupportedType(string targetTypeFullName)
    {
        // ここにサポートするタイプを列挙する
        // 文字、数字(整数、浮動小数など)、真偽値、GUIDのみをサポートする
        // それ以外はサポートしない
        return targetTypeFullName switch
        {
            "System.String" => true,
            "System.Int16" => true,
            "System.Int32" => true,
            "System.Int64" => true,
            "System.UInt16" => true,
            "System.UInt32" => true,
            "System.UInt64" => true,
            "System.Single" => true,
            "System.Double" => true,
            "System.Decimal" => true,
            "System.Boolean" => true,
            "System.Guid" => true,
            "System.DateTime" => true,
            "System.DateTimeOffset" => true,
            "System.DateOnly" => true,
            "System.TimeOnly" => true,
            _ => false,
        };
        
    }

    private static bool CanComparisonType(string targetTypeFullName)
    {
        // 大小比較ができるタイプを列挙する
        return targetTypeFullName switch
        {
            "System.Int16" => true,
            "System.Int32" => true,
            "System.Int64" => true,
            "System.UInt16" => true,
            "System.UInt32" => true,
            "System.UInt64" => true,
            "System.Single" => true,
            "System.Double" => true,
            "System.Decimal" => true,
            "System.DateTime" => true,
            "System.DateTimeOffset" => true,
            "System.DateOnly" => true,
            "System.TimeOnly" => true,
            _ => false,
        };
    }
}
