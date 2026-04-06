namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class RunReconciliationRequest
{
    public int BankCode { get; set; }
    public DateTime QueryDate { get; set; }
}
