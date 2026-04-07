using LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CustomerModels.Requests;

public class PaycoreCreateCustomerRequest
{
    public string BankingCustomerNo { get; set; }
    public string CustomerGroupCode { get; set; }
    public string BranchCode { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string CommunicationLanguage { get; set; }
    public DateTime ApplicationDate { get; set; }
    public CustomerAddress[] CstCustomerAddresses { get; set; }
    public CustomerCommunication[] CstCustomerCommunications { get; set; }
}
