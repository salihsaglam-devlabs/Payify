using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class PostingTransferErrorDto
{
    public DateTime PostingDate { get; set; }
    public Guid? MerchantId { get; set; }
    public string MerchantName { get; set; }
    public Guid MerchantTransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public int Currency { get; set; }
    public string ErrorMessage { get; set; }
    public PostingTransferErrorCategory TransferErrorCategory { get; set; }
    public string MerchantNumber { get; set; }
    public string MerchantOrderId { get; set; }
}