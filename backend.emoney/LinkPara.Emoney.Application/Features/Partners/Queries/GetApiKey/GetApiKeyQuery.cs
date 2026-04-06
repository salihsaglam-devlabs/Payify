using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Partners.Queries.GetApiKey;

public class GetApiKeyQuery : IRequest<ApiKeyDto>
{
    public string PublicKeyEncoded { get; set; }
}

public class GetApiKeyQueryHandler : IRequestHandler<GetApiKeyQuery, ApiKeyDto>
{
    private readonly IApiKeyService _apiKeyService;

    public GetApiKeyQueryHandler(IApiKeyService apiKeyService)
    {
        _apiKeyService = apiKeyService;
    }

    public async Task<ApiKeyDto> Handle(GetApiKeyQuery request, CancellationToken cancellationToken)
    {
        var encodedBytes = Convert.FromBase64String(request.PublicKeyEncoded);
        var publicKey = System.Text.Encoding.UTF8.GetString(encodedBytes);
        return await _apiKeyService.GetApiKey(publicKey);
    }
}
