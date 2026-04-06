namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class PostingPfProfitDetailDto
{
    public Guid Id { get; set; }
    public int AcquireBankCode { get; set; }
    public string BankName { get; set; }
    public decimal BankPayingAmount { get; set; }
    public int Currency { get; set; }
    public Guid PostingPfProfitId { get; set; }
}