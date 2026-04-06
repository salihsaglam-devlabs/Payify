using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;

public interface ICardBinHttpClient
{
    Task<CardBinDto> GetByIdAsync(Guid id);
    Task<PaginatedList<CardBinDto>> GetAllAsync(GetAllCardBinRequest request);
    Task SaveAsync(SaveCardBinRequest request);
    Task UpdateAsync(UpdateCardBinRequest request);
    Task DeleteCardBinAsync(Guid id);
    Task<UpdateCardBinRequest> PatchAsync(Guid id, JsonPatchDocument<UpdateCardBinRequest> cardBinPatch);
}
