using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Features.Merchants;

public class MerchantApiKeyPatch : IMapFrom<MerchantApiKey>
{
    public Guid Id { get; set; }
    public Guid MerchantId { get; set; }
    public string MerchantNumber { get; set; }
    public string PublicKey { get; set; }
    public string PrivateKeyEncrypted { get; set; }
}
