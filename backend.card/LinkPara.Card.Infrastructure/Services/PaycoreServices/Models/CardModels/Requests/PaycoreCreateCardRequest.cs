using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Requests;

public class PaycoreCreateCardRequest
{
    public PaycoreCardModel CrdCard { get; set; }
}

public class PaycoreCardModel
{
    public CrdCardAccount CrdCardAccount { get; set; }
    public CrdCardAuth CrdCardAuth { get; set; }
    public string BankingCustomerNo { get; set; }
    public string CardLevel { get; set; }
    public string EmbossName1 { get; set; }
    public string ProductCode { get; set; }
    public int BranchCode { get; set; }
    public bool NoMoreRenewal { get; set; }
    public string EmbossMethod { get; set; }
    public int? PersoCenterCode { get; set; }
    public int? CourierCompanyCode { get; set; }
    public CrdCardDelivery? CrdCardDelivery { get; set; }
}
