using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class LinkInfoSearchRequest
{
    public string LinkCode { get; set; }
    public ChannelStatus? LinkStatus { get; set; }
    public LinkAmountType? LinkAmountType { get; set; }
    public ChannelPaymentStatus? LinkPaymentStatus { get; set; }
    public LinkType? LinkType { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public DateTime? ExpiryDateStart { get; set; }
    public DateTime? ExpiryDateEnd { get; set; }
    public string? ProductName { get; set; }
    public string? ProductDescription { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

}

