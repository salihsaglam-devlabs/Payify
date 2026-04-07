namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;

public class GetCardAuthorizationsResponse
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public bool EcommercePermission { get; set; }
    public bool Non3DPermission { get; set; }
    public bool CashTransactionPermission { get; set; }
    public bool InternationalPermission { get; set; }
    public string ThreeDSecureType { get; set; }
}
