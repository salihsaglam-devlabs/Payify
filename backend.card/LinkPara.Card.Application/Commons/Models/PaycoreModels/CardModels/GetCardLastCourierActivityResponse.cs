using static MassTransit.Logging.OperationName;

namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;

public class GetCardLastCourierActivityResponse :PaycoreResponse
{
    public List<CardLastCourierActivity> CardLastCourierActivities { get; set; }
}
public class CardLastCourierActivity
{
    public string CourierActivityCode { get; set; } //Kurye aktivite tanımları (D: Teslim Edildi, R: İade, T: Taşıma)
    public string CourierStatusDescription { get; set; }
    public DateTime CourierStatChangeDate { get; set; }
    public string CourierStatChangeTime { get; set; }
    public string CourierAddress { get; set; }
    public string CourierCity { get; set; }
    public string CardDeliveryRecipientName { get; set; }
    public string Brand { get; set; }
    public int CourierCompanyCode { get; set; }
    public string CourierCompanyName { get; set; }
    public string CardNo { get; set; }
    public int BranchCode { get; set; }
    public DateTime EmbossDate { get; set; }
    public string EmbossName1 { get; set; }
    public string CompanyName { get; set; }
}

