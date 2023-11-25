using Microsoft.CodeAnalysis;

namespace DddModelGen;

public static class Descriptors
{
    public static readonly DiagnosticDescriptor DoNotInitializeByDefaultConstructor = new(
        id: "StronglyTypedValue001",
        title: $"Avoid default constructor on type with {Constants.MainAttributeClassName}",
        messageFormat: "Type '{0}' with " + Constants.MainAttributeClassName + " should not be instantiated with default constructor",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: $"Type with {Constants.MainAttributeClassName} should be instantiated with creation method.");

    public static readonly DiagnosticDescriptor NotSupportedAtomicType = new(
        id: "StronglyTypedValue002",
        title: "Not supported specific type",
        messageFormat: "Type '{0}' is not supported by " + Constants.MainAttributeClassName,
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: $"Only certain types are supported by {Constants.MainAttributeClassName}.");

    public static readonly DiagnosticDescriptor NoStronglyTypedValueAttribute = new(
        id: "StronglyTypedValue003",
        title: $"No {Constants.MainAttributeClassName} specific type",
        messageFormat: "Type '{0}' is not has " + Constants.MainAttributeClassName ,
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: $"{Constants.MainAttributeClassName} must be set for the specified type.");

    public static readonly DiagnosticDescriptor DoNotInitializeByDefaultExpressoinOrLiteral = new(
        id: "StronglyTypedValue004",
        title: $"Avoid default expresson on type with {Constants.MainAttributeClassName}",
        messageFormat: "Type '{0}' with " + Constants.MainAttributeClassName + " should not be instantiated with default",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: $"Type with {Constants.MainAttributeClassName} should be instantiated with creation method.");
}