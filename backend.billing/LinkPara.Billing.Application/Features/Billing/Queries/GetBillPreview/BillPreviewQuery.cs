using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Commons.Models.Billing;
using LinkPara.Billing.Domain.Enums;
using MediatR;

namespace LinkPara.Billing.Application.Features.Billing.Queries.GetBillPreview;

public class BillPreviewQuery : IRequest<BillPreviewResponseDto>
{
    public Guid InstitutionId { get; set; }
    public string WalletNumber { get; set; }
    public string PayeeFullName { get; set; }
    public string PayeeMobile { get; set; }
    public string PayeeEmail { get; set; }
    public PaymentSource PaymentSource { get; set; }
    public string RequestId { get; set; }
    public Bill Bill { get; set; }
}
public class BillPreviewQueryHandler : IRequestHandler<BillPreviewQuery, BillPreviewResponseDto>
{
    private readonly ICommissionService _commissionService;

    public BillPreviewQueryHandler(ICommissionService commissionService) 
        => _commissionService = commissionService;

    public async Task<BillPreviewResponseDto> Handle(BillPreviewQuery request, CancellationToken cancellationToken)
    {
        var commissionWithAmountDetail = await _commissionService.CalculateCommissionWithAmountDetailAsync(request.InstitutionId, request.Bill.Amount, request.PaymentSource);

        request.Bill.BsmvAmount = commissionWithAmountDetail.BsmvAmount;
        request.Bill.CommissionAmount = commissionWithAmountDetail.CommissionAmount;

        return new BillPreviewResponseDto
        {
            RequestId = request.RequestId,
            InstitutionId = request.InstitutionId,
            Bill = request.Bill,
            WalletNumber = request.WalletNumber,
            PayeeFullName = request.PayeeFullName,
            PayeeMobile = request.PayeeMobile,
            PayeeEmail = request.PayeeEmail,
            PaymentSource = request.PaymentSource
        };
    }
}
