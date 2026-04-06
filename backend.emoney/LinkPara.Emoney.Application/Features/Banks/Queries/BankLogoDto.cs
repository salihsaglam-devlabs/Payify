using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.Banks.Queries;

public class BankLogoDto : IMapFrom<BankLogo>
{
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public Guid BankId { get; set; }
}