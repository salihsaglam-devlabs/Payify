using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LinkPara.ApiGateway.Boa.Filters.CustomerContext;

public class CustomerHeaderAttributeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAttr = context.MethodInfo.GetCustomAttributes(true)
                       .OfType<CustomerContextRequiredAttribute>()
                       .Any();

        if (!hasAttr)
            return;

        operation.Parameters ??= [];

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-CustomerId",
            In = ParameterLocation.Header,
            Required = true,
            Schema = new OpenApiSchema { Type = "string" }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-PersonId",
            In = ParameterLocation.Header,
            Required = true,
            Schema = new OpenApiSchema { Type = "string" }
        });
    }
}
