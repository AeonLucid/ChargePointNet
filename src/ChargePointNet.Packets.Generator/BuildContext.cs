using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ChargePointNet.Packets.Generator;

public class BuildContext
{
    public BuildContext(
        SourceProductionContext context, 
        SemanticModel semanticModel,
        ClassDeclarationSyntax syntax)
    {
        Context = context;
        SemanticModel = semanticModel;
        Syntax = syntax;
    }
    
    public SourceProductionContext Context { get; }
    public SemanticModel SemanticModel { get; }
    public ClassDeclarationSyntax Syntax { get; }
}