using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DddModelGen.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AvoidDefaultAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(Descriptors.DoNotInitializeByDefaultExpressoinOrLiteral);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.DefaultExpression);
        context.RegisterSyntaxNodeAction(AnalyzeLiteralNode, SyntaxKind.DefaultLiteralExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var defaultExpression = (DefaultExpressionSyntax)context.Node;

        var typeSymbol = context.SemanticModel.GetTypeInfo(defaultExpression).Type as INamedTypeSymbol;
        if (typeSymbol is null)
        {
            return;
        }

        // Check for the StronglyTypedValueAttribute
        var hasStronglyTypedValueAttribute = typeSymbol.GetAttributes().Any(attr => attr.AttributeClass?.ContainingNamespace.Name == "DddModels" && attr.AttributeClass?.Name == "StronglyTypedValueAttribute");
        if (!hasStronglyTypedValueAttribute) return;

        var diagnostic = Diagnostic.Create(Descriptors.DoNotInitializeByDefaultExpressoinOrLiteral, defaultExpression.GetLocation(), typeSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private void AnalyzeLiteralNode(SyntaxNodeAnalysisContext context)
    {
        var literalExpression = (LiteralExpressionSyntax)context.Node;
        if (literalExpression.Kind() != SyntaxKind.DefaultLiteralExpression)
        {
            return;
        }
        var typeSymbol = context.SemanticModel.GetTypeInfo(literalExpression).Type as INamedTypeSymbol;
        if (typeSymbol is null)
        {
            return;
        }

        // Check for the StronglyTypedValueAttribute
        var hasStronglyTypedValueAttribute = typeSymbol.GetAttributes().Any(attr => attr.AttributeClass?.ContainingNamespace.Name == "DddModels" && attr.AttributeClass?.Name == "StronglyTypedValueAttribute");
        if (!hasStronglyTypedValueAttribute) return;

        var diagnostic = Diagnostic.Create(Descriptors.DoNotInitializeByDefaultExpressoinOrLiteral, literalExpression.GetLocation(), typeSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }
}
