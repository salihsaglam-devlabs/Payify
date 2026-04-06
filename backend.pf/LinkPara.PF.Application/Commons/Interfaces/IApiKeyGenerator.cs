using LinkPara.PF.Application.Features.Merchants;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IApiKeyGenerator
{
    Task<MerchantApiKeyDto> Generate(Guid merchantId);
}
