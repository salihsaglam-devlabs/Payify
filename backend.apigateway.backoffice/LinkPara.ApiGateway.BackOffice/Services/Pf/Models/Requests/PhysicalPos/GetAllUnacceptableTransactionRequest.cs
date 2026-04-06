using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums.PhysicalPos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;

public class GetAllUnacceptableTransactionRequest : SearchQueryParams
{
    public string PaymentId { get; set; }
    public string BatchId { get; set; }
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public string Currency { get; set; }
    public string MerchantId { get; set; }
    public string TerminalId { get; set; }
    public string BinNumber { get; set; }
    public string ProvisionNo { get; set; }
    public string Vendor { get; set; }
    public string Rrn { get; set; }
    public string Stan { get; set; }
    public string BankRef { get; set; }
    public string OriginalRef { get; set; }
    public Guid? PfMerchantId { get; set; }
    public string ConversationId { get; set; }
    public string SerialNumber { get; set; }
    public UnacceptableTransactionStatus? CurrentStatus {get; set;}
    public Guid? PhysicalPosEodId { get; set; }
    public EndOfDayStatus? EndOfDayStatus { get; set; }
}