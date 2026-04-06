namespace LinkPara.HttpProviders.PF.Models.Request;

public class UpdateMerchantTransactionRequest
{
    public bool IsChargeback { get; set; }
    public bool IsSuspecious { get; set; }
    public string SuspeciousDescription { get; set; }
}