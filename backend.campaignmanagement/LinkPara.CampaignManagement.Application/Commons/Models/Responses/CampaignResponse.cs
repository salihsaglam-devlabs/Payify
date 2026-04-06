

namespace LinkPara.CampaignManagement.Application.Commons.Models.Responses;

public class CampaignResponse
{
    public string code { get; set; }
    public List<Campaign> Data { get; set; }
}

public class Campaign
{
    public int id { get; set; }
    public string campaign_type { get; set; }
    public decimal min_amount { get; set; }
    public DateTime start_date { get; set; }
    public DateTime end_date { get; set; }
    public string cb_type { get; set; }
    public string title { get; set; }
    public string body { get; set; }
    public string image_url { get; set; }
    public List<CampaignMerchant> campaign_merchants { get; set; }
}

public class CampaignMerchant
{
    public decimal cb_amount { get; set; }
    public string content { get; set; }
    public string image_url { get; set; }
    public Merchant merchant { get; set; }
}

public class Merchant
{
    public int id { get; set; }
    public string name { get; set; }
    public string _type { get; set; }
    public List<string> sector_arr { get; set; }
    public string logo { get; set; }
    public List<Sector> sector_array { get; set; }
}

public class Sector
{
    public int id { get; set; }
    public string letter { get; set; }
    public string name { get; set; }
}

