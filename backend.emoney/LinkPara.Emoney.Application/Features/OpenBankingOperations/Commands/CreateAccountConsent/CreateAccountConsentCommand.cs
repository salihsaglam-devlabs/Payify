using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateAccountConsent;
public class CreateAccountConsentCommand : IRequest<AccountConsentDetailResultDto>
{
    public string HhsCode { get; set; }
    public int AppUserId { get; set; }
    public YosForwardType ForwardType { get; set; }
    public DateTime AccessExpireDate { get; set; }
    public List<string> PermissionTypes { get; set; }
    public string StatusCode { get; set; }
    public AccountConsentIdentityInfo Identity { get; set; }

}

public class CreateAccountConsentCommandHandler : IRequestHandler<CreateAccountConsentCommand, AccountConsentDetailResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public CreateAccountConsentCommandHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<AccountConsentDetailResultDto> Handle(CreateAccountConsentCommand request,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.CreateAccountConsentAsync(request);
    }
}
