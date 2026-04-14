namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;

public class GetCardStatusResponse : PaycoreResponse
{
    public CrdCardStatus[] Result { get; set; }
}

