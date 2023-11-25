using Microsoft.CodeAnalysis;

namespace DddModelGen
{
    internal static class Util
    {

        public static string CreateFileName(INamedTypeSymbol typeSymbol, string key) =>
            $"{typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                .Replace("global::", "")
                .Replace("<", "_")
                .Replace(">", "_")}.{key}.g.cs";

        public static (AttributeData? attr, int index) GetMainAttributeData(INamedTypeSymbol typeSymbol)
            => GetAttributeData(typeSymbol, Constants.NamespaceName, Constants.MainAttributeClassName);

        public static (AttributeData? attr, int index) GetEfConverterAttributeData(INamedTypeSymbol typeSymbol)
            => GetAttributeData(typeSymbol, Constants.NamespaceName, Constants.EfConverterAttributeClassName);

        public static (AttributeData? attr, int index) GetAttributeData(INamedTypeSymbol typeSymbol, string ns, string name)
        {
            for (var i = 0; i < typeSymbol.GetAttributes().Length; i++)
            {
                var attr = typeSymbol.GetAttributes()[i];
                if (attr.AttributeClass?.ContainingNamespace.Name == ns && attr.AttributeClass?.Name == name)
                {
                    return (attr, i);
                }
            }
            return (null, -1);
        }

        public static string? GetFullName(INamedTypeSymbol? symbol)
        {
            if (symbol == null)
                return null;

            var prefix = GetFullNamespace(symbol);
            var suffix = "";
            if (symbol.Arity > 0)
            {
                suffix = $"<{string.Join(", ", symbol.TypeArguments.Select(targ => GetFullName((INamedTypeSymbol)targ)))}>";
            }

            return prefix != "" ? $"{prefix}.{symbol.Name}{suffix}" : symbol.Name + suffix;
        }

        public static string GetFullNamespace(INamedTypeSymbol symbol)
        {
            var parts = new Stack<string>();
            var current = symbol.ContainingNamespace;
            while (current != null && !current.IsGlobalNamespace)
            {
                parts.Push(current.Name);
                current = current.ContainingNamespace;
            }
            return string.Join(".", parts);
        }

        public static INamedTypeSymbol? GetTypeSymbolFromFirstArgument(AttributeData? attr)
        {
            if (attr is null) return null;
            if (attr.ConstructorArguments.Length != 1) return null;
            var firstArg = attr.ConstructorArguments[0];
            if (firstArg.Kind != TypedConstantKind.Type) return null;
            return (INamedTypeSymbol)firstArg.Value!;
        }

        public static string GetCSharpAccesibilityString(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Public => "public",
                Accessibility.Protected => "protected",
                Accessibility.Private => "private",
                Accessibility.Internal => "internal",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.ProtectedAndInternal => "private protected",
                _ => throw new NotImplementedException(),
            };
        }   
    }
}
