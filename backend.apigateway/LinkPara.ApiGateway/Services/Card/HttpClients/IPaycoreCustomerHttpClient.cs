
using LinkPara.ApiGateway.Services.Card.Models.PaycoreCustomer.Request;
using LinkPara.ApiGateway.Services.Card.Models.PaycoreCustomer.Response;
using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.HttpClients;

public interface IPaycoreCustomerHttpClient
{
    Task<PaycoreResponse> CreateCustomerAsync(CreateCustomerRequest request);
    Task<GetCustomerInformationResponse> GetCustomerInformationAsync(GetCustomerInformationRequest request);
    Task<List<GetCustomerCardsResponse>> GetCustomerCardsAsync(GetCustomerCardsRequest request);
    Task<List<GetCustomerLimitInfoResponse>> GetCustomerLimitInfoAsync(GetCustomerLimitInfoRequest request);
    Task<PaycoreResponse> UpdateCustomerAsync(UpdateCustomerRequest request);
    Task<PaycoreResponse> UpdateCustomerCommunicationAsync(UpdateCustomerCommunicationRequest request);
    Task<PaycoreResponse> UpdateCustomerAddressAsync(UpdateCustomerAddressRequest request);
    Task<PaycoreResponse> UpdateCustomerLimitAsync(UpdateCustomerLimitRequest request);
}