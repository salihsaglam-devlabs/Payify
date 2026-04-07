using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.ParameterModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.ParameterServices.Queries.GetProductsQuery;

public record GetProductsQuery : IRequest<GetProductsResponse>;
public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, GetProductsResponse>
{
    private readonly IPaycoreParameterService _paycoreService;
    public GetProductsQueryHandler(IPaycoreParameterService paycoreService)
    {
        _paycoreService = paycoreService;
    }
    public async Task<GetProductsResponse> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        return await _paycoreService.GetProductsAsync(request);
    }
}
