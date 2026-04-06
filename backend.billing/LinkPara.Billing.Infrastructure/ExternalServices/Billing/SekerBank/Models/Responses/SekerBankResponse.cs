namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Responses;

public class SekerBankResponse<T>
{
    public T Data { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorDescription { get; set; }
    public SekerBankErrorResponse ErrorResponse { get; set; }
}