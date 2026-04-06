using LinkPara.SharedModels.Persistence;

namespace LinkPara.HttpProviders.PF.Models.Response
{
    public class MerchantBankAccountDto
    {
        public Guid Id { get; set; }
        public string Iban { get; set; }
        public int BankCode { get; set; }
        public Guid MerchantId { get; set; }
        public string CreatedBy { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
