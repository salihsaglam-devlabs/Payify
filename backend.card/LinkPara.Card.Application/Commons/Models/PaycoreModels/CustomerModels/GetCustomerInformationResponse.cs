namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;
public class GetCustomerInformationResponse
{
    public CustomerAddress[] cstCustomerAddresses { get; set; }
    public CustomerCommunication[] cstCustomerCommunications { get; set; }
    public CustomerLimit[] customerLimits { get; set; }
    public string primaryCardNo { get; set; }
    public string bankingCustomerNo { get; set; }
    public string name { get; set; }
    public string surname { get; set; }
    public int branchCode { get; set; }
}
