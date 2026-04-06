using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Accounting.Application.Features.Customers.Queries.GetFilterCustomer;

public class GetFilterCustomerQuery : SearchQueryParams, IRequest<PaginatedList<CustomerDto>>
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public bool? IsSuccess { get; set; }
}

public class GetFilterCustomerQueryHandler : IRequestHandler<GetFilterCustomerQuery, PaginatedList<CustomerDto>>
{

    private readonly ICustomerService _customerService;

    public GetFilterCustomerQueryHandler(
        ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<PaginatedList<CustomerDto>> Handle(GetFilterCustomerQuery request, CancellationToken cancellationToken)
    {
        return await _customerService.GetFilterCustomerAsync(request);
    }
}