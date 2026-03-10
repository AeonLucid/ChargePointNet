using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ChargePointNet.Packets.Generator.Extensions;

internal static class TypeSyntaxExtensions
{
    public static bool IsPacketPayloadType(this TypeSyntax node, SemanticModel semanticModel)
    {
        var nodeStr = node.ToString();
        if (nodeStr != Constants.PacketBinaryType &&
            nodeStr != Constants.PacketHexType)
        {
            return false;
        }
            
        var symbolInfo = semanticModel.GetSymbolInfo(node);
        var symbol = symbolInfo.Symbol;
        if (symbol == null)
        {
            return false;
        }

        var typeNamespace = symbol.ContainingNamespace.ToString();
        if (typeNamespace != Constants.PacketNamespace)
        {
            return false;
        }

        return true;
    }
    
    public static string GetGlobalTypeName(this TypeSyntax node, SemanticModel semanticModel)
    {
        var semType = semanticModel.GetTypeInfo(node);
        var typeNamespace = semType.Type?.ContainingNamespace.ToString();
        var typeClass = semType.Type?.Name;
        var typeGlobal = $"global::{typeNamespace}.{typeClass}";

        return typeGlobal;
    }
}