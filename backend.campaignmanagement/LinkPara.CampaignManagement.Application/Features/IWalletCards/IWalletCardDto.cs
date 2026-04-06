namespace LinkPara.CampaignManagement.Application.Features.IWalletCards;

public class IWalletCardDto
{
    public Guid Id { get; set; }
    public int CardId { get; set; }
    public string CardNumber { get; set; }
    public bool IsExistingCustomer { get; set; }
}
