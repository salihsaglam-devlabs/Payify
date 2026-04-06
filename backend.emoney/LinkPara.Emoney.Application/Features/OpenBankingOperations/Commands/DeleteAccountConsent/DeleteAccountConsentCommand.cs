using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.DeleteAccountConsent;
public class DeleteAccountConsentCommand : IRequest<YosServiceResultDto>
{
    public string ConsentId { get; set; }

}

public class DeleteAccountConsentCommandHandler : IRequestHandler<DeleteAccountConsentCommand, YosServiceResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public DeleteAccountConsentCommandHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<YosServiceResultDto> Handle(DeleteAccountConsentCommand request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.DeleteAccountConsentAsync(request);
    }
}
