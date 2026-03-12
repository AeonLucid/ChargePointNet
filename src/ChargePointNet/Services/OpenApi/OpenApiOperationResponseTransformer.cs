using ChargePointNet.Models;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ChargePointNet.Services.OpenApi;

public class OpenApiOperationResponseTransformer : IOpenApiOperationTransformer
{
    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation.Responses == null)
        {
            return;
        }
        
        if (operation.Responses.ContainsKey("404"))
        {
            operation.Responses["404"] = new OpenApiResponse
            {
                Description = "Not Found", 
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new()
                    {
                        Schema = await context.GetOrCreateSchemaAsync(typeof(ErrorResponse), cancellationToken: cancellationToken)
                    }
                }
            };
        }
    }
}