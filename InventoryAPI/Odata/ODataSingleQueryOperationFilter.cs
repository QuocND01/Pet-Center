using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace InventoryAPI.Odata
{
    public class ODataSingleQueryOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var path = context.ApiDescription.RelativePath;

            if (path == null || !path.StartsWith("odata", StringComparison.OrdinalIgnoreCase))
                return;

            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "queryOptions",
                In = ParameterLocation.Query,
                Description = "OData query (ex: $filter=QuantityAvailable gt 10&$orderby=LastUpdated desc&$top=5)",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }
    }
}
