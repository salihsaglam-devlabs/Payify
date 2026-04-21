using LinkPara.Card.Application.Commons.Models.PaycoreModels;
namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Responses;

public class PaycoreGetCardEmbossStatusResponse
{
    public int embossStat { get; set; }
    public string description { get; set; }
    public DateTimeOffset embossRequestDate { get; set; }
}
