namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class PostingBillDto
{
    public Guid MerchantId { get; set; }
    public string MerchantName { get; set; }
    public int BillMonth { get; set; }
    public DateTime BillDate { get; set; }
    public decimal BillAmount { get; set; }
    public int CurrencyCode { get; set; }
    public string CurrencyIsoCode { get; set; }
    public string CurrencyName { get; set; }
    public bool HasBsmv { get; set; }
}