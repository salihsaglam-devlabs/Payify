using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.PF.Domain.Entities;

public class MerchantBankAccount : AuditEntity, ITrackChange
{
    public string Iban { get; set; }
    public int BankCode { get; set; }
    public Bank Bank { get; set; }
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
}