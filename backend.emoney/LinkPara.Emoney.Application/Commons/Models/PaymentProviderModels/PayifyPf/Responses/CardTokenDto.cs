namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

public class CardTokenDto : ResponseModel
{
    public string CardToken { get; set; }
    public Guid CardTopupRequestId { get; set; }
}
