using LinkPara.Card.Application.Commons.Models.PaycoreModels;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Responses;

public class PaycoreCreateCardResponse : PaycoreResponse
{
    public string cardNo { get; set; }
    public VirtualCardResponse virtualCardResponse { get; set; }
}

public class VirtualCardResponse
{
    public string cvv2 { get; set; }
    public int expiryDate { get; set; }
    public bool isNewCardCreated { get; set; }
}
