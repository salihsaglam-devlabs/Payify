namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class ConsentedAccountDto
{
    public string RizaNo { get; set; }
    public AccDetail HspTml { get; set; }
}

public class AccDetail
{
    public string HspRef { get; set; }
    public string HspNo { get; set; }
    public string KisaAd { get; set; }
    public string PrBrm { get; set; }
    public string HspTur { get; set; }
    public string HspTip { get; set; }
    public string HspUrunAdi { get; set; }
    public string HspDrm { get; set; }
    public string HspShb { get; set; }
    public string SubeAdi { get; set; }
    public AccountDetail HspDty { get; set; }
}

public class AccountDetail
{
    public DateTime HspAclsTrh { get; set; }
}