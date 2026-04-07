using LinkPara.Card.Application.Commons.Models.PaycoreModels;
namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Responses;

public class PaycoreGetCardEncryptedCvv2AndExpireDateResponse: PaycoreResponse
{
    public string bankingCustomerNo { get; set; }
    public string customerNo { get; set; }
    public string cardNo { get; set; }
    public string expiryDateAndCvv2 { get; set; }
}