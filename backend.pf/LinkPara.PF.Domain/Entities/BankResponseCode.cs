using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class BankResponseCode : AuditEntity
{
    public int BankCode { get; set; }
    public Bank Bank { get; set; }
    public string ResponseCode { get; set; }
    public string Description { get; set; }
    public bool ProcessTimeoutManagement { get; set; }
    public Guid? MerchantResponseCodeId { get; set; }
    public MerchantResponseCode MerchantResponseCode { get; set; }
}