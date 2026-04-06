using LinkPara.Accounting.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Accounting.Application.Features.Customers.Queries.GetById;

public class GetByIdQuery : IRequest<CustomerDto>
{
    public Guid Id { get; set; }
}

public class GetByIdQueryHandler : IRequestHandler<GetByIdQuery, CustomerDto>
{
    private readonly ICustomerService _customerService;

    public GetByIdQueryHandler(
        ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<CustomerDto> Handle(GetByIdQuery request, CancellationToken cancellationToken)
    {
        return await _customerService.GetByIdAsync(request.Id);
    }
}