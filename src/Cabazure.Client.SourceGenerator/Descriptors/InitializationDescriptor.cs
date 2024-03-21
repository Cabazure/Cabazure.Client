using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator.Descriptors;

public record InitializationDescriptor(
    InitializationClassDescriptor Class,
    InitializationMethodDescriptor Method,
    string ClientName)
{
    public static InitializationDescriptor? Create(
        Action<Diagnostic> diagnostics,
        SemanticModel semanticModel,
        AttributeSyntax attribute)
    {
        var expression = attribute.ArgumentList?.Arguments.Select(a => a.Expression).FirstOrDefault();
        if (expression == null)
        {
            return null;
        }

        var clientName = semanticModel.GetConstantValue(expression).Value as string;
        if (clientName == null)
        {
            return null;
        }

        var classDescriptor = InitializationClassDescriptor.Create(diagnostics, attribute);
        if (classDescriptor == null)
        {
            return null;
        }

        var methodDescriptor = InitializationMethodDescriptor.Create(diagnostics, semanticModel, attribute);
        if (methodDescriptor == null)
        {
            return null;
        }

        return new InitializationDescriptor(
            classDescriptor,
            methodDescriptor,
            clientName);
    }
}
