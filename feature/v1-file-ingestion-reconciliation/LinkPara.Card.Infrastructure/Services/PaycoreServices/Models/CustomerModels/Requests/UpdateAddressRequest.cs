using LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CustomerModels.Requests;
public class UpdateAddressRequest
{
    public string BankingCustomerNo { get; set; }
    public CustomerAddress[] CstCustomerAddresses { get; set; }
}
