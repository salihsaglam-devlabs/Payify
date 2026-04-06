namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

public class MerchantApiKeyDto
{
    public Guid MerchantId { get; set; }
    public string MerchantNumber { get; set; }
    public string PublicKey { get; set; }
    public string PrivateKeyEncrypted { get; set; }
}
