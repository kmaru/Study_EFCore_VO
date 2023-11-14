using DddModelGen.Codes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.CompilerServices;

namespace DddModelGen;

[Generator(LanguageNames.CSharp)]
public class EfConverterGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource($"{Constants.StronglyTypedValueEfConverterAttrClassName}.cs", Constants.StronglyTypedValueEfConverterAttrCode));

        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            $"{Constants.NamespaceName}.{Constants.StronglyTypedValueEfConverterAttrClassName}", 
            static (node, token) => true,
            static (context, token) => context);

        context.RegisterSourceOutput(source, Emit);
    }

    private static void Emit(SourceProductionContext context, GeneratorAttributeSyntaxContext source)
    {
        var typeNode = (TypeDeclarationSyntax)source.TargetNode;
        var typeSymbol = (INamedTypeSymbol)source.TargetSymbol;
        var (_, index) = GetAttributeData(typeSymbol, Constants.NamespaceName, Constants.StronglyTypedValueEfConverterAttrClassName);
        var stvTypeSymbol = GetTypeSymbolByAttributeFirstArgument(typeSymbol, Constants.NamespaceName, Constants.StronglyTypedValueEfConverterAttrClassName);
        if (stvTypeSymbol is null) return;
        var stvTypeFullName = GetFullName(stvTypeSymbol);
        if (stvTypeFullName is null) return;
        
        var (stvAttrData, _) = GetAttributeData(stvTypeSymbol, Constants.NamespaceName, Constants.StronglyTypedValueAttrClassName);
        if(stvAttrData is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.NoStronglyTypedValueAttribute, typeNode.AttributeLists[index].GetLocation(), stvTypeFullName));
            return;
        }

        var targetTypeSymbol = GetTypeSymbolByAttributeFirstArgument(stvTypeSymbol, Constants.NamespaceName, Constants.StronglyTypedValueAttrClassName);
        if (targetTypeSymbol is null) return;
        var targetTypeFullName = GetFullName(targetTypeSymbol);

        var code = EfValueConverterWriter.Write(
            typeSymbol.ContainingNamespace.IsGlobalNamespace ? null : typeSymbol.ContainingNamespace.Name,
            typeSymbol.Name, stvTypeFullName, targetTypeFullName);
        
        context.AddSource(CreateFileName(typeSymbol, "EfValueConverter"), code);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetFullName(INamedTypeSymbol symbol) => $"{symbol.ContainingNamespace}.{symbol.Name}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (AttributeData? Attr, int Index) GetAttributeData(INamedTypeSymbol typeSymbol, string ns, string name)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static INamedTypeSymbol? GetTypeSymbolByAttributeFirstArgument(INamedTypeSymbol typeSymbol, string ns, string name)
    {
        var (attr, _) = GetAttributeData(typeSymbol, ns, name);
        if (attr is null) return null;
        if (attr.ConstructorArguments.Length != 1) return null;
        var firstArg = attr.ConstructorArguments[0];
        if (firstArg.Kind != TypedConstantKind.Type) return null;
        return (INamedTypeSymbol)firstArg.Value!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string CreateFileName(INamedTypeSymbol typeSymbol, string key) => 
        $"{typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "")
            .Replace("<", "_")
            .Replace(">", "_")}.{key}.g.cs";
}
