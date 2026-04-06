namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

public class ReverseResponse : ResponseModel
{
    public string ProvisionNumber { get; set; }
    public SignatureDataResponse SignatureDataResponse { get; set; }
}
