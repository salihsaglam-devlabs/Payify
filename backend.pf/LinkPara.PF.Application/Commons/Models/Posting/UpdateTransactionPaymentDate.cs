namespace LinkPara.PF.Application.Commons.Models.Posting;

public class UpdateTransactionPaymentDate
{
    public List<Guid> PostingTransactionIds { get; set; }
    public List<Guid> MerchantTransactionIds { get; set; }
    public DateTime PaymentDate { get; set; }
}