using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.AccountServiceProviders;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateStandingPaymentOrderConsent;
public class CreateStandingPaymentOrderConsentCommand : IRequest<StandingPaymentOrderConsentResultDto>
{
    public string ConsentId { get; set; }
    public string AppUserId { get; set; }
    public string HhsCode { get; set; }
    public string YonTipi { get; set; }
    public PaymentGkdDto Gkd { get; set; }
    public PaymentInformationDto OdmBsltm { get; set; }
    public PaymentDetail OdmAyr { get; set; }
    
}


public class CreateStandingPaymentOrderConsentCommandHandler : IRequestHandler<CreateStandingPaymentOrderConsentCommand, StandingPaymentOrderConsentResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public CreateStandingPaymentOrderConsentCommandHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<StandingPaymentOrderConsentResultDto> Handle(CreateStandingPaymentOrderConsentCommand command,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.CreateStandingPaymentOrderConsentAsync(command);
    }
}
