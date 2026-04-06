namespace LinkPara.CampaignManagement.Application.Commons.Models.Requests;

public class CardApplicationRequest
{
    public CardRequest card { get; set; }
}

public class CardRequest
{
    public string name_surname { get; set; }
    public string ext_wallet_id { get; set; }
    public string wallet_id { get; set; }
    public string tc_no { get; set; }
    public string email { get; set; }
    public string gsm { get; set; }
    public string address { get; set; }
    public int city_id { get; set; }
    public int town_id { get; set; }
    public string utype { get; set; }
    public string code { get; set; }
    public string branch_name { get; set; }
    public bool individual_framework_agreement { get; set; }
    public bool preliminary_information_agreement { get; set; }
    public bool commercial_electronic_communication_aggrement { get; set; }
    public bool kvkk_agreement { get; set; }
}