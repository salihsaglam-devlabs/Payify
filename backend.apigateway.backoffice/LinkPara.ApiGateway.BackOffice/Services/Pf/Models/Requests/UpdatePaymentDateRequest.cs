namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class UpdatePaymentDateRequest
{
    public Guid MerchantBlockageId { get; set; }
    public Guid PostBalanceId { get; set; }
    public DateTime PaymentDate { get; set; }
}
