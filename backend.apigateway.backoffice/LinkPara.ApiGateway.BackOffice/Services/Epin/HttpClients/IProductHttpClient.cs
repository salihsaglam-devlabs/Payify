using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;

public interface IProductHttpClient
{
    Task<ActionResult<List<ProductDto>>> GetFilterProductsAsync(GetFilterProductsRequest request);
}
