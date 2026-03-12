using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ChargePointNet.Services.OpenApi;

public class OpenApiDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info.Description = "Bridge project to serve EV chargers with a simple REST API. " +
                                    "https://github.com/AeonLucid/ChargePointNet";
            
        return Task.CompletedTask;
    }
}