using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Epin.Application.Features.Orders.Queries.GetUserOrdersFilter;

public class GetUserOrdersFilterQuery : SearchQueryParams, IRequest<PaginatedList<UserOrderDto>>
{
    public Guid UserId { get; set; }
}

public class GetFilterUserOrdersQueryHandler : IRequestHandler<GetUserOrdersFilterQuery, PaginatedList<UserOrderDto>>
{
    private readonly IOrderService _orderService;

    public GetFilterUserOrdersQueryHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<PaginatedList<UserOrderDto>> Handle(GetUserOrdersFilterQuery request, CancellationToken cancellationToken)
    {
        return await _orderService.GetUserOrdersFilterAsync(request);
    }
}