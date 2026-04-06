using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.CorporateWallet.Services.MoneyTransfer.Models.Responses
{
    public class SourceBankAccountDto
    {
        public Guid Id { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public string AccountName { get; set; }
        public string IBANNumber { get; set; }
        public virtual BankDto Bank { get; set; } = new BankDto();
        public bool DefaultEftBank { get; set; }
    }
}
