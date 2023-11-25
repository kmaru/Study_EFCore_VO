using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DddModelGen.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AvoidNewAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
        = ImmutableArray.Create(Descriptors.DoNotInitializeByDefaultConstructor);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ObjectCreationExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var objectCreationExpression = (ObjectCreationExpressionSyntax)context.Node;

        var typeSymbol = context.SemanticModel.GetTypeInfo(objectCreationExpression).Type as INamedTypeSymbol;
        if (typeSymbol == null || !typeSymbol.IsValueType)
        {
            return;
        }

        // Check for the StronglyTypedValueAttribute
        var hasStronglyTypedValueAttribute = typeSymbol.GetAttributes().Any(attr => attr.AttributeClass?.ContainingNamespace.Name == "DddModels" && attr.AttributeClass?.Name == "StronglyTypedValueAttribute");
        if (!hasStronglyTypedValueAttribute) return;
        var argCount = objectCreationExpression.ArgumentList?.Arguments.Count ?? 0;
        if (argCount > 0) return;

        var diagnostic = Diagnostic.Create(Descriptors.DoNotInitializeByDefaultConstructor, objectCreationExpression.GetLocation(), typeSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }
}
