using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Response;

public class GetCardLastCourierActivityResponse : PaycoreResponse
{
    public List<CardLastCourierActivity> CardLastCourierActivities { get; set; }
}