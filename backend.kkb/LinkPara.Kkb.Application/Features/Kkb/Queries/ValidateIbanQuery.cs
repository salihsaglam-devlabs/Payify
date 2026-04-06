using LinkPara.HttpProviders.Vault;
using LinkPara.Kkb.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Kkb.Application.Features.Kkb.Queries;

public class ValidateIbanQuery : IRequest<ValidateIbanResponse>
{
    public string Iban { get; set; }
    public string IdentityNo { get; set; }
}

public class ValidateIbanQueryHandler : IRequestHandler<ValidateIbanQuery, ValidateIbanResponse>
{
    private readonly IKkbValidationService _kkbService;
    private readonly IVaultClient _vaultClient;

    public ValidateIbanQueryHandler(IKkbValidationService kkbService, 
        IVaultClient vaultClient)
    {
        _kkbService = kkbService;
        _vaultClient = vaultClient;
    }

    public async Task<ValidateIbanResponse> Handle(ValidateIbanQuery query, CancellationToken cancellationToken)
    {
        var isKkbEnabled = 
            _vaultClient.GetSecretValue<bool>("/SharedSecrets", "ServiceState", "KkbEnabled");

        if (!isKkbEnabled)
        {
            return await Task.FromResult(new ValidateIbanResponse { IsValid = false });
        }
        
        return await _kkbService.IbanValidationAsync(query.Iban, query.IdentityNo);
    }
}
