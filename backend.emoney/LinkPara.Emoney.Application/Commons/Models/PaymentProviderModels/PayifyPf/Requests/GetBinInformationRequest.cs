namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

public class GetBinInformationRequest : RequestHeaderInfo
{
    public string CardToken { get; set; }
    public string BinNumber { get; set; }
    public string LanguageCode = "TR";
}
