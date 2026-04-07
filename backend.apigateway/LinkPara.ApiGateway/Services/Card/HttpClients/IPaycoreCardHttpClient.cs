
using LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Request;
using LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Response;
using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.HttpClients;

public interface IPaycoreCardHttpClient
{
    Task<PaycoreResponse> CreateCardAsync(CreateCardRequest request);
    Task<UpdateCardStatusResponse> UpdateCardStatusAsync(UpdateCardStatusRequest request);
    Task<GetCardAuthorizationsResponse> GetCardAuthorizationsAsync(GetCardAuthorizationsRequest request);
    Task<List<GetCardInformationsResponse>> GetCardInformationsAsync(GetCardInformationsRequest request);
    Task<PaycoreResponse> UpdateCardAuthorizationsAsync(UpdateCardAuthorizationRequest request);
    Task<GetCardTransactionsResponse> GetCardTransactionsAsync(GetCardTransactionsRequest request);
    Task<GetCardLastCourierActivityResponse> GetCardLastCourierActivityAsync(GetCardLastCourierActivityRequest request);
    Task<AddAdditionalLimitRestrictionResponse> AddAdditionalLimitRestrictionAsync(AddAdditionalLimitRestrictionRequest request);
    Task<GetCardSensitiveDataResponse> GetCardSensitiveDataAsync(GetCardSensitiveDataRequest request);
}