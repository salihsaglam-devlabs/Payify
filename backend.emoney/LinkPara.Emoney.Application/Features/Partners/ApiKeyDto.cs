using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.Partners;

public class ApiKeyDto : IMapFrom<ApiKey>
{
    public string PartnerId { get; set; }
    public PartnerDto Partner { get; set; }
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }
}
