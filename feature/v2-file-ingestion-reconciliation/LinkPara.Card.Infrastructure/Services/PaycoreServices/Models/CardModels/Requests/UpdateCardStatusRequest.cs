namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Requests;

public class UpdateCardStatusRequest
{
    public string CardNo { get; set; }
    public string StatCode { get; set; }
    public string FreeText { get; set; }
    public string StatusReasonCode { get; set; }
}
