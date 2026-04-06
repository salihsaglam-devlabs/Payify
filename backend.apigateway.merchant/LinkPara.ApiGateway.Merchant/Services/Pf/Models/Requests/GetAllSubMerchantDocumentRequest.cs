using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class GetAllSubMerchantDocumentRequest : SearchQueryParams
{
    public Guid? DocumentId { get; set; }
    public string DocumentName { get; set; }
    public Guid? DocumentTypeId { get; set; }
    public Guid? SubMerchantId { get; set; }    
}