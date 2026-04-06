using LinkPara.Billing.Domain.Enums;

namespace LinkPara.Billing.Application.Commons.Models.Billing;

public class BillStatusResponse
{
    public Guid InstitutionId { get; set; }
    public string BpcOid { get; set; }
    public string DebtOid { get; set; }
    public BillStatus BillStatus { get; set; }
}