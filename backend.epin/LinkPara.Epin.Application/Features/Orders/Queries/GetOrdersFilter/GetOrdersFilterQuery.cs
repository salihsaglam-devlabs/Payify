using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.Epin.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Epin.Application.Features.Orders.Queries.GetOrdersFilter;

public class GetOrdersFilterQuery : SearchQueryParams, IRequest<PaginatedList<OrderDto>>
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public string Email { get; set; }
    public OrderStatus? OrderStatus { get; set; }
    public Guid? PublisherId { get; set; }
    public Guid? BrandId { get; set; }
    public int? ProductId { get; set; }
    public ReconciliationStatus? ReconciliationStatus { get; set; }

}

public class GetOrdersFilterQueryHandler : IRequestHandler<GetOrdersFilterQuery, PaginatedList<OrderDto>>
{
    private readonly IOrderService _orderService;

    public GetOrdersFilterQueryHandler(IOrderService orderService)
    {
        _orderService = orderService;
    }
    public async Task<PaginatedList<OrderDto>> Handle(GetOrdersFilterQuery request, CancellationToken cancellationToken)
    {
        return await _orderService.GetOrdersFilterAsync(request);
    }
}