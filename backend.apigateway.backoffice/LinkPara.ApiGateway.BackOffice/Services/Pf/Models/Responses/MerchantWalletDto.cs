using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class MerchantWalletDto
{
    public Guid Id { get; set; }
    public string WalletNumber { get; set; }
    public Guid MerchantId { get; set; }
    public string CreatedBy { get; set; }
    public RecordStatus RecordStatus { get; set; }
}