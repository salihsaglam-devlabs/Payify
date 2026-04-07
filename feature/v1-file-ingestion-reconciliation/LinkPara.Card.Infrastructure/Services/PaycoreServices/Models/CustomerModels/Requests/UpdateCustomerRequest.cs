using LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CustomerModels.Requests;
public class UpdateCustomerRequest
{
    public string BankingCustomerNo { get; set; }
    public string CustomerGroupCode { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
}