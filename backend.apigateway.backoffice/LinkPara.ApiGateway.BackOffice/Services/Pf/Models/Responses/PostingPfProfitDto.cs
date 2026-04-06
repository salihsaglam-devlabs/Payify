namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class PostingPfProfitDto
{
    public Guid Id { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal AmountWithoutBankCommission { get; set; }
    public decimal TotalPayingAmount { get; set; }
    public decimal TotalPfNetCommissionAmount { get; set; }
    public decimal ProtectionTransferAmount { get; set; }
    public int Currency { get; set; }
    public List<PostingPfProfitDetailDto> PostingPfProfitDetails { get; set; }
}