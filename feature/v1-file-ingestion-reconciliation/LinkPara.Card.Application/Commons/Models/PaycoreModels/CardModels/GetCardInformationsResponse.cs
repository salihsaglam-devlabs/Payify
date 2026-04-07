namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;

public class GetCardInformationsResponse
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public string Cvv2 { get; set; }
    public string CardNumber { get; set; }
    public int ExpireDate { get; set; }
}
