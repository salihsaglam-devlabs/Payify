using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreatePaymentConsent;
public class CreatePaymentConsentCommand : IRequest<PaymentOrderConsentDetailDto>
{
    public string HhsCode { get; set; }
    public string AppUserId { get; set; }
    public YosForwardType YonTipi { get; set; }
    public string OhkTur { get; set; }
    public string DrmKod { get; set; }
    public ProcessAmountOb IslTtr { get; set; }
    public RetailPersonOb Alc { get; set; }
    public QrPaymentOb Kkod { get; set; }
    public RetailPersonOb Gon { get; set; }
    public PaymentConsentDetailOb OdmAyr { get; set; }
    public PaymentConsentCompanyDto IsyOdmBlg { get; set; }
    public PaymentConsentInformationDto OdmBsltm { get; set; }

}

public class CreatePaymentConsentCommandHandler : IRequestHandler<CreatePaymentConsentCommand, PaymentOrderConsentDetailDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public CreatePaymentConsentCommandHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<PaymentOrderConsentDetailDto> Handle(CreatePaymentConsentCommand request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.CreatePaymentConsentAsync(request);
    }
}
