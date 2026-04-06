using LinkPara.HttpProviders.Vault;
using LinkPara.Kkb.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Kkb.Application.Features.Kkb.Queries;

public class InquireIbanQuery : IRequest<InquireIbanResponse>
{
    public string Iban { get; set; }
}

public class InquireIbanQueryHandler : IRequestHandler<InquireIbanQuery, InquireIbanResponse>
{
    private readonly IKkbValidationService _kkbService;
    private readonly IVaultClient _vaultClient;

    public InquireIbanQueryHandler(IKkbValidationService kkbService, 
        IVaultClient vaultClient)
    {
        _kkbService = kkbService;
        _vaultClient = vaultClient;
    }

    public async Task<InquireIbanResponse> Handle(InquireIbanQuery query, CancellationToken cancellationToken)
    {
        var isKkbEnabled = 
            _vaultClient.GetSecretValue<bool>("/SharedSecrets", "ServiceState", "KkbEnabled");

        if (!isKkbEnabled)
        {
            return await Task.FromResult(new InquireIbanResponse ());
        }
        
        return await _kkbService.IbanInquireAsync(query.Iban);
    }
}
