using AutoMapper;
using LinkPara.Billing.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Billing.Application.Features.Billing.Queries.GetBillStatus;

public class GetBillStatusQuery : IRequest<BillStatusResponseDto>
{
    public Guid TransactionId { get; set; }
}

public class GetBillStatusQueryHandler : IRequestHandler<GetBillStatusQuery, BillStatusResponseDto>
{
    private readonly IMapper _mapper;
    private readonly IBillingService _billingService;

    public GetBillStatusQueryHandler(IBillingService billingService, IMapper mapper)
    {
        _billingService = billingService;
        _mapper = mapper;
    }

    public async Task<BillStatusResponseDto> Handle(GetBillStatusQuery request, CancellationToken cancellationToken)
    {
        var billStatusResponse = await _billingService.InquireBillStatusAsync(request.TransactionId);

        return _mapper.Map<BillStatusResponseDto>(billStatusResponse);
    }
}