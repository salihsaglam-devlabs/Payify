namespace LinkPara.PF.Application.Commons.Models.Posting;

public class PostingUpdateBalanceItems
{
    public Guid PostingBankBalanceId { get; set; }
    public Guid? PostingBalanceId { get; set; }
    public List<Guid> PostingTransactionIds { get; set; }
}