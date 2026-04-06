using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LinkPara.ApiGateway.Authorization.CustomPolicies.OtpPolicy;

public class OtpRequirementOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();
     
        context.ApiDescription.TryGetMethodInfo(out var methodInfo);
        
        if (methodInfo?.DeclaringType == null) 
            return;
        
        var customAttributes = methodInfo.CustomAttributes;
        foreach (var attribute in customAttributes)
        {
            if (attribute.NamedArguments is not null)
            {
                foreach (var argument in attribute.NamedArguments)
                {
                    if (argument.TypedValue.Value != null && argument.TypedValue.Value.ToString() == "RequireOtp")
                    {
                        operation.Parameters.Add(new OpenApiParameter
                        {
                            Name = "otp-timestamp",  
                            In = ParameterLocation.Header,  
                            Description = "The timestamp of requested otp",  
                            Required = true  
                        });
                        operation.Parameters.Add(new OpenApiParameter
                        {
                            Name = "otp-authorization-id",  
                            In = ParameterLocation.Header,  
                            Description = "authorization id of otp",  
                            Required = true  
                        });
                        operation.Parameters.Add(new OpenApiParameter
                        {
                            Name = "otp-code",  
                            In = ParameterLocation.Header,  
                            Description = "otp code",  
                            Required = true  
                        });
                    }
                }
            }
        }
    }
}