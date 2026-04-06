namespace LinkPara.Billing.Application.Commons.Models.Billing;

public class BillInvoice : Bill
{
    public string ReferenceId { get; set; }
    public string VoucherNumber { get; set; }
    public bool IsSuccess { get; set; }
    public string Description { get; set; }
}