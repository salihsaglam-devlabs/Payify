using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.Response;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Responses;

public class PaycoreCardLastCourierActivityResponse : PaycoreBaseResponse
{
    public PaycoreCardLastCourierActivity[] Result { get; set; }
}

public class PaycoreCardLastCourierActivity
{
    public string courierStatusCode { get; set; }
    public string courierActivityCode { get; set; }
    public string courierStatusDescription { get; set; }
    public DateTime courierStatChangeDate { get; set; }
    public string courierStatChangeTime { get; set; }
    public string courierAddress { get; set; }
    public string courierCity { get; set; }
    public string cardDeliveryRecipientName { get; set; }
    public string brand { get; set; }
    public int courierCompanyCode { get; set; }
    public string courierCompanyName { get; set; }
    public string batchBarcodeNo { get; set; }
    public string bankingCustomerNo { get; set; }
    public string customerNo { get; set; }
    public string cardNo { get; set; }
    public string prevCardNo { get; set; }
    public string mainCardNo { get; set; }
    public string physicalType { get; set; }
    public string cardLevel { get; set; }
    public int branchCode { get; set; }
    public string dci { get; set; }
    public string segment { get; set; }
    public string productName { get; set; }
    public string productCode { get; set; }
    public string logoCode { get; set; }
    public string statCode { get; set; }
    public string statDescription { get; set; }
    public string statusReason { get; set; }
    public string embossCode { get; set; }
    public DateTime embossDate { get; set; }
    public string embossName1 { get; set; }
    public string embossName2 { get; set; }
    public string applicationRefNo { get; set; }
    public string barcodeNo { get; set; }
    public string bankAccountNo { get; set; }
    public bool isNonameCard { get; set; }
    public bool isOpen { get; set; }
    public string customerGroupType { get; set; }
    public string cardBrand { get; set; }
    public string companyName { get; set; }
    public int digitalSlipType { get; set; }
}

