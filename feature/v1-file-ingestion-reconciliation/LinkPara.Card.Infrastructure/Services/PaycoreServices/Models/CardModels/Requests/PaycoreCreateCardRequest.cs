using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using Newtonsoft.Json;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Requests;

[JsonObject("CrdCard")]
public class PaycoreCreateCardRequest
{
    public CardAccount CrdCardAccount { get; set; }
    public string BankingCustomerNo { get; set; }
    public string CardLevel { get; set; }
    public string EmbossName1 { get; set; }
    public string EmbossName2 { get; set; }
    public string ProductCode { get; set; }
    public int BranchCode { get; set; }
    public string EmbossMethod { get; set; }
    public CardDelivery CrdCardDelivery { get; set; }
}
