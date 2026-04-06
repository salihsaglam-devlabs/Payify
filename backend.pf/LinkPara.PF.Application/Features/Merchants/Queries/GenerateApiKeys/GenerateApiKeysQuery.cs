using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.Merchants.Queries.GenerateApiKeys;

public class GenerateApiKeysQuery : IRequest<MerchantApiKeyDto>
{
    public Guid MerchantId { get; set; }
}

public class GenerateApiKeysQueryHandler : IRequestHandler<GenerateApiKeysQuery, MerchantApiKeyDto>
{
    private readonly IApiKeyGenerator _apiKeyGenerator;
    private readonly IRestrictionService _restrictionService;

    public GenerateApiKeysQueryHandler(IApiKeyGenerator apiKeyGenerator, IRestrictionService restrictionService)
    {
        _apiKeyGenerator = apiKeyGenerator;
        _restrictionService = restrictionService;
    }
    public async Task<MerchantApiKeyDto> Handle(GenerateApiKeysQuery request, CancellationToken cancellationToken)
    {
        await _restrictionService.IsUserAuthorizedAsync(request.MerchantId);
        
        return await _apiKeyGenerator.Generate(request.MerchantId);
    }
}
