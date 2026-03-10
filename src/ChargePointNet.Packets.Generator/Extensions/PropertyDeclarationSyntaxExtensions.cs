using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ChargePointNet.Packets.Generator.Extensions;

internal static class PropertyDeclarationSyntaxExtensions
{
    public static string GetName(this PropertyDeclarationSyntax property)
    {
        return property.Identifier.Text;
    }

    public static string GetTypeName(this TypeSyntax typeSyntax)
    {
        if (typeSyntax is NullableTypeSyntax nullableType)
        {
            return nullableType.ElementType.ToString();
        }
            
        return typeSyntax.ToString();
    }

    public static PacketFieldAttribute GetAttribute(this PropertyDeclarationSyntax property, SemanticModel semanticModel)
    {
        if (semanticModel.GetDeclaredSymbol(property) is not IPropertySymbol propertySymbol)
        {
            throw new Exception("Property symbol not found");
        }
            
        var result = new PacketFieldAttribute();
            
        foreach (var attributeData in propertySymbol.GetAttributes())
        {
            if (!nameof(PacketFieldAttribute).Equals(attributeData.AttributeClass?.Name))
            {
                continue;
            }
                
            foreach (var argument in attributeData.NamedArguments)
            {
                if (argument.Value.IsNull)
                {
                    continue;
                }
                
                switch (argument.Key)
                {
                    case nameof(PacketFieldAttribute.LengthSize):
                        result.LengthSize = (int) argument.Value.Value!;
                        break;
                    case nameof(PacketFieldAttribute.MaxSize):
                        result.MaxSize = (int) argument.Value.Value!;
                        break;
                    case nameof(PacketFieldAttribute.FixedSize):
                        result.FixedSize = (int) argument.Value.Value!;
                        break;
                    case nameof(PacketFieldAttribute.BigEndian):
                        result.BigEndian = (bool) argument.Value.Value!;
                        break;
                    case nameof(PacketFieldAttribute.Raw):
                        result.Raw = (bool) argument.Value.Value!;
                        break;
                    case nameof(PacketFieldAttribute.Encoding):
                        result.Encoding = (StringEncoding) argument.Value.Value!;
                        break;
                }
            }
        }

        return result;
    }
}