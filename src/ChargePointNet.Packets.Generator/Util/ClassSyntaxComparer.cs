using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ChargePointNet.Packets.Generator.Util;

public class ClassSyntaxComparer : IEqualityComparer<(ClassDeclarationSyntax, Compilation)>
{
    public static readonly ClassSyntaxComparer Instance = new ClassSyntaxComparer();

    public bool Equals((ClassDeclarationSyntax, Compilation) x, (ClassDeclarationSyntax, Compilation) y)
    {
        return x.Item1.Equals(y.Item1);
    }

    public int GetHashCode((ClassDeclarationSyntax, Compilation) obj)
    {
        return obj.Item1.GetHashCode();
    }
}