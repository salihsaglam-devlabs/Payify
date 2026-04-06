using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests;

public class UpdateBankResponseCodeRequest
{
    public Guid Id { get; set; }
    public int BankCode { get; set; }
    public string ResponseCode { get; set; }
    public string Description { get; set; }
    public ResponseCodeAction Action { get; set; }
}
