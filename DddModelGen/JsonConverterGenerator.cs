using DddModelGen.Codes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DddModelGen;

[Generator(LanguageNames.CSharp)]
public class JsonConverterGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource($"{Constants.JsonConverterAttributeClassName}.cs", JsonConverterAttributeWriter.Write()));

        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            $"{Constants.NamespaceName}.{Constants.JsonConverterAttributeClassName}", 
            static (node, token) => true,
            static (context, token) => context);

        context.RegisterSourceOutput(source, Emit);
    }

    private static void Emit(SourceProductionContext context, GeneratorAttributeSyntaxContext source)
    {
        var typeNode = (TypeDeclarationSyntax)source.TargetNode;
        var typeSymbol = (INamedTypeSymbol)source.TargetSymbol;
        var (attr, index) = Util.GetJsonConverterAttributeData(typeSymbol);
        var strongTypeSymbol = Util.GetTypeSymbolFromFirstArgument(attr);
        if (strongTypeSymbol is null) return;

        var strongTypeFullName = Util.GetFullName(strongTypeSymbol);
        if (strongTypeFullName is null) return;
        
        var (mainAttrData, _) = Util.GetMainAttributeData(strongTypeSymbol);
        if(mainAttrData is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.NoStronglyTypedValueAttribute, typeNode.AttributeLists[index].GetLocation(), strongTypeFullName));
            return;
        }

        var targetTypeSymbol = Util.GetTypeSymbolFromFirstArgument(mainAttrData);
        if (targetTypeSymbol is null) return;
        var targetTypeFullName = Util.GetFullName(targetTypeSymbol);

        var code = JsonConverterWriter.Write(
            Util.GetFullNamespace(typeSymbol),
            Util.GetCSharpAccesibilityString(typeSymbol.DeclaredAccessibility),
            typeSymbol.Name,
            strongTypeFullName, 
            targetTypeFullName!);
        
        context.AddSource(Util.CreateFileName(typeSymbol, "JsonConverter"), code);
    }

}
