using LinkPara.PF.Pos.ApiGateway.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LinkPara.PF.Pos.ApiGateway.Commons.Filters;

public class ResponseModelStatusCodeFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult && 
            objectResult.Value is ResponseModel response && 
            !response.IsSucceed)
        {
            objectResult.StatusCode = StatusCodes.Status400BadRequest;
        }
        
        await next();
    }
}