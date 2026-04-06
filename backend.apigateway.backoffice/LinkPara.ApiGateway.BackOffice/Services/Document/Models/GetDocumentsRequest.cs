using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Document.Models
{
    public class GetDocumentsRequest : SearchQueryParams
    {
        public Guid? UserId { get; set; }
        public Guid? MerchantId { get; set; }
        public Guid? SubMerchantId { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? DocumentTypeId { get; set; }
        public bool OnlyLatest { get; set; }
    }
}
