using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class MerchantBankAccountDto
{
    public Guid Id { get; set; }
    public string Iban { get; set; }
    public int BankCode { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
