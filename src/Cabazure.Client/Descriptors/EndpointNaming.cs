using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cabazure.Client.SourceGenerator.Descriptors;

internal static class EndpointNaming
{
    public static (string InterfaceName, string ClassName) GetNames(InterfaceDeclarationSyntax syntax)
    {
        var interfaceName = syntax.Identifier.ValueText;
        var className = interfaceName.Length > 1 && interfaceName[0] == 'I'
            ? interfaceName.Substring(1)
            : interfaceName + "_Implementation";

        var interfaceParent = syntax.Parent;
        while (interfaceParent is ClassDeclarationSyntax c)
        {
            interfaceName = string.Concat(c.Identifier.ValueText, ".", interfaceName);
            interfaceParent = c.Parent;
        }

        return (interfaceName, className);
    }
}
