using LinkPara.Card.Application.Commons.Models.PaycoreModels.ParameterModels;
using LinkPara.Card.Application.Features.PaycoreServices.ParameterServices.Queries.GetProductsQuery;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IPaycoreParameterService
{
    Task<GetProductsResponse> GetProductsAsync(GetProductsQuery query);
}
