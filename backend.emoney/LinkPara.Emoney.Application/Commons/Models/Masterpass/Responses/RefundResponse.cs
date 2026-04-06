namespace LinkPara.Emoney.Application.Commons.Models.Masterpass.Responses;

public class RefundResponse
{
    public string AcquirerRetrievalNumber { get; set; }
    public string AuthCode { get; set; }
    public string OrderId { get; set; }
    public string IcaNumber { get; set; }
    public string TerminalGroupId { get; set; }
    public string TerminalGroupName { get; set; }
    public string Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string AccountKey { get; set; }
    public string TransactionDate { get; set; }
    public string RetrievalReferenceNumber { get; set; }
    public string PaymentRetrievalReferenceNumber { get; set; }
    public string ResponseCode { get; set; }
    public string Description { get; set; }
}