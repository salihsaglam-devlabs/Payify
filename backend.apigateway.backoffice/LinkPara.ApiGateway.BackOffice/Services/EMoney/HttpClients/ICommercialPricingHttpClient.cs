using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface ICommercialPricingHttpClient
{ 
    Task<PaginatedList<PricingCommercialDto>> GetAll(CommercialPricingFilterRequest request);
    Task CreatePricingCommercial(SaveCommercialPricingRequest request);
    Task UpdatePricingCommercial(UpdateCommercialPricingRequest request);
    Task DeletePricingCommercial(Guid id);
}