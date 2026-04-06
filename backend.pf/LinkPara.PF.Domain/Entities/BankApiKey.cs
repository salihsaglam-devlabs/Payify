using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class BankApiKey : AuditEntity
{
    public Guid AcquireBankId { get; set; }
    public AcquireBank AcquireBank { get; set; }
    public string Key { get; set; }
    public string MappingName { get; set; }
    public bool IsPfMainMerchantId { get; set; }
    public BankApiKeyCategory Category { get; set; }
}