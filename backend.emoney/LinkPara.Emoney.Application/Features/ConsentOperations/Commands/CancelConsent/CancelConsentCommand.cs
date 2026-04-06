using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.ConsentOperations.Commands.CancelConsent;
public class CancelConsentCommand : IRequest<CancelConsentResultDto>
{
    public string ConsentId { get; set; }
    public string Username { get; set; }
    public ConsentType ConsentTypeValue { get; set; }
    public string RevokeCode { get; set; }
    public bool IsDecoupledAuth { get; set; }

}

public class CancelConsentCommandHandler : IRequestHandler<CancelConsentCommand, CancelConsentResultDto>
{
    private readonly IConsentOperationsService _consentOperationsService;

    public CancelConsentCommandHandler(
        IConsentOperationsService consentOperationsService)
    {
        _consentOperationsService = consentOperationsService;
    }

    public async Task<CancelConsentResultDto> Handle(CancelConsentCommand request,
        CancellationToken cancellationToken)
    {
        return await _consentOperationsService.CancelConsentAsync(request);
    }
}
