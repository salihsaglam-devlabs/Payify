using LinkPara.Billing.Application.Commons.Models.Billing;
using LinkPara.Billing.Domain.Enums;

namespace LinkPara.Billing.Application.Features.Billing;

public class BillPreviewResponseDto
{
    public string RequestId { get; set; }
    public Guid InstitutionId { get; set; }
    public Bill Bill { get; set; }
    public string WalletNumber { get; set; }
    public string PayeeFullName { get; set; }
    public string PayeeMobile { get; set; }
    public string PayeeEmail { get; set; }
    public PaymentSource PaymentSource { get; set; }
}
