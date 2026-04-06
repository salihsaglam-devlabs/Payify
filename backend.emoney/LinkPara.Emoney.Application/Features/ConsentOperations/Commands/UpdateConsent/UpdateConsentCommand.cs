using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.ConsentOperations.Commands.UpdateConsent;
public class UpdateConsentCommand : IRequest<UpdateConsentResultDto>
{
    public string ConsentId { get; set; }
    public string CustomerId { get; set; }
    public ConsentType ConsentTypeValue { get; set; }
    public string SelectedAccountResponse { get; set; }
    public List<UpdateConsentAccount> Accounts { get; set; }
    public string CustomerName { get; set; }
    public string IdentityType { get; set; }
    public string IdentityValue { get; set; }
    public string CorporateIdentityType { get; set; }
    public string CorporateIdentityValue { get; set; }

}

public class UpdateConsentCommandHandler : IRequestHandler<UpdateConsentCommand, UpdateConsentResultDto>
{
    private readonly IConsentOperationsService _consentOperationsService;

    public UpdateConsentCommandHandler(
      IConsentOperationsService consentOperationsService)
    {
        _consentOperationsService = consentOperationsService;
    }

    public async Task<UpdateConsentResultDto> Handle(UpdateConsentCommand request,
        CancellationToken cancellationToken)
    {
        return await _consentOperationsService.UpdateConsentAsync(request);
    }
}
