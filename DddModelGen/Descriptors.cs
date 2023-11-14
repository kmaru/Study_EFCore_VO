using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace DddModelGen
{
    public static class Descriptors
    {
        public static readonly DiagnosticDescriptor DoNotInitializeByDefaultConstructor = new(
            id: "StronglyTypedValue001",
            title: "Avoid default constructor on struct with StronglyTypedValueAttribute",
            messageFormat: "Struct '{0}' with StronglyTypedValueAttribute should not be instantiated with default constructor",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Structs with StronglyTypedValueAttribute should be instantiated with specific constructors.");

        public static readonly DiagnosticDescriptor NotSupportedAtomicType = new(
            id: "StronglyTypedValue002",
            title: "Not supported specific type",
            messageFormat: "Type '{0}' is not supported by StronglyTypedValueAttribute",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "Only certain types are supported by StronglyTypedValueAttribute.");

        public static readonly DiagnosticDescriptor NoStronglyTypedValueAttribute = new(
            id: "StronglyTypedValue003",
            title: "No StronglyTypedValueAttribute specific type",
            messageFormat: "Type '{0}' is not has StronglyTypedValueAttribute",
            category: "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "StronglyTypedValueAttribute must be set for the specified type.");
    }
}
