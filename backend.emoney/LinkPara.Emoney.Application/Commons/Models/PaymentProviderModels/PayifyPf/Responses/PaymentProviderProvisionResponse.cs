namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

public class PaymentProviderProvisionResponse : ResponseModel
{
    public string OrderId { get; set; }
    public string ProvisionNumber { get; set; }
    public Guid CardTopupRequestId { get; set; }
    public SignatureDataResponse SignatureDataResponse { get; set; }
}