using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;

public class CompanyIbanDto
{
    public DateTime CreateDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string CreatedBy { get; set; }
    public string LastModifiedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }

    public string Iban { get; set; }
    public int BankCode { get; set; }
    public string BankName { get; set; }
    public string Description { get; set; }
}
