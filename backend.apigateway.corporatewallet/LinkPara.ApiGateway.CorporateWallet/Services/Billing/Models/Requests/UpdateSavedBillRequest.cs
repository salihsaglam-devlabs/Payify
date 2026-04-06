namespace LinkPara.ApiGateway.CorporateWallet.Services.Billing.Models.Requests;

public class UpdateSavedBillRequest
{
    public Guid Id { get; set; }
    public string BillName { get; set; }
}