using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;

public class BankResponseCodeDto
{
    public Guid Id { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public int BankCode { get; set; }
    public string ResponseCode { get; set; }
    public string Description { get; set; }
    public string BankName { get; set; }
    public ResponseCodeAction Action { get; set; }
}
