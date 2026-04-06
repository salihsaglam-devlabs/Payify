using AutoMapper;
using LinkPara.Billing.Application.Commons.Exceptions;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Features.Billing;
using MediatR;

namespace LinkPara.Billing.Application.Features.InstitutionApis.Queries.GetBillInquiry;

public class InquireBillQuery : IRequest<BillInquiryResponseDto>
{
    public Guid InstitutionId { get; set; }
    public string SubscriberNumber1 { get; set; }
    public string SubscriberNumber2 { get; set; }
    public string SubscriberNumber3 { get; set; }
}

public class InquireBillQueryHandler : IRequestHandler<InquireBillQuery, BillInquiryResponseDto>
{
    private readonly IBillingService _billingService;
    private readonly IMapper _mapper;

    public InquireBillQueryHandler(IBillingService billingService, IMapper mapper)
    {
        _billingService = billingService;
        _mapper = mapper;
    }

    public async Task<BillInquiryResponseDto> Handle(InquireBillQuery request, CancellationToken cancellationToken)
    {
        var billInquiryResponse = await _billingService.InquireBillsAsync(request);

        if (billInquiryResponse.IsSuccess && !billInquiryResponse.Response.Bills.Any())
        {
            throw new NoBillException();
        }

        return _mapper.Map<BillInquiryResponseDto>(billInquiryResponse);
    }
}