
namespace LinkPara.CampaignManagement.Application.Commons.Models.Responses;

public class CardResponse : BaseResponse
{
    public Card Data { get; set; }
}

public class Card
{
    public int id { get; set; }
    public string name_surname { get; set; }
    public string cc_number { get; set; }
    public string email { get; set; }
    public string gsm { get; set; }
    public int customer_branch_id { get; set; }
    public int customer_id { get; set; }
    public string tc_no { get; set; }
    public string ext_wallet_id { get; set; }
    public int wallet_id { get; set; }
}
