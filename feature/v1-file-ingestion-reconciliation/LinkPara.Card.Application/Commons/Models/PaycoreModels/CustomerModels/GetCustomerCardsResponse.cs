namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;

public class GetCustomerCardsResponse
{
    public string bankingCustomerNo { get; set; }
    public string cardNo { get; set; }
    public string mainCardNo { get; set; }
    public string physicalType { get; set; }
    public string cardLevel { get; set; }
    public int branchCode { get; set; }
    public string productName { get; set; }
    public string productCode { get; set; }
    public string statCode { get; set; }
    public string statDescription { get; set; }
    public string statusReason { get; set; }
    public string embossName1 { get; set; }
    public string embossName2 { get; set; }
    public bool isNonameCard { get; set; }
    public string customerGroupType { get; set; }
    public string cardBrand { get; set; }
}
