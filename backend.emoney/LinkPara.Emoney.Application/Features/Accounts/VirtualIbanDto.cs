using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.Accounts;

public class VirtualIbanDto : IMapFrom<VirtualIban>
{
    public string Iban { get; set; }
    public int BankCode { get; set; }
    public bool Available { get; set; }
}
