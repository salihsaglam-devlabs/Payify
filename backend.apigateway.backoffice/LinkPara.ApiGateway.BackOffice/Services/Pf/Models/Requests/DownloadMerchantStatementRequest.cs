using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class DownloadMerchantStatementRequest
{
    public Guid Id { get; set; }
    public MerchantStatementType StatementType { get; set; }
}