using LinkPara.ApiGateway.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetConsentedAccountActivitiesRequest
{
    public string AppUserId { get; set; }
    public string HhsCode { get; set; }
    public string AccountReference { get; set; }
    public DateTime HesapIslemBslTrh { get; set; }
    public DateTime HesapIslemBtsTrh { get; set; }
    public string MinIslTtr { get; set; }
    public string MksIslTtr { get; set; }
    public string BrcAlc { get; set; }
    public string SyfNo { get; set; }
    public string SrlmKrtr { get; set; }
    public string SrlmYon { get; set; }
    public string SyfKytSayi { get; set; }
}

