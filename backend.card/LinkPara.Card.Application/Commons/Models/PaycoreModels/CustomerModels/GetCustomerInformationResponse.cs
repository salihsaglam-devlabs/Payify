using System.Text.Json.Serialization;

namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;
public class GetCustomerInformationResponse
{
    [JsonPropertyName("cstCustomerAddresses")]
    public CustomerAddress[] CstCustomerAddresses { get; set; }
    [JsonPropertyName("cstCustomerCommunications")]
    public CustomerCommunication[] CstCustomerCommunications { get; set; }
    [JsonPropertyName("customerLimits")]
    public CustomerLimit[] CustomerLimits { get; set; }
    [JsonPropertyName("primaryCardNo")]
    public string PrimaryCardNo { get; set; }
    [JsonPropertyName("bankingCustomerNo")]
    public string BankingCustomerNo { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("surname")]
    public string Surname { get; set; }
    [JsonPropertyName("branchCode")]
    public int BranchCode { get; set; }
}
