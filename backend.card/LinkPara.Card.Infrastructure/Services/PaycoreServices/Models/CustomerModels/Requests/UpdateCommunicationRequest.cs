using LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CustomerModels.Requests;
public class UpdateCommunicationRequest
{
    public CustomerCommunication[] CstCustomerCommunications { get; set; }
    public string BankingCustomerNo { get; set; }
}
