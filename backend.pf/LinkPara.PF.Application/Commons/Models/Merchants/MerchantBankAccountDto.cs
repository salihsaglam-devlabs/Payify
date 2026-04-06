using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Commons.Models.Merchants;

public class MerchantBankAccountDto : IMapFrom<MerchantBankAccount>
{
    public Guid Id { get; set; }
    public string Iban { get; set; }
    public int BankCode { get; set; }
    public Guid MerchantId { get; set; }
    public string CreatedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
